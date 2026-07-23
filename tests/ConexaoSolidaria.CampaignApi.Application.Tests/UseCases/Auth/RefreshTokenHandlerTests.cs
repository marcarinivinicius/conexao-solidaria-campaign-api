using ConexaoSolidaria.CampaignApi.Application.Tests.Fakes;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Auth.Refresh;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using ConexaoSolidaria.CampaignApi.Domain.Exceptions;
using Xunit;

namespace ConexaoSolidaria.CampaignApi.Application.Tests.UseCases.Auth;

public class RefreshTokenHandlerTests
{
    private static readonly DateTimeOffset Agora = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private static (RefreshTokenHandler Handler, FakeRefreshTokenRepository Repository, FakeRefreshTokenGenerator Generator, FakeTimeProvider TimeProvider)
        CriarHandler()
    {
        var repository = new FakeRefreshTokenRepository();
        var generator = new FakeRefreshTokenGenerator();
        var timeProvider = new FakeTimeProvider(Agora);
        var tokenIssuer = new FakeTokenIssuer(generator, repository, timeProvider);
        var unitOfWork = new FakeUnitOfWork();

        var handler = new RefreshTokenHandler(repository, generator, tokenIssuer, unitOfWork, timeProvider);

        return (handler, repository, generator, timeProvider);
    }

    [Fact]
    public async Task Handle_ComTokenValido_DeveEmitirNovoParERevogarAntigo()
    {
        var (handler, repository, generator, timeProvider) = CriarHandler();
        var usuarioId = Guid.NewGuid();

        var (rawToken, tokenHash) = generator.Gerar();
        var tokenValido = RefreshToken.Criar(usuarioId, "Doador", "joao@exemplo.com", tokenHash, Agora.UtcDateTime, TimeSpan.FromDays(7));
        await repository.AdicionarAsync(tokenValido, CancellationToken.None);

        var result = await handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        Assert.NotEqual(rawToken, result.RefreshToken);
        Assert.Equal("Doador", result.Role);

        var antigoAtualizado = repository.Tokens.First(t => t.Id == tokenValido.Id);
        Assert.NotNull(antigoAtualizado.RevogadoEm);
        Assert.NotNull(antigoAtualizado.SubstituidoPorId);

        var novo = repository.Tokens.First(t => t.Id == antigoAtualizado.SubstituidoPorId);
        Assert.True(novo.EstaValido(Agora.UtcDateTime));
    }

    [Fact]
    public async Task Handle_ComTokenDesconhecido_DeveLancarInvalidRefreshTokenException()
    {
        var (handler, _, _, _) = CriarHandler();

        await Assert.ThrowsAsync<InvalidRefreshTokenException>(
            () => handler.Handle(new RefreshTokenCommand("token-que-nao-existe"), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ComTokenExpirado_DeveLancarRefreshTokenExpiredException()
    {
        var (handler, repository, generator, timeProvider) = CriarHandler();
        var usuarioId = Guid.NewGuid();
        var (rawToken, tokenHash) = generator.Gerar();
        var expirado = RefreshToken.Criar(usuarioId, "Doador", "joao@exemplo.com", tokenHash, Agora.UtcDateTime, TimeSpan.FromDays(7));
        await repository.AdicionarAsync(expirado, CancellationToken.None);

        timeProvider.Agora = Agora.AddDays(8);

        await Assert.ThrowsAsync<RefreshTokenExpiredException>(
            () => handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ComTokenReusado_DeveRevogarTodosOsAtivosDoUsuarioELancarExcecao()
    {
        var (handler, repository, generator, timeProvider) = CriarHandler();
        var usuarioId = Guid.NewGuid();

        var (rawTokenA, hashA) = generator.Gerar();
        var tokenA = RefreshToken.Criar(usuarioId, "Doador", "joao@exemplo.com", hashA, Agora.UtcDateTime, TimeSpan.FromDays(7));
        await repository.AdicionarAsync(tokenA, CancellationToken.None);

        var (_, hashB) = generator.Gerar();
        var tokenB = RefreshToken.Criar(usuarioId, "Doador", "joao@exemplo.com", hashB, Agora.UtcDateTime, TimeSpan.FromDays(7));
        await repository.AdicionarAsync(tokenB, CancellationToken.None);

        // Usa o token A uma vez (rotaciona, revoga A).
        await handler.Handle(new RefreshTokenCommand(rawTokenA), CancellationToken.None);

        // Reapresenta o token A (ja revogado) - reuso detectado.
        await Assert.ThrowsAsync<RefreshTokenReusedException>(
            () => handler.Handle(new RefreshTokenCommand(rawTokenA), CancellationToken.None));

        var tokenBAtualizado = repository.Tokens.First(t => t.Id == tokenB.Id);
        Assert.NotNull(tokenBAtualizado.RevogadoEm);
    }

    [Fact]
    public async Task Handle_ComSuperAdmin_DeveFuncionarIgualSemCasoEspecial()
    {
        var (handler, repository, generator, timeProvider) = CriarHandler();
        var (rawToken, tokenHash) = generator.Gerar();
        var token = RefreshToken.Criar(Guid.Empty, "SuperAdmin", "superadmin@conexaosolidaria.org.br", tokenHash, Agora.UtcDateTime, TimeSpan.FromDays(7));
        await repository.AdicionarAsync(token, CancellationToken.None);

        var result = await handler.Handle(new RefreshTokenCommand(rawToken), CancellationToken.None);

        Assert.Equal("SuperAdmin", result.Role);
    }
}

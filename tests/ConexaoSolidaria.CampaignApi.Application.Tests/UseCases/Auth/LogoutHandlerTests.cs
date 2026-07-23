using ConexaoSolidaria.CampaignApi.Application.Tests.Fakes;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Auth.Logout;
using ConexaoSolidaria.CampaignApi.Domain.Entities;
using Xunit;

namespace ConexaoSolidaria.CampaignApi.Application.Tests.UseCases.Auth;

public class LogoutHandlerTests
{
    private static readonly DateTimeOffset Agora = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Handle_ComTokenValido_DeveRevogar()
    {
        var repository = new FakeRefreshTokenRepository();
        var generator = new FakeRefreshTokenGenerator();
        var timeProvider = new FakeTimeProvider(Agora);
        var handler = new LogoutHandler(repository, generator, new FakeUnitOfWork(), timeProvider);

        var (rawToken, tokenHash) = generator.Gerar();
        var token = RefreshToken.Criar(Guid.NewGuid(), "Doador", "joao@exemplo.com", tokenHash, Agora.UtcDateTime, TimeSpan.FromDays(7));
        await repository.AdicionarAsync(token, CancellationToken.None);

        await handler.Handle(new LogoutCommand(rawToken), CancellationToken.None);

        Assert.NotNull(repository.Tokens.First(t => t.Id == token.Id).RevogadoEm);
    }

    [Fact]
    public async Task Handle_ChamadoDuasVezes_DeveSerIdempotente()
    {
        var repository = new FakeRefreshTokenRepository();
        var generator = new FakeRefreshTokenGenerator();
        var timeProvider = new FakeTimeProvider(Agora);
        var handler = new LogoutHandler(repository, generator, new FakeUnitOfWork(), timeProvider);

        var (rawToken, tokenHash) = generator.Gerar();
        var token = RefreshToken.Criar(Guid.NewGuid(), "Doador", "joao@exemplo.com", tokenHash, Agora.UtcDateTime, TimeSpan.FromDays(7));
        await repository.AdicionarAsync(token, CancellationToken.None);

        await handler.Handle(new LogoutCommand(rawToken), CancellationToken.None);
        var exception = await Record.ExceptionAsync(() => handler.Handle(new LogoutCommand(rawToken), CancellationToken.None));

        Assert.Null(exception);
    }

    [Fact]
    public async Task Handle_ComTokenDesconhecido_NaoDeveLancar()
    {
        var repository = new FakeRefreshTokenRepository();
        var generator = new FakeRefreshTokenGenerator();
        var handler = new LogoutHandler(repository, generator, new FakeUnitOfWork(), new FakeTimeProvider(Agora));

        var exception = await Record.ExceptionAsync(() => handler.Handle(new LogoutCommand("token-desconhecido"), CancellationToken.None));

        Assert.Null(exception);
    }
}

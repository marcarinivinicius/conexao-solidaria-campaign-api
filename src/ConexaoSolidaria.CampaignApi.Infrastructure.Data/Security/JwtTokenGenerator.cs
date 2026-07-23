using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ConexaoSolidaria.CampaignApi.Application.Abstractions;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ConexaoSolidaria.CampaignApi.Infrastructure.Data.Security;

public class JwtTokenGenerator(IOptions<JwtOptions> options, TimeProvider timeProvider) : IJwtTokenGenerator
{
    private readonly JwtOptions _options = options.Value;

    public string GerarToken(Guid usuarioId, string email, string role)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuarioId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_options.ExpirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

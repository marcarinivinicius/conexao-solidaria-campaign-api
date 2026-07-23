using System.Text;
using ConexaoSolidaria.CampaignApi.Api.Middlewares;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Prometheus;
using Scalar.AspNetCore;
using System.Threading.RateLimiting;

namespace ConexaoSolidaria.CampaignApi.Api.Configuration;

public static class ApiConfiguration
{
    private const string PlaceholderSecretPrefix = "troque-esta-chave";

    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Secao Jwt nao configurada.");

        if (!environment.IsDevelopment() && jwtOptions.SecretKey.StartsWith(PlaceholderSecretPrefix))
            throw new InvalidOperationException(
                "Jwt:SecretKey ainda esta no valor placeholder. Configure a variavel de ambiente Jwt__SecretKey.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey))
                };
            });

        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "Conexao Solidaria - Campaign API", Version = "v1" });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Informe: Bearer {seu token JWT}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    // Login/register/refresh sao alvo natural de forca bruta anonima -
    // register/gestor fica de fora (ja exige [Authorize(Roles=SuperAdmin)]).
    public const string AuthRateLimitPolicy = "auth";

    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddPolicy(AuthRateLimitPolicy, httpContext => RateLimitPartition.GetFixedWindowLimiter(
                httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 5,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                }));
        });

        return services;
    }

    // Sem AllowCredentials() de proposito: a API autentica via Bearer no
    // header, nao cookie, entao o risco principal que CORS mitiga
    // (autenticacao cross-site via cookie) nao se aplica do mesmo jeito aqui.
    public static IServiceCollection AddCorsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];

        services.AddCors(options =>
        {
            options.AddPolicy("Default", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .WithMethods("GET", "POST", "PUT", "DELETE");
            });
        });

        return services;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (!app.Environment.IsDevelopment())
            app.UseHsts();

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
            context.Response.Headers.Append("X-Frame-Options", "DENY");
            context.Response.Headers.Append("Referrer-Policy", "no-referrer");
            await next();
        });

        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapScalarApiReference(options => options
            .WithTitle("Conexao Solidaria - Campaign API")
            .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json"));

        app.UseHttpMetrics();

        app.UseCors("Default");
        app.UseRateLimiter();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");
        app.MapMetrics("/metrics");

        return app;
    }
}

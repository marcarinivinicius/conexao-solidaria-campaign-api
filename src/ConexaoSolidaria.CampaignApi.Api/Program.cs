using ConexaoSolidaria.CampaignApi.Api.Configuration;
using ConexaoSolidaria.CampaignApi.Application.Behaviors;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.CriarCampanha;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddValidatorsFromAssembly(typeof(CriarCampanhaCommand).Assembly);
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(CriarCampanhaCommand).Assembly);
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddJwtAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddRateLimiting();
builder.Services.AddCorsConfiguration(builder.Configuration);
builder.Services.AddSwaggerConfiguration();
builder.Services.AddHealthChecks();

builder.Services.AddInfrastructureData(builder.Configuration);
builder.Services.AddPostgresqlData(builder.Configuration);

builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

app.UseApiPipeline();

app.Services.ApplyMigrations();

await app.RunAsync();

public partial class Program;

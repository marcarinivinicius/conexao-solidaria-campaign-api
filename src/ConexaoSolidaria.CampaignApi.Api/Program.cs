using ConexaoSolidaria.CampaignApi.Api.Configuration;
using ConexaoSolidaria.CampaignApi.Application.UseCases.Campanhas.CriarCampanha;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data;
using ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CriarCampanhaCommand).Assembly));

builder.Services.AddJwtAuthentication(builder.Configuration);
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

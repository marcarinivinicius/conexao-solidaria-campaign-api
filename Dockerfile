FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY nuget.config .
COPY *.sln .
COPY src/ConexaoSolidaria.CampaignApi.Domain/*.csproj src/ConexaoSolidaria.CampaignApi.Domain/
COPY src/ConexaoSolidaria.CampaignApi.Application/*.csproj src/ConexaoSolidaria.CampaignApi.Application/
COPY src/ConexaoSolidaria.CampaignApi.Infrastructure.Data/*.csproj src/ConexaoSolidaria.CampaignApi.Infrastructure.Data/
COPY src/ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql/*.csproj src/ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql/
COPY src/ConexaoSolidaria.CampaignApi.Api/*.csproj src/ConexaoSolidaria.CampaignApi.Api/

# O pacote ConexaoSolidaria.Shared.RabbitMq vem do GitHub Packages, que exige
# auth mesmo pra leitura publica. O token entra via BuildKit secret (nao fica
# em nenhuma camada da imagem) - localmente, exporte GITHUB_PACKAGES_TOKEN e
# rode com `docker build --secret id=nuget_token,env=GITHUB_PACKAGES_TOKEN`.
RUN --mount=type=secret,id=nuget_token \
    dotnet nuget update source conexaoSolidaria \
      --username marcarinivinicius \
      --password "$(cat /run/secrets/nuget_token)" \
      --store-password-in-clear-text \
    && dotnet restore src/ConexaoSolidaria.CampaignApi.Api

COPY src/ src/
RUN dotnet publish src/ConexaoSolidaria.CampaignApi.Api -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "ConexaoSolidaria.CampaignApi.Api.dll"]

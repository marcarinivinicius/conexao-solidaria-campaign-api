# conexao-solidaria-campaign-api

API do hackathon "Conexão Solidária": autenticação JWT com RBAC
(`GestorONG` / `Doador`), gestão de campanhas, cadastro de doadores, painel
público de transparência e recebimento de doações (publicando evento
assíncrono no RabbitMQ — quem atualiza o valor arrecadado é o
[`conexao-solidaria-donation-worker`](https://github.com/marcarinivinicius/conexao-solidaria-donation-worker),
não esta API).

## Arquitetura (Clean Architecture)

```
src/
  ConexaoSolidaria.CampaignApi.Domain                    -> entidades e regras de negocio (Campanha, Doador)
  ConexaoSolidaria.CampaignApi.Application                -> use cases (MediatR), abstracoes
  ConexaoSolidaria.CampaignApi.Infrastructure.Data         -> JWT, hash de senha, publisher RabbitMQ
  ConexaoSolidaria.CampaignApi.Infrastructure.Data.Postgresql -> EF Core, repositorios, migrations
  ConexaoSolidaria.CampaignApi.Api                         -> controllers, auth, swagger, health/metrics
```

Depende do pacote [`ConexaoSolidaria.Shared.RabbitMq`](https://github.com/marcarinivinicius/conexao-solidaria-shared)
(GitHub Packages) para publicar o evento `DoacaoRecebidaEvent`.

## Rodando localmente

**Pré-requisito**: o [`conexao-solidaria-infra`](https://github.com/marcarinivinicius/conexao-solidaria-infra)
precisa estar de pé primeiro (Postgres + RabbitMQ), via `docker compose up -d`
naquele repo — ele cria a network `conexao-solidaria-net` usada abaixo.

### Autenticação no feed de pacotes (uma vez só)

O pacote `ConexaoSolidaria.Shared.RabbitMq` vem do GitHub Packages, que
exige autenticação mesmo para pacotes públicos. Gere um Personal Access
Token pessoal (escopo `read:packages`) e exporte:

```bash
export GITHUB_PACKAGES_TOKEN=ghp_xxx  # nunca commite isso
dotnet nuget update source github-conexao-solidaria \
  --username marcarinivinicius \
  --password $GITHUB_PACKAGES_TOKEN \
  --store-password-in-clear-text
```

### Opção 1 — `dotnet run`

```bash
dotnet restore
dotnet run --project src/ConexaoSolidaria.CampaignApi.Api
```

A API aplica as migrations do EF Core automaticamente no startup. Swagger
em `http://localhost:5000/swagger` (ou a porta que o Kestrel imprimir).

### Opção 2 — Docker Compose

```bash
docker compose build --secret id=nuget_token,env=GITHUB_PACKAGES_TOKEN
docker compose up -d
```

API em `http://localhost:8081`.

## Autenticação

| Role | Como logar |
|---|---|
| `Doador` | `POST /api/v1/doadores` (cadastro) e depois `POST /api/v1/doadores/login` |
| `GestorONG` | `POST /api/v1/auth/gestor/login` com a credencial seedada em `appsettings.json` (`GestorOng` section) — default de dev: `gestor@conexaosolidaria.org.br` / `TrocarSenha123!` |

Use o token retornado no header `Authorization: Bearer <token>`.

## Endpoints principais

| Método | Rota | Acesso |
|---|---|---|
| `GET` | `/api/v1/campanhas` | Público — painel de transparência (só campanhas `Ativa`) |
| `POST` | `/api/v1/campanhas` | `GestorONG` |
| `PUT` | `/api/v1/campanhas/{id}` | `GestorONG` |
| `POST` | `/api/v1/doadores` | Público — cadastro |
| `POST` | `/api/v1/doadores/login` | Público |
| `POST` | `/api/v1/doacoes` | `Doador` — publica `DoacaoRecebidaEvent`, não atualiza o total direto |
| `GET` | `/health` | Público |
| `GET` | `/metrics` | Público (formato Prometheus, coletado pelo Zabbix agent2 do `conexao-solidaria-infra`) |

## Testes

```bash
dotnet test
```

Cobertura focada no domínio (`Campanha`/`Doador`), onde ficam as regras de
negócio do desafio (meta financeira > 0, data de término não pode estar no
passado, CPF válido).

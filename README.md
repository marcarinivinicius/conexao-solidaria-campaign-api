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
em `http://localhost:5000/swagger`, e uma UI alternativa (Scalar, lê o
mesmo `swagger.json`) em `http://localhost:5000/scalar/v1` (ou a porta
que o Kestrel imprimir).

### Opção 2 — Docker Compose

```bash
docker compose build --secret id=nuget_token,env=GITHUB_PACKAGES_TOKEN
docker compose up -d
```

API em `http://localhost:8081` — Swagger em `/swagger`, Scalar em
`/scalar/v1`.

## Autenticação

Login **único** pra qualquer role — `POST /api/v1/auth/login` com
`{ "email": "...", "senha": "..." }`. O backend resolve sozinho quem é
quem (tenta `SuperAdmin` → `Gestor` → `Doador`, nessa ordem) e devolve
`{ "token": "...", "role": "..." }`.

Cadastro: `/api/v1/auth/register` é o registro padrão, público, de
doador. Gestor tem rota própria porque exige autenticação de
`SuperAdmin`:

| Role | Como existe | Cadastro | Login |
|---|---|---|---|
| `SuperAdmin` | Credencial única seedada em `appsettings.json` (seção `SuperAdmin`) — default de dev: `superadmin@conexaosolidaria.org.br` / `TrocarSenha123!` | — | `POST /api/v1/auth/login` |
| `GestorONG` | Cadastrado por quem já é `SuperAdmin` | `POST /api/v1/auth/register/gestor` (autenticado como `SuperAdmin`) | `POST /api/v1/auth/login` |
| `Doador` | Auto-cadastro público | `POST /api/v1/auth/register` (anônimo) | `POST /api/v1/auth/login` |

Use o token retornado no header `Authorization: Bearer <token>`.

## Endpoints principais

| Método | Rota | Acesso |
|---|---|---|
| `POST` | `/api/v1/auth/login` | Público — login único (SuperAdmin/GestorONG/Doador) |
| `POST` | `/api/v1/auth/register` | Público — auto-cadastro de doador |
| `POST` | `/api/v1/auth/register/gestor` | `SuperAdmin` — cadastro de gestor da ONG |
| `GET` | `/api/v1/campanhas` | Público — painel de transparência (só campanhas `Ativa`) |
| `POST` | `/api/v1/campanhas` | `GestorONG` |
| `PUT` | `/api/v1/campanhas/{id}` | `GestorONG` |
| `POST` | `/api/v1/doacoes` | `Doador` — publica `DoacaoRecebidaEvent`, não atualiza o total direto |
| `GET` | `/health` | Público |
| `GET` | `/metrics` | Público (formato Prometheus) |

## Postman

Collection completa em [`postman/ConexaoSolidaria.postman_collection.json`](postman/ConexaoSolidaria.postman_collection.json)
— login, cadastro de doador, CRUD de campanhas e doação, com os tokens
capturados automaticamente entre as requisições (rode as pastas na ordem
1 → 4). `baseUrl` default é `http://localhost:8081`.

## Deploy (GitOps)

Não existe mais `kubectl apply -k k8s/` neste repo. O deploy em Kubernetes
é feito pelo [`conexao-solidaria-infra`](https://github.com/marcarinivinicius/conexao-solidaria-infra),
que também guarda os manifests (`Rollout` com canary, `Ingress`,
`Service`).

O CI tem duas partes com gatilhos diferentes:

| Gatilho | O que roda |
|---|---|
| Push em `main` ou Pull Request | Só `build-test` (build + `dotnet test`) — **nenhuma imagem é criada** |
| Push de uma tag `v*` (ex.: `v1.2.0`) | `build-test` + build/push da imagem em `ghcr.io/marcarinivinicius/conexao-solidaria-campaign-api:<tag>` + abre PR em `conexao-solidaria-infra` bumpando pra essa tag |

Ou seja: imagem só existe quando alguém decide cortar uma versão
(`git tag v1.2.0 && git push origin v1.2.0`) — commits normais na `main`
não geram deploy nenhum. Quem promove a nova imagem pro cluster depois do
PR mergeado é o ArgoCD + Argo Rollouts.

```bash
git tag v1.0.0
git push origin v1.0.0
```

Requer o secret `INFRA_REPO_TOKEN` (Personal Access Token com escrita em
`conexao-solidaria-infra`) configurado neste repo em *Settings → Secrets
and variables → Actions*. Sem ele, o job `deploy` do CI falha com erro
claro na etapa de clone/push — não afeta build nem testes.

## Testes

```bash
dotnet test
```

Cobertura focada no domínio (`Campanha`/`Doador`), onde ficam as regras de
negócio do desafio (meta financeira > 0, data de término não pode estar no
passado, CPF válido).

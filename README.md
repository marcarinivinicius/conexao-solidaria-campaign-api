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
dotnet nuget update source conexaoSolidaria \
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
`{ "token": "...", "refreshToken": "...", "role": "..." }`.

`token` é um JWT de vida curta (`Jwt:ExpirationMinutes`, default **15
min**). `refreshToken` é um segredo opaco (não é JWT) de vida longa
(`RefreshToken:ExpirationDays`, default **7 dias**) — troque por um par
novo em `POST /api/v1/auth/refresh` com `{ "refreshToken": "..." }`. Cada
uso **rotaciona**: o refresh token usado é invalidado e um novo é
devolvido junto com o access token novo. Se um refresh token já usado for
apresentado de novo (replay/roubo), a API revoga **toda** a sessão daquele
usuário e devolve `401`. `POST /api/v1/auth/logout` com o mesmo corpo
revoga o refresh token (idempotente, sempre `204`).

Cadastro: `/api/v1/auth/register` é o registro padrão, público, de
doador — já devolve `{ "id": "...", "token": "...", "refreshToken": "...",
"role": "Doador" }`, sem precisar chamar `/auth/login` em seguida. Gestor
tem rota própria porque exige autenticação de `SuperAdmin` ou `GestorONG`
(por isso não devolve token de outra pessoa pra quem chamou):

| Role | Como existe | Cadastro | Login |
|---|---|---|---|
| `SuperAdmin` | Credencial única seedada em `appsettings.json` (seção `SuperAdmin`) — default de dev: `superadmin@conexaosolidaria.org.br` / `TrocarSenha123!` | — | `POST /api/v1/auth/login` |
| `GestorONG` | Cadastrado por quem já é `SuperAdmin` ou `GestorONG` | `POST /api/v1/auth/register/gestor` (autenticado como `SuperAdmin` ou `GestorONG`) | `POST /api/v1/auth/login` |
| `Doador` | Auto-cadastro público | `POST /api/v1/auth/register` (anônimo) | `POST /api/v1/auth/login` |

Use o token retornado no header `Authorization: Bearer <token>`.
`login`, `register` e `refresh` têm rate limit de 5 requisições/minuto por
IP (`429` acima disso).

## Endpoints principais

| Método | Rota | Acesso |
|---|---|---|
| `POST` | `/api/v1/auth/login` | Público — login único (SuperAdmin/GestorONG/Doador) |
| `POST` | `/api/v1/auth/refresh` | Público — troca refresh token por par novo (rotação) |
| `POST` | `/api/v1/auth/logout` | Público — revoga o refresh token |
| `POST` | `/api/v1/auth/register` | Público — auto-cadastro de doador |
| `POST` | `/api/v1/auth/register/gestor` | `SuperAdmin` ou `GestorONG` — cadastro de gestor da ONG |
| `GET` | `/api/v1/campanhas` | Público — painel de transparência (só campanhas `Ativa`) |
| `POST` | `/api/v1/campanhas` | `GestorONG` — `dataInicio`/`dataFim` em ISO 8601 (`"2026-07-24"`), não `dd/MM/yyyy` |
| `PUT` | `/api/v1/campanhas/{id}` | `GestorONG` |
| `POST` | `/api/v1/doacoes` | `Doador` — publica `DoacaoRecebidaEvent`, não atualiza o total direto. Header opcional `Idempotency-Key: <guid>` evita duplicar a doação em retry |
| `GET` | `/health` | Público |
| `GET` | `/metrics` | Público (formato Prometheus) |

## Postman

Collection completa em [`postman/ConexaoSolidaria.postman_collection.json`](postman/ConexaoSolidaria.postman_collection.json)
— login, cadastro de doador (com refresh/logout), cadastro de gestor, CRUD
de campanhas, doação (com demo de `Idempotency-Key`) e observabilidade,
com os tokens capturados automaticamente entre as requisições (rode as
pastas na ordem 1 → 5). `baseUrl` default é `http://localhost:8081`.

## Deploy (GitOps)

Não existe mais `kubectl apply -k k8s/` neste repo. O deploy em Kubernetes
é feito pelo [`conexao-solidaria-infra`](https://github.com/marcarinivinicius/conexao-solidaria-infra),
que também guarda os manifests (`Rollout` com canary, `Ingress`,
`Service`).

O CI tem três partes com gatilhos diferentes:

| Gatilho | O que roda |
|---|---|
| Pull Request | Só `build-test` (build + `dotnet test`) — nenhuma imagem é criada |
| Push em `main` | `build-test` + build/push da imagem em `ghcr.io/marcarinivinicius/conexao-solidaria-campaign-api:main-<sha curto>` — **nenhum PR de deploy é aberto** |
| Push de uma tag `v*` (ex.: `v1.2.0`) | `build-test` + build/push da imagem com a tag de versão (`:v1.2.0`, nunca `:latest`) + abre PR em `conexao-solidaria-infra` bumpando pra essa tag |

Ou seja: toda imagem de commit fica rastreável no GHCR (satisfaz "gerar a
imagem Docker a cada push na main"), mas só uma tag de versão (`git tag
v1.2.0 && git push origin v1.2.0`) dispara o PR de deploy — commits
normais na `main` não promovem nada pro cluster. Quem promove a nova
imagem pro cluster depois do PR mergeado é o ArgoCD + Argo Rollouts.

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

Dois projetos: `Domain.Tests` (regras de negócio — meta financeira > 0,
data de término não pode estar no passado, CPF válido) e
`Application.Tests` (handlers de auth com fakes em memória — rotação de
refresh token, expiração, reuso detectado revogando a sessão inteira,
logout idempotente, caso SuperAdmin).

## Validação, rate limiting e idempotência

- **Validação de entrada**: FluentValidation + pipeline do MediatR em
  todos os commands — request malformado retorna `400` com
  `{ "message": "Dados invalidos.", "errors": [{ "propertyName", "errorMessage" }] }`
  antes de chegar no domínio/banco.
- **Rate limiting**: 5 requisições/minuto por IP em `login`/`register`/`refresh`
  (`429` acima disso).
- **CORS**: policy `Default`, origens lidas de `Cors:AllowedOrigins`
  (vazio por padrão — configure por ambiente).
- **Idempotência de doação**: ver header `Idempotency-Key` na tabela de
  endpoints acima. Protege contra retry duplicado do cliente; o
  `donation-worker` também se protege contra redelivery do RabbitMQ do
  lado dele (ver README daquele repo).

# fiap-app

Aplicação principal (**API .NET**, Clean Architecture) da oficina, executada no **Kubernetes** e observada via **New Relic**. Um dos 4 repositórios do Tech Challenge — Fase 3 (SOAT/FIAP).

Cobre o ciclo completo da **Ordem de Serviço** (recepção → orçamento → execução → entrega), gestão de clientes/veículos/serviços/peças e controle de estoque. Rotas sensíveis são protegidas por **JWT** (o token do cliente é emitido pela `fiap-auth-lambda` via CPF).

## Tecnologias
- **.NET 10 / ASP.NET Core** — Clean Architecture (Domain / Application / Infrastructure / Api)
- **EF Core + Npgsql** → **PostgreSQL gerenciado** (Railway, `fiap-infra-db`)
- **JWT Bearer** (HS256) — mesmo `issuer/audience/secret` da Lambda
- **Serilog** — logs estruturados JSON com correlação (`trace.id`/`span.id`)
- **New Relic** — APM (agent .NET) + logs + dashboards
- **Docker** + **Kubernetes** (HPA), Swagger/OpenAPI, FluentValidation, xUnit

## Arquitetura de componentes

```mermaid
flowchart TB
  subgraph API["Oficina.Api (ASP.NET Core)"]
    MW[Middlewares\nErrorHandling • Serilog • JWT]
    CTRL[Controllers\nClientes • Veiculos • Servicos • Pecas • OrdensServico • Auth]
    HC[HealthChecks\n/health/live • /health/ready]
  end
  subgraph APP["Oficina.Application (casos de uso)"]
    UC[UseCases + DTOs + Validators + Mapping]
  end
  subgraph DOM["Oficina.Domain (regras de negocio)"]
    ENT[Entidades + Value Objects\nCliente • Veiculo • OrdemDeServico • Estoque • Cpf/Cnpj]
  end
  subgraph INFRA["Oficina.Infrastructure"]
    REPO[Repositories + UnitOfWork\nEF Core / Npgsql]
    AUTH[JWT + PasswordHasher]
    NOTIF[Notificacoes]
  end
  CTRL --> UC --> ENT
  UC --> REPO --> DB[(PostgreSQL\ngerenciado - Railway)]
  MW -. valida .-> JWT{{JWT HS256\nOficina.Api}}
  API -. APM + logs .-> NR[(New Relic)]
  REPO --- AUTH
```

Dependências apontam **para dentro** (Api → Application → Domain); a Infrastructure implementa as interfaces do Domain (inversão de dependência).

## Observabilidade
- **APM:** New Relic .NET agent embutido na imagem (ativado por `CORECLR_*` + `NEW_RELIC_LICENSE_KEY` em runtime).
- **Logs JSON correlacionados:** Serilog `CompactJsonFormatter`; cada evento tem `trace.id`/`span.id` (e `@tr`/`@sp` no request log) para correlação com os traces do APM.
- **Health:** `/health/live` (liveness) e `/health/ready` (readiness, checa o Postgres) — usados pelas probes do K8s.

## Execução local

### Docker Compose (app + Postgres local)
```bash
cp .env.example .env    # ajuste as variáveis
docker compose up --build
```

### Kubernetes (imagem local + fiap-infra-k8s)
```bash
docker build -t fiap-app:local .          # constrói a imagem que o cluster espera
kubectl rollout restart deployment/fiap-app -n oficina
kubectl port-forward -n oficina svc/fiap-app 8080:80
# http://localhost:8080  (Swagger na raiz)
```

### Variáveis principais
| Variável | Descrição |
|---|---|
| `ConnectionStrings__Postgres` | conexão Npgsql ao banco gerenciado (Railway) |
| `Jwt__Secret` | segredo HS256 (**igual** ao da Lambda) |
| `NEW_RELIC_LICENSE_KEY` | license key do New Relic (via Secret) |

## API / Swagger
- Swagger UI na **raiz** (`/`) e OpenAPI em `/swagger/v1/swagger.json`.
- Coleção OpenAPI versionada em [`docs/openapi.json`](docs/openapi.json).

## Testes
```bash
dotnet test Oficina.slnx -c Release      # 102 testes (domínio + aplicação + API)
```

## Deploy em nuvem (Railway)

Além do Kubernetes local, a API roda **em nuvem real** no **Railway** (mesmo projeto `fiap-fase3`
do banco), como **container** construído a partir deste repositório (autodeploy a cada push na `main`).

- **URL pública:** https://fiap-app-production.up.railway.app (Swagger na raiz).
- **Banco:** conecta ao Postgres gerenciado pela **rede privada** do Railway
  (`postgres.railway.internal:5432`), sem passar pela internet.
- **Variáveis** (Railway → serviço `fiap-app`): `ConnectionStrings__Postgres` (com referência
  `${{Postgres.PGPASSWORD}}`), `Jwt__Secret` (segredo HS256 canônico), `ASPNETCORE_ENVIRONMENT=Production`.
- **Health:** `/health/live` e `/health/ready` (este confirma a conexão com o Postgres).

> **Arquitetura dual:** o **Railway** é o deploy de **nuvem** (produção); o **Kubernetes**
> (`fiap-infra-k8s`, Docker Desktop) permanece como o **cluster escalável com HPA** exigido pelo
> enunciado. As duas formas usam a mesma imagem/o mesmo banco.

## CI/CD (`.github/workflows/ci.yml`)
- **PR/push:** restore, build, **test + coverage**, e **docker build** da imagem.
- O deploy no cluster é responsabilidade do `fiap-infra-k8s` (Docker Desktop local). Para nuvem: push da imagem para um registry (GHCR/ECR) e rollout via credenciais.

## O sistema completo (4 repositórios)

Este repositório é a **aplicação**. A solução da Fase 3 é composta por 4 repos independentes:

| Repositório | Papel |
|---|---|
| **[fiap-auth-lambda](https://github.com/MathboyL3/fiap-auth-lambda)** | Autenticação por CPF → JWT (API Gateway + Lambda / LocalStack) |
| **[fiap-app](https://github.com/MathboyL3/fiap-app)** (este) | API principal da oficina (.NET / Kubernetes) |
| **[fiap-infra-k8s](https://github.com/MathboyL3/fiap-infra-k8s)** | Infra do cluster (Terraform: deployment, HPA, Ingress, secrets) |
| **[fiap-infra-db](https://github.com/MathboyL3/fiap-infra-db)** | Banco de dados gerenciado (Terraform + PostgreSQL no Railway) |

> **Visão de arquitetura, diagrama de componentes (cloud) e diagramas de sequência
> (autenticação CPF→JWT e abertura de OS):** [`docs/ARQUITETURA.md`](docs/ARQUITETURA.md).
> **RFCs** (decisões do sistema): [`docs/rfc/`](docs/rfc/).

## Documentação
- [`docs/ARQUITETURA.md`](docs/ARQUITETURA.md) — arquitetura, diagrama de componentes e sequência.
- [`docs/rfc/`](docs/rfc/) — RFCs (decisões de arquitetura do sistema).
- [`docs/adr/0001-observabilidade-newrelic.md`](docs/adr/0001-observabilidade-newrelic.md)

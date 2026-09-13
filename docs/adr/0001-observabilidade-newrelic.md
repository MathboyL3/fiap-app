# ADR-0001 — Observabilidade com New Relic + logs estruturados

- **Status:** Aceito
- **Data:** Fase 3
- **Decisores:** Equipe Tech Challenge

## Contexto
A Fase 3 exige **observabilidade**: latência, uso de CPU/memória, healthchecks, alertas e **logs estruturados com correlação**, além de dashboards de negócio. O enunciado cita Datadog/New Relic.

## Decisão
- **New Relic (free tier)** como plataforma de observabilidade:
  - **APM** via **agent .NET** embutido na imagem Docker (ativado por `CORECLR_*` + `NEW_RELIC_LICENSE_KEY`).
  - **Logs estruturados JSON** com **Serilog** (`CompactJsonFormatter`), enriquecidos com `trace.id`/`span.id` para correlação log↔trace.
  - **Healthchecks** `/health/live` e `/health/ready` (readiness checa o Postgres).
- A **license key** nunca fica na imagem — é injetada por **K8s Secret** em runtime.

## Justificativa
- New Relic tem free tier generoso, agent .NET maduro (instrumentação automática de ASP.NET Core, EF Core, HttpClient) e correlação nativa logs↔APM↔infra.
- Serilog em JSON permite parsing/consulta e correlação sem parser customizado.
- Agent na imagem (vs. sidecar) simplifica o deploy e captura métricas do processo.

## Consequências
- **Positivas:** visão de latência, throughput, erros, dependências (banco) e logs correlacionados; base para dashboards e alertas (Fase 5).
- **Negativas:** a imagem cresce (~5 MB do agent); requer a license key como secret. O agent só reporta quando `NEW_RELIC_LICENSE_KEY` está presente (sem ela, a app roda normalmente, sem telemetria).

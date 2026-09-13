# RFC 0004 — Observabilidade unificada com New Relic

- **Status:** Aceita
- **Data:** Fase 3

## Contexto
O enunciado exige observabilidade abrangente: **latência**, **CPU/memória do Kubernetes**,
**health checks**, **alertas**, **logs JSON correlacionados** e **dashboards de negócio**
(volume de OS/dia, tempo médio por status, erros de integração).

## Decisão
Adotar **New Relic (free tier)** como plataforma única:
- **APM** na API .NET (agent embutido na imagem via tarball, ativado por `CORECLR_*` +
  `NEW_RELIC_LICENSE_KEY` em runtime) → latência, throughput, erros, traces.
- **Infraestrutura Kubernetes** via `nri-bundle` (Helm): CPU/memória/pods, kube-events, logs.
- **Logs JSON correlacionados** com **Serilog** (`CompactJsonFormatter`), cada evento com
  `trace.id`/`span.id` para amarrar logs aos traces do APM.
- **Dashboards as-code** (NRQL) e **alertas** (latência P95, taxa de erro, app sem tráfego)
  versionados em `fiap-app/observability/`.

## Alternativas consideradas
- **Prometheus + Grafana + OTel (OSS local)** — poderoso, mas exige manter o stack de coleta e
  armazenamento; mais peças para operar localmente.
- **Datadog free** — cobertura semelhante; New Relic foi escolhido pelo free tier generoso e pela
  auto-instrumentação .NET + Kubernetes num só agente.

## Consequências
- (+) Uma só plataforma cobre APM, infra, logs, dashboards e alertas.
- (+) Dashboards e alertas versionados (as-code), reproduzíveis.
- (−) Dependência de SaaS externo (license key como secret, nunca versionada).
- (−) O banco gerenciado (Railway) é observado **indiretamente** via APM da app (sem acesso ao host
  para agente de banco).

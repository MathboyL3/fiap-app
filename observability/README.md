# Observabilidade — New Relic (Fase 3)

Cobre **latência, CPU/memória do K8s, healthchecks, alertas e logs JSON correlacionados**, além de dashboards de negócio (volume de OS, tempo por etapa, erros de integração).

## Componentes
| Sinal | Como é coletado |
|---|---|
| **APM** (latência, throughput, erros, DB) | New Relic .NET agent embutido na imagem (`CORECLR_*` + `NEW_RELIC_LICENSE_KEY`) |
| **Infra K8s** (CPU/mem, pods, HPA, eventos) | `nri-bundle` via Helm (nri-kubernetes + kube-state-metrics) |
| **Logs** (JSON, correlacionados) | Serilog `CompactJsonFormatter` (`trace.id`/`span.id`) + newrelic-logging |
| **Banco** (queries, db time) | via APM (instrumentação Npgsql/EF Core) — Railway é gerenciado, sem agent no host |

## Instalação

### 1) APM da aplicação
A license key é injetada como **K8s Secret** e exposta como env `NEW_RELIC_LICENSE_KEY` (ver `fiap-infra-k8s`). O agent já vem na imagem `fiap-app`.

### 2) Agente de infra + logs (Helm)
```bash
helm repo add newrelic https://helm-charts.newrelic.com && helm repo update
helm upgrade --install newrelic-bundle newrelic/nri-bundle \
  --namespace newrelic --create-namespace \
  --set global.licenseKey="$NEW_RELIC_LICENSE_KEY" \
  --set global.cluster="fiap-fase3-docker-desktop" \
  --set newrelic-infrastructure.privileged=true \
  --set global.lowDataMode=true \
  --set kube-state-metrics.enabled=true \
  --set nri-kube-events.enabled=true \
  --set newrelic-logging.enabled=true --set logging.enabled=true
```

## Dashboard (as-code)
`dashboard-fiap-fase3.json` — 3 páginas (Aplicação/Kubernetes/Negócio). Crie/atualize via NerdGraph:
```bash
# criar
curl -s https://api.newrelic.com/graphql -H "Api-Key: $NEW_RELIC_USER_KEY" \
  -H 'Content-Type: application/json' \
  --data-binary @<(jq -n --slurpfile d dashboard-fiap-fase3.json \
    '{query:"mutation($a:Int!,$dash:DashboardInput!){dashboardCreate(accountId:$a,dashboard:$dash){entityResult{guid}}}",variables:{a:8507486,dash:$d[0]}}')
```
> Ajuste `accountId` (aqui `8507486`) nas queries do JSON conforme sua conta.

## Alertas (NRQL)
Policy **"FIAP Fase 3 - Oficina"** com 3 condições:
| Condição | Regra |
|---|---|
| Latência P95 alta | `percentile(duration*1000,95) > 1500ms` por 5 min |
| Taxa de erro alta | `percentage(count(*), WHERE error IS true) > 10%` por 5 min |
| App sem tráfego/down | `count(*) < 1` por 5 min |

## Queries NRQL úteis
```sql
-- Latência média
SELECT average(duration)*1000 FROM Transaction WHERE appName='fiap-app' TIMESERIES

-- Volume de OS criadas por dia
SELECT count(*) FROM Transaction WHERE appName='fiap-app' AND request.method='POST'
  AND name LIKE '%OrdensServico%' TIMESERIES 1 day SINCE 7 days ago

-- CPU/memória dos pods
SELECT average(cpuUsedCores), average(memoryUsedBytes)
  FROM K8sContainerSample WHERE containerName='fiap-app' TIMESERIES

-- Erros de integração
SELECT count(*) FROM TransactionError WHERE appName='fiap-app' TIMESERIES

-- Logs correlacionados (trace.id)
SELECT message, trace.id FROM Log WHERE entity.name='fiap-app' SINCE 30 minutes ago
```

## Gerar tráfego para demonstração
```bash
kubectl port-forward -n oficina svc/fiap-app 18080:80 &
BASE=http://localhost:18080 ../scripts/gerar-trafego.sh 10
```

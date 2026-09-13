# Guia de Entrega — Tech Challenge Fase 3 (SOAT/FIAP)

Documento de fechamento: onde está cada requisito do enunciado e o que ainda depende de ação manual.

## Repositórios (links)
- **fiap-auth-lambda** — https://github.com/MathboyL3/fiap-auth-lambda
- **fiap-app** — https://github.com/MathboyL3/fiap-app
- **fiap-infra-k8s** — https://github.com/MathboyL3/fiap-infra-k8s
- **fiap-infra-db** — https://github.com/MathboyL3/fiap-infra-db

## Como os requisitos foram atendidos

| Requisito do enunciado | Onde está |
|---|---|
| API Gateway + autenticação por CPF (Lambda valida CPF → consulta cliente → JWT) | `fiap-auth-lambda` (API Gateway REST + Lambda Node/TS no LocalStack) |
| 4 repositórios com CI/CD, `main` protegida, merge via PR | Os 4 repos (GitHub Actions + ruleset `protect-main`) |
| Infra cloud via Terraform: API Gateway, Lambda, banco gerenciado, cluster K8s escalável | `fiap-auth-lambda/terraform`, `fiap-infra-db`, `fiap-infra-k8s` |
| Banco de dados gerenciado + justificativa + modelo ER | `fiap-infra-db` (PostgreSQL no Railway) + `fiap-infra-db/docs/MODELO-DADOS.md` |
| Cluster Kubernetes escalável (HPA) | `fiap-infra-k8s` (deployment + HPA 2..6 + Ingress) — escala comprovada ao vivo |
| Observabilidade: latência, CPU/mem K8s, health checks, alertas, logs JSON correlacionados | New Relic — APM + `nri-bundle` + Serilog; `fiap-app/observability/` |
| Dashboards (volume OS/dia, tempo médio por status, erros) | `fiap-app/observability/dashboard-fiap-fase3.json` (as-code, NRQL) |
| Diagrama de componentes (cloud) | `fiap-app/docs/ARQUITETURA.md` (§1) |
| Diagrama de sequência (auth + abrir OS) | `fiap-app/docs/ARQUITETURA.md` (§2 e §3) |
| RFCs e ADRs | `fiap-app/docs/rfc/` + `docs/adr/` de cada repo |
| READMEs, Dockerfiles, pipelines | Em cada repositório |

## Verificações feitas ao vivo (evidências)
- Lambda auth e2e no LocalStack: `200` (JWT), `404` (CPF não cadastrado), `400` (CPF inválido).
- JWT emitido pela Lambda **aceito pela API .NET** (mesmo iss/aud/secret) — validado no cluster.
- Fluxo e2e: sem token → `401`; token de cliente em rota de admin → `403` (JWT validado, role negada).
- App aplicou migrations no Railway → **11 tabelas** criadas (schema real).
- **HPA escalou 2 → 6 réplicas** sob carga (memória acima do alvo).
- New Relic com dados reais (NRQL): transações, OS criadas, erros, `K8sContainerSample`, logs.
- Dashboard (3 páginas: Aplicação/Kubernetes/Negócio) + política de alertas com 3 condições.

## Pendências manuais (fora do escopo automatizável)
> Estes passos **dependem do responsável pela entrega** e devem ser feitos antes de submeter:

1. **Adicionar o usuário `soat-architecture`** como colaborador (read) nos 4 repositórios
   (Settings → Collaborators). *Intencionalmente não automatizado.*
2. **Gravar o vídeo (≤ 15 min)** demonstrando a solução (auth por CPF, abertura de OS,
   escalabilidade/HPA e os dashboards do New Relic).
3. **Montar o PDF final** com os links dos 4 repositórios e o link do vídeo.
4. Garantir que os **secrets de CI** estejam configurados em cada repo (já feito nesta sessão):
   `RAILWAY_TOKEN` (fiap-infra-db), `JWT_SECRET` + `DATABASE_URL` (fiap-auth-lambda).

## Roteiro sugerido para o vídeo
1. Mostrar os 4 repos e o ruleset `protect-main` (PR obrigatório).
2. `POST /auth` com CPF válido no API Gateway (LocalStack) → recebe JWT.
3. Chamar a API .NET no K8s com o JWT → abrir uma OS e conduzir o ciclo.
4. Mostrar o HPA escalando (`kubectl get hpa -n oficina -w`) sob carga.
5. Abrir os dashboards do New Relic (Aplicação / Kubernetes / Negócio) e uma condição de alerta.
6. Fechar com a visão de arquitetura (`docs/ARQUITETURA.md`).

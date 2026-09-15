# RFC 0001 — Separação em 4 repositórios com CI/CD e `main` protegida

- **Status:** Aceita
- **Data:** Fase 3

## Contexto
O enunciado exige que a solução seja entregue em **4 repositórios independentes** — serviço serverless de
autenticação, infraestrutura do Kubernetes, infraestrutura do banco e a aplicação — cada um com
**CI/CD de deploy automatizado**, `main` **protegida** e alterações somente via **Pull Request**.

## Decisão
Criar 4 repositórios públicos no GitHub (owner `MathboyL3`):

- `fiap-auth-lambda` — serviço serverless de autenticação CPF→JWT (TypeScript/Bun, Railway).
- `fiap-app` — API .NET (Clean Architecture) da oficina.
- `fiap-infra-k8s` — Terraform do cluster Kubernetes + gateway Kong (deployment, HPA, Kong/Konga, secrets).
- `fiap-infra-db` — Terraform do banco gerenciado.

Cada repositório tem:
- Workflow **GitHub Actions** próprio (build/test/validate em PR; deploy/apply no merge à `main`).
- **Ruleset `protect-main`**: exige PR, bloqueia push direto, force-push e deleção da `main`.
- Todo o trabalho flui por **branch → PR → merge**.

## Alternativas consideradas
- **Monorepo único** — mais simples de coordenar, mas viola o requisito de 4 repositórios e mistura
  ciclos de deploy muito diferentes (infra vs. aplicação vs. serverless).
- **Repositórios privados** — na conta gratuita, proteção de branch/ruleset só é grátis em
  repositórios **públicos**; por isso os 4 são públicos.

## Consequências
- (+) Ciclos de release independentes; cada peça evolui e faz deploy sozinha.
- (+) Proteção de `main` e revisão via PR em todos.
- (−) Contrato entre repos (ex.: JWT `iss/aud/secret`, connection string) precisa ser mantido
  **coerente manualmente** — documentado neste conjunto de RFCs/ADRs e em variáveis/secrets
  compartilhados.

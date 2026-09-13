# RFC 0003 — Nuvem simulada (LocalStack) + banco gerenciado real (Railway)

- **Status:** Aceita
- **Data:** Fase 3

## Contexto
O enunciado pede **infraestrutura cloud provisionada via Terraform** (API Gateway, Lambda, banco
gerenciado, cluster Kubernetes escalável) e **observabilidade**. O objetivo é entregar tudo
**sem custo**, mas mantendo o código de infraestrutura **portável para nuvem real**.

## Decisão
- **AWS simulada com LocalStack** (Docker): API Gateway + Lambda + Secrets Manager + IAM + Logs.
  O Terraform usa os providers **`aws` reais** apontando os endpoints para `localhost:4566` com
  credenciais fake — o mesmo código roda contra a AWS real trocando o endpoint/credenciais.
- **Banco gerenciado real: PostgreSQL no Railway**, provisionado/gerenciado via Terraform
  (provider comunitário `railway`). É um banco **gerenciado de verdade** (backup, volume, operação
  pelo provedor), acessível pela app e pela Lambda via TCP proxy público.
- **Kubernetes local escalável** (Docker Desktop) provisionado por Terraform (provider `kubernetes`)
  com HPA — cluster real, escalável, sem custo.

## Alternativas consideradas
- **AWS real (RDS/Lambda/API Gateway)** — mais fiel, porém com custo/risco de cobrança; rejeitada
  para a entrega, mas o Terraform foi mantido portável.
- **Postgres dentro do cluster** — rejeitada: o enunciado pede banco **gerenciado**; Railway atende
  sem custo relevante.

## Consequências
- (+) Custo zero, reproduzível localmente, código portável para AWS real.
- (+) Banco gerenciado de verdade, com persistência independente do cluster.
- (−) LocalStack não é 100% idêntico à AWS (limitações pontuais) — suficiente para o escopo.
- (−) O TCP proxy do Railway não é gerenciável pelo provider Terraform (tratado como variável +
  ADR no `fiap-infra-db`).

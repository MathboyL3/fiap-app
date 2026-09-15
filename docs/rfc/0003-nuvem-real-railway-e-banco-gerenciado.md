# RFC 0003 — Nuvem real (Railway) + banco gerenciado + cluster Kubernetes

- **Status:** Aceita
- **Data:** Fase 3

## Contexto
O enunciado pede **infraestrutura cloud provisionada via Terraform** (autenticação serverless,
banco gerenciado, cluster Kubernetes escalável) e **observabilidade**. O objetivo é entregar tudo
com **custo baixo/zero**, usando **nuvem real** sempre que possível e mantendo o código de
infraestrutura reprodutível.

## Decisão
- **Nuvem gerenciada real: Railway.** Hospeda dois componentes:
  - **PostgreSQL gerenciado** (banco da aplicação), provisionado/importado via Terraform
    (provider comunitário `railway`) — banco **gerenciado de verdade** (backup, volume persistente,
    operação pelo provedor).
  - **fiap-auth** (serviço serverless de autenticação, container **Bun**) com **URL pública**,
    consumindo o mesmo Postgres.
- **Cluster Kubernetes escalável** (Docker Desktop) provisionado por Terraform (providers
  `kubernetes` e `helm`), com **HPA** e o **Kong** como API Gateway (roteamento + rate-limiting)
  e o **Konga** como GUI — cluster real, escalável, sem custo de nuvem.

## Alternativas consideradas
- **AWS real (RDS / Lambda / API Gateway)** — mais fiel a "serverless AWS", porém com custo e risco
  de cobrança; rejeitada para a entrega em favor do Railway (nuvem real, sem custo relevante).
- **Postgres dentro do cluster** — rejeitada: o enunciado pede banco **gerenciado**; o Railway
  atende sem custo relevante e com persistência independente do cluster.
- **Ingress NGINX como gateway** — mantido como alternativa disponível; o **Kong** foi escolhido
  como gateway principal por alinhamento com o padrão de API Gateway (Services/Routes/Plugins) e
  administração via GUI (Konga).

## Consequências
- (+) **Nuvem real** para banco e autenticação, com URL pública e deploy contínuo, sem custo relevante.
- (+) Banco gerenciado de verdade, com persistência independente do cluster.
- (+) Cluster Kubernetes real, escalável (HPA), atrás de um API Gateway (Kong).
- (−) O driver `pg` (Node/Bun) não resolve a rede privada IPv6-only do Railway; a auth usa o
  **TCP proxy público com SSL** (a API .NET/Npgsql resolve a rede interna). Detalhado no ADR do
  `fiap-auth-lambda` e no `fiap-infra-db`.
- (−) O TCP proxy do Railway não é gerenciável pelo provider Terraform (tratado como variável +
  ADR no `fiap-infra-db`).

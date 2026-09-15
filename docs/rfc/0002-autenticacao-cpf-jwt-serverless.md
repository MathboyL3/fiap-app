# RFC 0002 — Autenticação por CPF via serviço serverless

- **Status:** Aceita
- **Data:** Fase 3

## Contexto
A autenticação do cliente deve ser feita **por CPF**: o cliente informa o CPF, o sistema valida,
localiza o cliente e devolve um **JWT**. O enunciado pede que isso seja feito por um **serviço
serverless desacoplado** da API principal.

## Decisão
- **Function Serverless em TypeScript** executada no **Railway Functions** (runtime **Bun**, `Bun.serve`), exposta por HTTP
  (`POST /auth`) e hospedado em **nuvem real (Railway)** com URL pública.
- O serviço: valida os dígitos verificadores do CPF → consulta o cliente no Postgres → assina um
  **JWT HS256**.
- **Contrato do JWT compartilhado** com a API .NET: `iss = aud = Oficina.Api`, algoritmo HS256,
  **mesmo segredo**, claims `sub` (id do cliente), `email`, `role=Cliente`, `cpf`, `name`, `jti`.
  Isso permite que o token emitido pela auth seja aceito diretamente pela API .NET.
- Segredos (`JWT_SECRET`, `DATABASE_URL`) lidos de **variáveis de ambiente** injetadas pela
  plataforma (Railway).

## Alternativas consideradas
- **Autenticação embutida na API .NET** — rejeitada: o enunciado pede o fluxo por um serviço
  serverless desacoplado, o que permite trocar/escalar a autenticação sem tocar na API.
- **AWS Lambda + API Gateway** — considerada por ser o "serverless" clássico; preterida em favor de
  uma **Railway Function** (serverless nativo, runtime Bun, URL pública, sem custo relevante),
  mantendo o mesmo contrato de JWT.
- **Runtime em .NET/Python** — TypeScript/Bun foi escolhido pela subida rápida, ecossistema de
  JWT/pg maduro e simplicidade de empacotamento.

## Consequências
- (+) Autenticação isolada, com deploy e escala próprios, em nuvem real.
- (+) Interoperabilidade: o JWT emitido pela auth é aceito pela API .NET (mesmo iss/aud/secret).
- (−) O segredo HS256 precisa estar sincronizado entre a auth e a API (gerido por secret/variável,
  não versionado).
- (−) A auth depende do schema do banco (nomes de colunas do EF) — acoplamento tratado com query
  explícita e testes.

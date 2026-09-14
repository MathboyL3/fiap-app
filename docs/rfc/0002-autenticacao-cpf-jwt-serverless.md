# RFC 0002 — Autenticação por CPF via serverless (API Gateway + Lambda)

- **Status:** Aceita
- **Data:** Fase 3

## Contexto
A autenticação do cliente deve ser feita **por CPF**: o cliente informa o CPF, o sistema valida,
localiza o cliente e devolve um **JWT**. O enunciado pede que isso passe por **API Gateway** e uma
**função (Lambda)** — desacoplada da API principal.

## Decisão
- **Lambda em Node.js/TypeScript** atrás de um **API Gateway REST** (`POST /auth`).
- A Lambda: valida os dígitos verificadores do CPF → consulta o cliente no Postgres → assina um
  **JWT HS256**.
- **Contrato do JWT compartilhado** com a API .NET: `iss = aud = Oficina.Api`, algoritmo HS256,
  **mesmo segredo**, claims `sub` (id do cliente), `email`, `role=Cliente`, `cpf`, `name`, `jti`.
  Isso permite que o token emitido pela Lambda seja aceito diretamente pela API .NET.
- Segredos (`JWT_SECRET`, `DATABASE_URL`) lidos do **Secrets Manager** (fallback env em dev).

## Alternativas consideradas
- **Autenticação embutida na API .NET** — rejeitada: o enunciado pede o fluxo via API Gateway +
  Lambda, e o desacoplamento permite trocar/escalar a autenticação sem tocar na API.
- **Lambda em .NET/Python** — Node/TS foi escolhido pela partida a frio rápida, ecossistema de
  JWT/pg maduro e simplicidade de empacotamento (esbuild → zip).

## Consequências
- (+) Autenticação isolada, com deploy e escala próprios.
- (+) Interoperabilidade: o JWT emitido pela Lambda é aceito pela API .NET (mesmo iss/aud/secret).
- (−) O segredo HS256 precisa estar sincronizado entre Lambda e API (gerido por secret, não versionado).
- (−) A Lambda depende do schema do banco (nomes de colunas do EF) — acoplamento tratado com query
  explícita e testes.

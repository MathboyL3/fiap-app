#!/usr/bin/env bash
# Gera trafego e2e realista para popular a telemetria (APM/dashboards New Relic):
# cria clientes, veiculos, servicos, pecas e Ordens de Servico percorrendo o
# ciclo de status (aberta -> diagnostico -> aprovacao -> execucao -> finalizada).
# Uso: BASE=http://localhost:18080 ./scripts/gerar-trafego.sh [N_OS]
set -euo pipefail
BASE="${BASE:-http://localhost:18080}"
N="${1:-8}"

jqget() { python -c "import sys,json;print(json.load(sys.stdin).get('$1',''))"; }

echo "==> login gerente"
TOKEN=$(curl -s -X POST "$BASE/api/auth/login" -H "content-type: application/json" \
  -d '{"email":"gerente@oficina.com","senha":"Senha@1234"}' | jqget accessToken)
[ -n "$TOKEN" ] || { echo "falha no login"; exit 1; }
AUTH=(-H "Authorization: Bearer $TOKEN" -H "content-type: application/json")

echo "==> catalogo: servico + peca"
SVC=$(curl -s "${AUTH[@]}" -X POST "$BASE/api/servicos" \
  -d '{"nome":"Troca de oleo","descricao":"Troca completa","valorBase":150.0,"tempoEstimadoMinutos":60}' | jqget id)
PECA=$(curl -s "${AUTH[@]}" -X POST "$BASE/api/pecas" \
  -d '{"nome":"Filtro de oleo","codigo":"FL-'$RANDOM'","unidade":"UN","valor":40.0,"quantidadeInicial":500,"limiteMinimo":10}' | jqget id)

CPFS=(52998224725 11144477735 39053344705 12345678909 71428793860)
for i in $(seq 1 "$N"); do
  CPF=${CPFS[$((i % ${#CPFS[@]}))]}
  CLI=$(curl -s "${AUTH[@]}" -X POST "$BASE/api/clientes" \
    -d "{\"nome\":\"Cliente $i\",\"documento\":\"$CPF\",\"email\":\"c$i@ex.com\",\"telefone\":\"1198888$i\"}" | jqget id)
  [ -n "$CLI" ] || CLI=$(curl -s "${AUTH[@]}" "$BASE/api/clientes/por-documento/$CPF" | jqget id)
  VEI=$(curl -s "${AUTH[@]}" -X POST "$BASE/api/veiculos" \
    -d "{\"clienteId\":\"$CLI\",\"placa\":\"ABC$((1000+i))\",\"marca\":\"VW\",\"modelo\":\"Gol\",\"ano\":2020}" | jqget id)
  OS=$(curl -s "${AUTH[@]}" -X POST "$BASE/api/ordens-servico" \
    -d "{\"clienteId\":\"$CLI\",\"veiculoId\":\"$VEI\"}" | jqget id)
  [ -n "$OS" ] || { echo "  [$i] falha ao abrir OS"; continue; }
  curl -s "${AUTH[@]}" -X POST "$BASE/api/ordens-servico/$OS/servicos" -d "{\"servicoId\":\"$SVC\",\"quantidade\":1}" >/dev/null
  curl -s "${AUTH[@]}" -X POST "$BASE/api/ordens-servico/$OS/pecas" -d "{\"pecaId\":\"$PECA\",\"quantidade\":2}" >/dev/null
  curl -s "${AUTH[@]}" -X POST "$BASE/api/ordens-servico/$OS/diagnostico" -d '{"observacoes":"OK"}' >/dev/null
  curl -s "${AUTH[@]}" -X POST "$BASE/api/ordens-servico/$OS/enviar-aprovacao" >/dev/null
  curl -s "${AUTH[@]}" -X POST "$BASE/api/ordens-servico/$OS/aprovar" >/dev/null
  # metade finaliza/entrega, outra parte fica em execucao (variedade de status)
  if [ $((i % 2)) -eq 0 ]; then
    curl -s "${AUTH[@]}" -X POST "$BASE/api/ordens-servico/$OS/finalizar" >/dev/null
    curl -s "${AUTH[@]}" -X POST "$BASE/api/ordens-servico/$OS/entregar" >/dev/null
  fi
  # leituras (geram spans de GET)
  curl -s "${AUTH[@]}" "$BASE/api/ordens-servico/$OS" >/dev/null
  curl -s "${AUTH[@]}" "$BASE/api/ordens-servico" >/dev/null
  echo "  [$i] OS $OS criada (cpf $CPF)"
done

echo "==> leituras de painel/metricas"
curl -s "${AUTH[@]}" "$BASE/api/ordens-servico/painel" >/dev/null || true
curl -s "${AUTH[@]}" "$BASE/api/ordens-servico/metricas/tempo-medio" >/dev/null || true
echo "==> concluido."

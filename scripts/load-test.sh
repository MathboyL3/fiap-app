#!/usr/bin/env bash
# Gera carga na API para demonstrar o HPA escalando os pods.
# Faz um port-forward e dispara requisições em paralelo contra o endpoint público de status.
# Em outro terminal, observe: kubectl get hpa -n oficina -w   e   kubectl get pods -n oficina -w
set -euo pipefail

DURATION="${DURATION:-120}"      # segundos de carga
CONCURRENCY="${CONCURRENCY:-50}" # requisições concorrentes
PORT="${PORT:-8080}"

echo ">> Iniciando port-forward em :$PORT ..."
kubectl port-forward -n oficina svc/oficina-api "$PORT:80" >/tmp/oficina-pf.log 2>&1 &
PF_PID=$!
trap 'kill $PF_PID 2>/dev/null || true' EXIT
sleep 3

URL="http://localhost:$PORT/health/ready"
echo ">> Gerando carga em $URL por ${DURATION}s com ${CONCURRENCY} workers ..."
echo ">> Acompanhe em outro terminal:  kubectl get hpa -n oficina -w"

END=$(( $(date +%s) + DURATION ))
worker() {
  while [ "$(date +%s)" -lt "$END" ]; do
    curl -s -o /dev/null "$URL" || true
  done
}

for _ in $(seq 1 "$CONCURRENCY"); do worker & done
wait

echo ">> Carga finalizada. Verifique o HPA:"
kubectl get hpa -n oficina
kubectl get pods -n oficina

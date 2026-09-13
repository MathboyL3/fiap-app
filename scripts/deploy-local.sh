#!/usr/bin/env bash
# Deploy local completo da Oficina no cluster do Docker Desktop.
# Builda a imagem no mesmo daemon do cluster e aplica os manifestos do k8s/.
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
APP_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

CONTEXT="${KUBE_CONTEXT:-docker-desktop}"
IMAGE="${API_IMAGE:-oficina-api:local}"

echo ">> Usando contexto kubectl: $CONTEXT"
kubectl config use-context "$CONTEXT"

echo ">> Buildando imagem $IMAGE ..."
docker build -t "$IMAGE" -f "$APP_DIR/Dockerfile" "$APP_DIR"

echo ">> Aplicando manifestos (kubectl apply -k k8s/) ..."
kubectl apply -k "$APP_DIR/k8s/"

echo ">> Aguardando Postgres ..."
kubectl wait --for=condition=ready pod -l app.kubernetes.io/name=postgres -n oficina --timeout=120s

echo ">> Aguardando rollout da API ..."
kubectl rollout status deployment/oficina-api -n oficina --timeout=180s

echo ""
echo ">> Deploy concluído. Pods:"
kubectl get pods -n oficina

cat <<'EOF'

Acesse a API localmente com port-forward:
  kubectl port-forward -n oficina svc/oficina-api 8080:80
  # depois: http://localhost:8080  (Swagger na raiz)

Crie o primeiro usuário (bootstrap):
  curl -X POST http://localhost:8080/api/auth/bootstrap \
    -H "Content-Type: application/json" \
    -d '{"nome":"Admin","email":"admin@oficina.local","senha":"Admin@123","role":"Gerente"}'
EOF

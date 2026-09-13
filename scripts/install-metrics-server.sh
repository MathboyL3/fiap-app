#!/usr/bin/env bash
# Instala o metrics-server no cluster local (Docker Desktop) — pré-requisito do HPA.
# No Docker Desktop é preciso o flag --kubelet-insecure-tls (certificado do kubelet é self-signed).
set -euo pipefail

echo ">> Aplicando metrics-server..."
kubectl apply -f https://github.com/kubernetes-sigs/metrics-server/releases/latest/download/components.yaml

echo ">> Aplicando patch --kubelet-insecure-tls (necessário no Docker Desktop)..."
kubectl patch deployment metrics-server -n kube-system --type=json \
  -p='[{"op":"add","path":"/spec/template/spec/containers/0/args/-","value":"--kubelet-insecure-tls"}]' || true

echo ">> Aguardando metrics-server ficar disponível..."
kubectl rollout status deployment/metrics-server -n kube-system --timeout=120s

echo ">> Pronto. Teste com: kubectl top pods -n oficina"

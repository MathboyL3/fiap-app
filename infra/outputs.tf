output "namespace" {
  description = "Namespace provisionado."
  value       = kubernetes_namespace_v1.oficina.metadata[0].name
}

output "api_service" {
  description = "Service da API."
  value       = kubernetes_service_v1.api.metadata[0].name
}

output "api_node_port" {
  description = "NodePort da API (acesso local)."
  value       = var.node_port
}

output "acesso_local" {
  description = "Como acessar a API localmente (port-forward é o método confiável no Docker Desktop)."
  value       = "kubectl port-forward -n ${var.namespace} svc/oficina-api 8080:80  ->  http://localhost:8080"
}

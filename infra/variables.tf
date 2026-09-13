variable "kube_config_path" {
  description = "Caminho do kubeconfig."
  type        = string
  default     = "~/.kube/config"
}

variable "kube_context" {
  description = "Contexto do kubectl (cluster-alvo)."
  type        = string
  default     = "docker-desktop"
}

variable "namespace" {
  description = "Namespace onde a aplicação será provisionada."
  type        = string
  default     = "oficina"
}

variable "api_image" {
  description = "Imagem da API (buildada localmente no Docker Desktop)."
  type        = string
  default     = "oficina-api:local"
}

variable "api_replicas" {
  description = "Número inicial de réplicas da API (o HPA ajusta em runtime)."
  type        = number
  default     = 2
}

variable "node_port" {
  description = "NodePort para acesso local à API."
  type        = number
  default     = 30080
}

# --- Segredos (marcados como sensíveis; sobrescreva via TF_VAR_* ou terraform.tfvars) ---

variable "jwt_secret" {
  description = "Segredo JWT (>= 32 chars)."
  type        = string
  sensitive   = true
  default     = "CHANGE_ME_use_at_least_32_characters_secret_2026"
}

variable "webhook_secret" {
  description = "Segredo do webhook de aprovação de orçamento."
  type        = string
  sensitive   = true
  default     = "CHANGE_ME_webhook_secret_dev_only"
}

variable "postgres_password" {
  description = "Senha do Postgres."
  type        = string
  sensitive   = true
  default     = "oficina_dev_only"
}

variable "postgres_storage" {
  description = "Tamanho do volume do Postgres."
  type        = string
  default     = "1Gi"
}

variable "storage_class" {
  description = "StorageClass para o PVC do Postgres (default do Docker Desktop = standard)."
  type        = string
  default     = "standard"
}

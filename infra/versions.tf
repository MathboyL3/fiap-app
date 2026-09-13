terraform {
  required_version = ">= 1.6"

  required_providers {
    kubernetes = {
      source  = "hashicorp/kubernetes"
      version = "~> 2.31"
    }
  }
}

# Aponta para o cluster local do Docker Desktop (kubeconfig padrão do usuário).
provider "kubernetes" {
  config_path    = var.kube_config_path
  config_context = var.kube_context
}

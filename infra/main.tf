locals {
  common_labels = {
    "app.kubernetes.io/part-of" = "oficina"
  }
  api_labels = {
    "app.kubernetes.io/name"    = "oficina-api"
    "app.kubernetes.io/part-of" = "oficina"
  }
  pg_labels = {
    "app.kubernetes.io/name"    = "postgres"
    "app.kubernetes.io/part-of" = "oficina"
  }
}

# ---------------------------------------------------------------------------
# Namespace
# ---------------------------------------------------------------------------
resource "kubernetes_namespace_v1" "oficina" {
  metadata {
    name   = var.namespace
    labels = local.common_labels
  }
}

# ---------------------------------------------------------------------------
# ConfigMap (config não-sensível)
# ---------------------------------------------------------------------------
resource "kubernetes_config_map_v1" "config" {
  metadata {
    name      = "oficina-config"
    namespace = kubernetes_namespace_v1.oficina.metadata[0].name
    labels    = local.common_labels
  }

  data = {
    ASPNETCORE_ENVIRONMENT   = "Production"
    ASPNETCORE_URLS          = "http://+:8080"
    Jwt__Issuer              = "Oficina.Api"
    Jwt__Audience            = "Oficina.Api"
    Jwt__ExpirationMinutes   = "60"
    Postgres__Host           = "postgres"
    Postgres__Port           = "5432"
    Postgres__Database       = "oficina"
    Postgres__Username       = "oficina"
  }
}

# ---------------------------------------------------------------------------
# Secret (segredos)
# ---------------------------------------------------------------------------
resource "kubernetes_secret_v1" "secrets" {
  metadata {
    name      = "oficina-secrets"
    namespace = kubernetes_namespace_v1.oficina.metadata[0].name
    labels    = local.common_labels
  }

  type = "Opaque"

  data = {
    Jwt__Secret        = var.jwt_secret
    Webhook__Secret    = var.webhook_secret
    Postgres__Password = var.postgres_password
  }
}

# ---------------------------------------------------------------------------
# Postgres — Service headless + StatefulSet com PVC
# ---------------------------------------------------------------------------
resource "kubernetes_service_v1" "postgres" {
  metadata {
    name      = "postgres"
    namespace = kubernetes_namespace_v1.oficina.metadata[0].name
    labels    = local.pg_labels
  }

  spec {
    cluster_ip = "None"
    selector   = { "app.kubernetes.io/name" = "postgres" }
    port {
      name        = "postgres"
      port        = 5432
      target_port = 5432
    }
  }
}

resource "kubernetes_stateful_set_v1" "postgres" {
  metadata {
    name      = "postgres"
    namespace = kubernetes_namespace_v1.oficina.metadata[0].name
    labels    = local.pg_labels
  }

  spec {
    service_name = "postgres"
    replicas     = 1

    selector {
      match_labels = { "app.kubernetes.io/name" = "postgres" }
    }

    template {
      metadata {
        labels = local.pg_labels
      }

      spec {
        container {
          name  = "postgres"
          image = "postgres:16-alpine"

          port {
            name           = "postgres"
            container_port = 5432
          }

          env {
            name = "POSTGRES_DB"
            value_from {
              config_map_key_ref {
                name = kubernetes_config_map_v1.config.metadata[0].name
                key  = "Postgres__Database"
              }
            }
          }
          env {
            name = "POSTGRES_USER"
            value_from {
              config_map_key_ref {
                name = kubernetes_config_map_v1.config.metadata[0].name
                key  = "Postgres__Username"
              }
            }
          }
          env {
            name = "POSTGRES_PASSWORD"
            value_from {
              secret_key_ref {
                name = kubernetes_secret_v1.secrets.metadata[0].name
                key  = "Postgres__Password"
              }
            }
          }
          env {
            name  = "PGDATA"
            value = "/var/lib/postgresql/data/pgdata"
          }

          volume_mount {
            name       = "postgres-data"
            mount_path = "/var/lib/postgresql/data"
          }

          readiness_probe {
            exec {
              command = ["sh", "-c", "pg_isready -U oficina -d oficina"]
            }
            initial_delay_seconds = 5
            period_seconds        = 10
          }

          liveness_probe {
            exec {
              command = ["sh", "-c", "pg_isready -U oficina -d oficina"]
            }
            initial_delay_seconds = 15
            period_seconds        = 20
          }

          resources {
            requests = {
              cpu    = "100m"
              memory = "128Mi"
            }
            limits = {
              cpu    = "500m"
              memory = "256Mi"
            }
          }
        }
      }
    }

    volume_claim_template {
      metadata {
        name = "postgres-data"
      }
      spec {
        access_modes       = ["ReadWriteOnce"]
        storage_class_name = var.storage_class
        resources {
          requests = {
            storage = var.postgres_storage
          }
        }
      }
    }
  }
}

# ---------------------------------------------------------------------------
# API — Deployment + Service NodePort
# ---------------------------------------------------------------------------
resource "kubernetes_deployment_v1" "api" {
  metadata {
    name      = "oficina-api"
    namespace = kubernetes_namespace_v1.oficina.metadata[0].name
    labels    = local.api_labels
  }

  spec {
    replicas = var.api_replicas

    selector {
      match_labels = { "app.kubernetes.io/name" = "oficina-api" }
    }

    template {
      metadata {
        labels = local.api_labels
      }

      spec {
        container {
          name              = "oficina-api"
          image             = var.api_image
          image_pull_policy = "IfNotPresent"

          port {
            name           = "http"
            container_port = 8080
          }

          env_from {
            config_map_ref {
              name = kubernetes_config_map_v1.config.metadata[0].name
            }
          }

          env {
            name = "Jwt__Secret"
            value_from {
              secret_key_ref {
                name = kubernetes_secret_v1.secrets.metadata[0].name
                key  = "Jwt__Secret"
              }
            }
          }
          env {
            name = "Webhook__Secret"
            value_from {
              secret_key_ref {
                name = kubernetes_secret_v1.secrets.metadata[0].name
                key  = "Webhook__Secret"
              }
            }
          }
          env {
            name = "Postgres__Password"
            value_from {
              secret_key_ref {
                name = kubernetes_secret_v1.secrets.metadata[0].name
                key  = "Postgres__Password"
              }
            }
          }
          env {
            name  = "ConnectionStrings__Postgres"
            value = "Host=postgres;Port=5432;Database=oficina;Username=oficina;Password=$(Postgres__Password)"
          }

          startup_probe {
            http_get {
              path = "/health/live"
              port = "http"
            }
            failure_threshold = 30
            period_seconds    = 5
          }

          liveness_probe {
            http_get {
              path = "/health/live"
              port = "http"
            }
            initial_delay_seconds = 10
            period_seconds        = 15
          }

          readiness_probe {
            http_get {
              path = "/health/ready"
              port = "http"
            }
            initial_delay_seconds = 10
            period_seconds        = 10
          }

          resources {
            requests = {
              cpu    = "100m"
              memory = "128Mi"
            }
            limits = {
              cpu    = "500m"
              memory = "512Mi"
            }
          }
        }
      }
    }
  }
}

resource "kubernetes_service_v1" "api" {
  metadata {
    name      = "oficina-api"
    namespace = kubernetes_namespace_v1.oficina.metadata[0].name
    labels    = local.api_labels
  }

  spec {
    type     = "NodePort"
    selector = { "app.kubernetes.io/name" = "oficina-api" }
    port {
      name        = "http"
      port        = 80
      target_port = 8080
      node_port   = var.node_port
    }
  }
}

# ---------------------------------------------------------------------------
# HPA — escala por CPU e memória
# ---------------------------------------------------------------------------
resource "kubernetes_horizontal_pod_autoscaler_v2" "api" {
  metadata {
    name      = "oficina-api"
    namespace = kubernetes_namespace_v1.oficina.metadata[0].name
    labels    = local.api_labels
  }

  spec {
    scale_target_ref {
      api_version = "apps/v1"
      kind        = "Deployment"
      name        = kubernetes_deployment_v1.api.metadata[0].name
    }

    min_replicas = 2
    max_replicas = 6

    metric {
      type = "Resource"
      resource {
        name = "cpu"
        target {
          type                = "Utilization"
          average_utilization = 60
        }
      }
    }

    metric {
      type = "Resource"
      resource {
        name = "memory"
        target {
          type                = "Utilization"
          average_utilization = 75
        }
      }
    }
  }
}

#!/usr/bin/env bash
set -euo pipefail

CLUSTER_NAME="agro-dev"
KIND_CONFIG_FILE="k8s/kind/config.yaml"
export RABBITMQ_HOST="${RABBITMQ_HOST:-rabbitmq-service.sensor-ingestion.svc.cluster.local}"
export RABBITMQ_PORT="${RABBITMQ_PORT:-5672}"
export RABBITMQ_DEFAULT_USER="${RABBITMQ_DEFAULT_USER:-admin}"
export RABBITMQ_DEFAULT_PASS="${RABBITMQ_DEFAULT_PASS:-admin123}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

log() {
  echo "▶ $1"
}

error() {
  echo "❌ $1" >&2
  exit 1
}

check_command() {
  command -v "$1" >/dev/null 2>&1 || error "Comando '$1' não encontrado"
}

log "Verificando dependências"
check_command kubectl
check_command kind

if kind get clusters | grep -q "^${CLUSTER_NAME}$"; then
    log "Cluster Kind '$CLUSTER_NAME' já existe"
    [ "$(kubectl config current-context 2>/dev/null)" != "kind-${CLUSTER_NAME}" ] && kubectl config use-context kind-${CLUSTER_NAME} 2>/dev/null || true
else
    log "Criando cluster Kind '$CLUSTER_NAME'"
    if [[ -f "$ROOT_DIR/$KIND_CONFIG_FILE" ]]; then
        kind create cluster \
        --name "$CLUSTER_NAME" \
        --config "$ROOT_DIR/$KIND_CONFIG_FILE"
    else
        log "Arquivo de configuração não existe"
        exit 1
    fi
fi

WAIT_TO="${WAIT_TIMEOUT:-45}"
if kubectl wait --for=condition=Ready nodes --all --timeout=0s 2>/dev/null; then log "Cluster já pronto."; else log "Aguardando cluster..."; kubectl wait --for=condition=Ready nodes --all --timeout="${WAIT_TO}s"; fi

CURRENT_CONTEXT="$(kubectl config current-context)"
if [[ "$CURRENT_CONTEXT" != kind-* ]]; then
  error "Contexto atual ($CURRENT_CONTEXT) não é um cluster Kind"
fi

PROJECTS_ROOT="${PROJECTS_ROOT:-$(cd "$ROOT_DIR/.." && pwd)}"
DATA_INGESTION_ROOT="${DATA_INGESTION_ROOT:-$PROJECTS_ROOT/AgroSolutions.DataIngestion}"
if ! kubectl get ns sensor-ingestion &>/dev/null && [ -d "$DATA_INGESTION_ROOT/k8s" ]; then
  log "Aplicando RabbitMQ (dependência)..."
  kubectl apply -f "$DATA_INGESTION_ROOT/k8s/namespaces.yaml"
  kubectl apply -f "$DATA_INGESTION_ROOT/k8s/infra/rabbitmq"
  kubectl wait --for=condition=ready pod -l app=rabbitmq -n sensor-ingestion --timeout="${WAIT_TO}s" 2>/dev/null || sleep 10
fi

if [[ -z "${SKIP_BUILD:-}" ]]; then
  log "Buildando imagem do worker"
  docker build -t agro-medicoes-worker:dev "$ROOT_DIR"
  log "Carregando imagem no Kind"
  kind load docker-image agro-medicoes-worker:dev --name "$CLUSTER_NAME"
fi

log "Criando namespaces"
kubectl apply -f "$ROOT_DIR/k8s/base/namespaces"

log "Criando secrets"
bash "$ROOT_DIR/scripts/create-secrets.sh"

log "Criando configmaps"
bash "$ROOT_DIR/scripts/deploy-configmap.sh"

log "Aplicando manifests base"
kubectl apply -f "$ROOT_DIR/k8s/base/mailpit"
kubectl apply -f "$ROOT_DIR/k8s/base/observability/collector"
kubectl apply -f "$ROOT_DIR/k8s/base/observability/loki"
kubectl apply -f "$ROOT_DIR/k8s/base/observability/prometheus"
kubectl apply -f "$ROOT_DIR/k8s/base/observability/tempo"
kubectl apply -f "$ROOT_DIR/k8s/base/postgresql"
kubectl apply -f "$ROOT_DIR/k8s/base/grafana"
kubectl apply -f "$ROOT_DIR/k8s/base/app"

log "Deploy concluído com sucesso ✅"

echo ""
echo "================================================"
echo "  Medicoes - URLs e acesso"
echo "================================================"
echo ""
echo "App:"
echo "  Medicoes:       namespace agro-medicoes (worker)"
echo ""
echo "Infra:"
echo "  Grafana:        http://localhost:30000 (admin/admin)"
echo "  Prometheus:     http://localhost:30902"
echo "  Mailpit:        http://localhost:30025 (UI)  |  SMTP: localhost:31025"
echo ""

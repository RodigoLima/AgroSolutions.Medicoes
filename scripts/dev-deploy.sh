#!/usr/bin/env bash
set -euo pipefail

# ========= CONFIG =========
CLUSTER_NAME="dev-worker"
KIND_CONFIG_FILE="k8s/kind/config.yaml"

# ========= PATHS =========
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"

# ========= HELPERS =========
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

# ========= CHECKS =========
log "Verificando dependências"
check_command kubectl
check_command kind

# ========= CLUSTER =========
if kind get clusters | grep -q "^${CLUSTER_NAME}$"; then
    log "Cluster Kind '$CLUSTER_NAME' já existe"
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


# ========= WAIT CLUSTER =========
log "Aguardando cluster ficar acessível"
kubectl wait --for=condition=Ready nodes --all --timeout=120s

CURRENT_CONTEXT="$(kubectl config current-context)"
if [[ "$CURRENT_CONTEXT" != kind-* ]]; then
  error "Contexto atual ($CURRENT_CONTEXT) não é um cluster Kind"
fi

# ========= BUILD WORKER IMAGE =========
log "Buildando imagem do worker"
docker build -t agro-medicoes-worker:dev "$ROOT_DIR"

log "Carregando imagem no Kind"
kind load docker-image agro-medicoes-worker:dev --name "$CLUSTER_NAME"

# ========= DEPLOY =========
log "Criando namespaces"
kubectl apply -f "$ROOT_DIR/k8s/base/namespaces"

log "Criando secrets"
bash "$ROOT_DIR/scripts/create-secrets.sh"

log "Criando configmaps"
bash "$ROOT_DIR/scripts/deploy-configmap.sh"

log "Aplicando manifests base"
kubectl apply -f "$ROOT_DIR/k8s/base/mailpit"
kubectl apply -f "$ROOT_DIR/k8s/base/rabbitmq"
kubectl apply -f "$ROOT_DIR/k8s/base/postgresql"
kubectl create configmap grafana-datasources \
    --from-file=$ROOT_DIR/observability/grafana/provisioning/datasources.yaml \
    -n agro-medicoes \
    --dry-run=client -o yaml | kubectl apply -f -
kubectl apply -f "$ROOT_DIR/k8s/base/grafana"
kubectl apply -f "$ROOT_DIR/k8s/base/app"

log "Deploy concluído com sucesso ✅"
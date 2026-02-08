#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
ENV_FILE="$ROOT_DIR/.env"
NAMESPACE="agro-medicoes"

set -a
[ -f "$ENV_FILE" ] && source "$ENV_FILE"
set +a
RABBITMQ_HOST="${RABBITMQ_HOST:-rabbitmq-service.sensor-ingestion.svc.cluster.local}"
RABBITMQ_PORT="${RABBITMQ_PORT:-5672}"
ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}"
export RABBITMQ_HOST RABBITMQ_PORT ASPNETCORE_ENVIRONMENT

echo "📦 Aplicando ConfigMaps do Grafana"

kubectl create configmap grafana-dashboard-provider \
  --from-file="$ROOT_DIR/observability/grafana/provisioning/dashboards.yaml" \
  -n agro-medicoes \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl create configmap grafana-datasources \
  --from-file="$ROOT_DIR/observability/grafana/provisioning/datasources.yaml" \
  -n agro-medicoes \
  --dry-run=client -o yaml | kubectl apply -f -

kubectl create configmap grafana-dashboards \
  --from-file="$ROOT_DIR/observability/grafana/dashboards" \
  -n agro-medicoes \
  --dry-run=client -o yaml | kubectl apply -f -

echo "📦 Aplicando ConfigMap do APP"

envsubst < "$ROOT_DIR/k8s/base/configmap/app-configmap.yaml" | kubectl apply -f - -n "$NAMESPACE"

echo "✅ ConfigMaps aplicados com sucesso!"
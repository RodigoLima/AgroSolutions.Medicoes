#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
ENV_FILE="$ROOT_DIR/.env"
NAMESPACE="agro-medicoes"

if [ ! -f "$ENV_FILE" ]; then
    echo "❌ Arquivo .env não encontrado em $ENV_FILE"
    exit 1
fi

# Carrega variáveis
set -a
source "$ENV_FILE"
set +a

# Validações mínimas (opcional, mas recomendo)
: "${ASPNETCORE_ENVIRONMENT:?}"

echo "📦 Aplicando ConfigMap do APP"

envsubst < $ROOT_DIR/k8s/base/configmap/app-configmap.yaml | kubectl apply -f - -n "$NAMESPACE"

echo "✅ ConfigMaps aplicados com sucesso!"
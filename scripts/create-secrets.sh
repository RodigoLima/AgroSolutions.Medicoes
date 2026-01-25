#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "$SCRIPT_DIR/.." && pwd)"
ENV_FILE="$ROOT_DIR/.env"
NAMESPACE="agro-medicoes"
RABBIT_SECRET="rabbitmq-auth"
GRAFANA_SECRET="grafana-auth"
DATABASE_SECRET="database-config"

if [ ! -f "$ENV_FILE" ]; then
    echo "❌ Arquivo .env não encontrado em $ENV_FILE"
    exit 1
fi

set -a
source "$ENV_FILE"
set +a

# validações
: "${RABBITMQ_DEFAULT_USER:?Variável RABBITMQ_DEFAULT_USER não definida}"
: "${RABBITMQ_DEFAULT_PASS:?Variável RABBITMQ_DEFAULT_PASS não definida}"

: "${GF_SECURITY_ADMIN_USER:?Variável GF_SECURITY_ADMIN_USER não definida}"
: "${GF_SECURITY_ADMIN_PASSWORD:?Variável GF_SECURITY_ADMIN_PASSWORD não definida}"

: "${DB_HOST:?Variável DB_HOST não definida}"
: "${DB_PORT:?Variável DB_PORT não definida}"
: "${DB_USER:?Variável DB_USER não definida}"
: "${DB_PASSWORD:?Variável DB_PASSWORD não definida}"
: "${DB_NAME:?Variável DB_NAME não definida}"

echo "🔐 Criando secret: $RABBIT_SECRET"
kubectl create secret generic $RABBIT_SECRET \
    -n $NAMESPACE \
    --from-literal=RABBITMQ_DEFAULT_USER="$RABBITMQ_DEFAULT_USER" \
    --from-literal=RABBITMQ_DEFAULT_PASS="$RABBITMQ_DEFAULT_PASS" \
    --dry-run=client -o yaml | kubectl apply -f -

echo "🔐 Criando secret: $GRAFANA_SECRET"
kubectl create secret generic $GRAFANA_SECRET \
    -n $NAMESPACE \
    --from-literal=GF_SECURITY_ADMIN_USER="$GF_SECURITY_ADMIN_USER" \
    --from-literal=GF_SECURITY_ADMIN_PASSWORD="$GF_SECURITY_ADMIN_PASSWORD" \
    --dry-run=client -o yaml | kubectl apply -f -

echo "🔐 Criando secret: $DATABASE_SECRET"
kubectl create secret generic $DATABASE_SECRET \
    -n $NAMESPACE \
    --from-literal=DB_HOST="$DB_HOST" \
    --from-literal=DB_PORT="$DB_PORT" \
    --from-literal=DB_USER="$DB_USER" \
    --from-literal=DB_PASSWORD="$DB_PASSWORD" \
    --from-literal=DB_NAME="$DB_NAME" \
    --dry-run=client -o yaml | kubectl apply -f -

echo "🔐 Criando secret: app-connection-string"
kubectl create secret generic app-connection-string \
    -n $NAMESPACE \
    --from-literal=ConnectionStrings__DefaultConnection="Host=$DB_HOST;Port=$DB_PORT;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASSWORD" \
    --dry-run=client -o yaml | kubectl apply -f -

echo "✅ Secrets aplicados com sucesso!"

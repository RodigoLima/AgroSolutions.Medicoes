# AgroSolutions.Medicoes

Worker de análise e alertas da plataforma AgroSolutions. Consome dados de sensores (umidade, temperatura, precipitação) via RabbitMQ, persiste medições, avalia regras (ex.: alta temperatura, seca) e publica atualização de status do talhão para o PropertyService. Dashboards no Grafana exibem dados históricos e alertas para o produtor.

## Tecnologias

- .NET 8
- PostgreSQL
- MassTransit (RabbitMQ)
- Docker e Kubernetes (Kind)
- Prometheus, Grafana, Tempo, Loki

## Estrutura

```
src/
├── AgroSolutions.Medicoes.Application   (regras de alerta, serviços)
├── AgroSolutions.Medicoes.Domain
├── AgroSolutions.Medicoes.Infrastructure
└── AgroSolutions.Medicoes.Worker         (consumers RabbitMQ, scheduler de regras)
tests/
├── AgroSolutions.Medicoes.Application.Tests
└── AgroSolutions.Medicoes.Domain.Tests
k8s/
├── kind/
└── base/   (app, postgresql, rabbitmq, grafana, prometheus, etc.)
observability/
└── grafana/   (datasources, dashboards)
scripts/
└── deploy.sh
```

## Pré-requisitos

- .NET 8 SDK
- PostgreSQL
- RabbitMQ

## Configuração

Arquivo `.env` na raiz (use `.env.example` como referência). Variáveis típicas: banco, RabbitMQ, OTEL_EXPORTER_OTLP_ENDPOINT, e-mail (opcional).

## Executar

### Docker Compose

```bash
docker build -t agro-solutions-medicoes:latest .
docker compose up -d
```

### Kind (Kubernetes local)

```bash
chmod +x scripts/create-secrets.sh scripts/deploy-configmap.sh scripts/deploy.sh
./scripts/deploy.sh
```

O script cria o cluster Kind, namespace, secrets, configmaps (incluindo Grafana datasources e dashboards), aplica os manifests e faz o deploy da aplicação.

## Funcionalidades

- Consumers: `SensorDataMessage`, `PropriedadeDataMessage`, `TalhaoDataMessage`, `ProdutorDataMessage`
- Motor de alertas: regras por período (ex.: média alta de temperatura); geração de alertas e e-mail ao produtor
- Publicação de `TalhaoStatusUpdateMessage` para o PropertyService (status Normal, Alerta de Seca, Risco de Praga)
- Dashboards Grafana (PostgreSQL): visão por propriedade/talhão, medições históricas, precipitação, status e alertas

## Grafana

Datasources e dashboards são provisionados via configmaps. Acesso típico: `http://localhost:30000` (admin/admin). Dashboards exibem dados de medições e alertas para o produtor.

## Testes e CI/CD

```bash
dotnet test AgroSolutions.Medicoes.sln --configuration Release
```

Pipeline GitHub Actions: CI (build + testes) e CD (build e push da imagem Docker para Docker Hub).

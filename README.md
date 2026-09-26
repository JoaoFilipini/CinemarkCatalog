# 🎬 Cinemark Catalog & Notifications Backend

Backend desenvolvido em **.NET 9** para o sistema de catálogo de filmes e processamento de notificações da Cinemark. O projeto foi construído sob os princípios de **Clean Architecture**, **Domain-Driven Design (DDD)** e **SOLID**, garantindo um código escalável, testável e de fácil manutenção.

---

## 🏗️ Arquitetura da Solução

O projeto está organizado em microsserviços e camadas bem definidas, promovendo alto isolamento de regras de negócio, baixo acoplamento e separação de responsabilidades.

```text
CinemarkCatalog/
│
├── src/
│   ├── Services/
│   │   ├── MovieCatalog/
│   │   │   ├── MovieCatalog.Domain          # Entidades, Enums e Interfaces (Core)
│   │   │   ├── MovieCatalog.Application     # Casos de uso, DTOs e Validadores (FluentValidation)
│   │   │   ├── MovieCatalog.Infra           # Repositórios MongoDB, Cache (Redis) e Mensageria (SQS)
│   │   │   └── MovieCatalog.API             # Controllers, Middlewares e Injeção de Dependências
│   │   │
│   │   └── Notifications/
│   │       └── NotificationConsumer.Service # Worker para consumo de mensagens via AWS SQS
│   │
└── tests/
    ├── MovieCatalog.UnitTests               # Testes unitários do catálogo (xUnit + Moq)
    └── NotificationConsumer.UnitTests       # Testes unitários do consumer de notificação
```

## 🚀 Tecnologias e Padrões Utilizados

- **Linguagem & Framework**: C# e .NET 9
- **Banco de Dados**: MongoDB (Driver oficial, connection pooling configurado, índices criados via `IHostedService` na inicialização)
- **Cache**: Redis (padrão Cache-aside, TTL configurável via `appsettings`, fallback gracioso se o Redis estiver indisponível)
- **Mensageria**: AWS SQS via LocalStack (`AWSSDK.SQS`)
- **Validação**: FluentValidation com regras customizadas
- **Observabilidade**: Serilog (logs estruturados em JSON), Correlation ID por requisição, Health Check (`/health`) no consumer
- **Testes**: xUnit, Moq

---

## 🛠️ Como Executar o Projeto

### Opção 1 — Docker Compose (recomendado)

Sobe MongoDB, Redis, LocalStack (com a fila `film-events` já criada) e os dois serviços .NET.

**Pré-requisitos**: Docker e Docker Compose instalados.

```bash
git clone https://github.com/JoaoFilipini/CinemarkCatalog.git
cd CinemarkCatalog
docker compose up --build
```

Serviços disponíveis após a subida:

| Serviço | URL |
|---|---|
| Movie Catalog API (Swagger) | http://localhost:8080/swagger |
| Notification Consumer (Health Check) | http://localhost:8081/health |
| MongoDB | localhost:27017 |
| Redis | localhost:6379 |
| LocalStack (SQS) | http://localhost:4566 |

### Opção 2 — Rodando localmente

**Pré-requisitos**:
- .NET 9 SDK
- MongoDB, Redis e LocalStack rodando localmente (pode subir só a infra com `docker compose up mongodb redis localstack`)

```bash
git clone https://github.com/JoaoFilipini/CinemarkCatalog.git
cd CinemarkCatalog
dotnet restore
dotnet build
```

**Rodar a API do Catálogo de Filmes:**

```bash
dotnet run --project src/Services/MovieCatalog/MovieCatalog.API/MovieCatalog.API.csproj
```

Swagger disponível em: http://localhost:5215/swagger

**Rodar o Serviço de Notificações (Background Worker):**

```bash
dotnet run --project src/Services/Notifications/NotificationConsumer.Service/NotificationConsumer.Service.csproj
```

---

## 🧪 Executando os Testes

```bash
dotnet test
```

Para gerar relatório de cobertura de código:

```bash
dotnet test --collect:"XPlat Code Coverage"
```

O relatório é gerado em `tests/**/TestResults/*/coverage.cobertura.xml` e pode ser visualizado com ferramentas como [ReportGenerator](https://github.com/danielpalme/ReportGenerator).

---

## 📋 Principais Funcionalidades

- **Catálogo de Filmes (CRUD & Paginação)**: cadastro, atualização, exclusão lógica (soft delete) e listagem paginada com filtros dinâmicos de gênero e status de ativação.
- **Validação de Domínio**: regras de negócio (título obrigatório e único, duração positiva, nota entre 0 e 10 com uma casa decimal, data de lançamento obrigatória) aplicadas via FluentValidation, com erros de validação (400) e conflitos de negócio (400) tratados por middleware global.
- **Cache Inteligente**: consultas por ID e listagens paginadas usam cache-aside no Redis, com invalidação automática em criação, atualização e exclusão, e chaves organizadas por namespace (`films:{id}`, `films:list:*`).
- **Mensageria com AWS SQS**: eventos `FilmCreated`, `FilmUpdated` e `FilmDeleted` publicados de forma assíncrona e não bloqueante, consumidos por um worker dedicado que loga cada evento recebido.
- **Observabilidade**: Correlation ID propagado em toda requisição, logs estruturados em JSON via Serilog, e endpoint `/health` no consumer para monitorar a conexão com o SQS.

---

## 📖 Endpoints Principais

| Método | Rota | Descrição |
|---|---|---|
| POST | `/api/v1/films` | Cria um filme |
| GET | `/api/v1/films/{id}` | Busca um filme por ID (com cache) |
| GET | `/api/v1/films?genre=&active=&page=&pageSize=` | Lista filmes paginados (com cache) |
| PUT | `/api/v1/films/{id}` | Atualiza um filme |
| DELETE | `/api/v1/films/{id}` | Remove um filme (soft delete) |

Exemplos de requisição estão disponíveis em `src/Services/MovieCatalog/MovieCatalog.API/MovieCatalog.API.http`.

Markdown
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
│   │   │   ├── MovieCatalog.Domain          # Entidades, Value Objects e Interfaces (Core)
│   │   │   ├── MovieCatalog.Application     # Casos de uso, DTOs e Validadores (FluentValidation)
│   │   │   ├── MovieCatalog.Infra           # Repositórios MongoDB e Cache (Redis)
│   │   │   └── MovieCatalog.API             # Controllers, Middlewares e Injeção de Dependências
│   │   │
│   │   └── Notifications/
│   │       └── NotificationConsumer.Service # Worker para consumo de mensagens via AWS SQS
│   │
└── tests/
    ├── MovieCatalog.UnitTests               # Testes unitários do catálogo (XUnit + Moq)
    └── NotificationConsumer.UnitTests       # Testes unitários do consumer de notificação
🚀 Tecnologias e Padrões Utilizados
Linguagem & Framework: C# e .NET 9

Banco de Dados de Leitura/Escrita: MongoDB (Driver Oficial, inicialização assíncrona de índices via IHostedService)

Gerenciamento de Cache: Redis (padrão Cache-aside com TTL dinâmico)

Mensageria & Cloud: AWS SQS (AWSSDK.SQS)

Validações de Domínio e Request: FluentValidation

Resiliência & Observabilidade: Serilog (structured JSON logging) e Health Checks customizados (/health)

Testes: XUnit, Moq, FluentAssertions

🛠️ Como Executar o Projeto localmente
Pré-requisitos
.NET 9 SDK instalado

Instância do MongoDB e Redis rodando (podem ser inicializados via Docker)

1. Clonar e Restaurar Dependências
Abra o terminal na pasta raiz do repositório e execute:

Bash
git clone [https://github.com/SEU-USUARIO/CinemarkCatalog.git](https://github.com/SEU-USUARIO/CinemarkCatalog.git)
cd CinemarkCatalog
dotnet restore
dotnet build
2. Rodar a API do Catálogo de Filmes
Este serviço expõe os endpoints REST para o gerenciamento de filmes.

Bash
dotnet run --project src/Services/MovieCatalog/MovieCatalog.API/MovieCatalog.API.csproj
Acesse a documentação do Swagger em: http://localhost:porta/swagger

3. Rodar o Serviço de Notificações (Background Worker)
Este serviço fica "escutando" a fila do AWS SQS e processando as notificações assincronamente.

Bash
dotnet run --project src/Services/Notifications/NotificationConsumer.Service/NotificationConsumer.Service.csproj

🧪 Executando os Testes Unitários
Para garantir a qualidade e a integridade da camada de aplicação e do domínio, execute a suíte de testes:

Bash
dotnet test

📋 Principais Funcionalidades
Catálogo de Filmes (CRUD & Paginação): Cadastro, atualização, exclusão lógica (Soft Delete) e listagem paginada com filtros dinâmicos de gênero e status de ativação.

Segurança e Proteção contra Injeção: Sanitização rigorosa de buscas e proteção nativa contra Regex Injection (uso de Regex.Escape).

Cache Inteligente: Listagens e consultas por ID utilizam cache no Redis, reduzindo a carga no banco de dados para leituras frequentes.

Mensageria com AWS SQS: Processamento assíncrono de notificações de forma resiliente e não bloqueante.

Observabilidade: Endpoints /health integrados para monitoramento em tempo real do microsserviço, conexão com banco e broker de mensagens.

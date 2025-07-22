# 📊 Enquetix

**Sistema de enquetes em tempo real com dashboard interativo**

Enquetix é uma aplicação web moderna para criação, gerenciamento e participação em enquetes (polls) com atualizações em tempo real. O sistema permite que usuários criem enquetes, adicionem opções dinamicamente e acompanhem os resultados conforme os votos são registrados.

![GitHub](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Angular](https://img.shields.io/badge/Angular-20.1-red.svg)

## ✨ Funcionalidades

### 🔐 Sistema de Autenticação

-   **Login/Registro de usuários** com validação de formulários
-   **Autenticação baseada em sessões** com cookies seguros
-   **Middleware de autorização** personalizado
-   **Log de auditoria** para login/logout e alterações

### 📋 Gerenciamento de Enquetes

-   **Criação de enquetes** com título, descrição e período de validade
-   **Adição dinâmica de opções** em tempo real
-   **Edição e exclusão** de enquetes (apenas pelo criador)
-   **Paginação** para navegação eficiente

### 🗳️ Sistema de Votação

-   **Votação em tempo real** com processamento assíncrono
-   **Mudança de voto** permitida
-   **Remoção de votos** pelos usuários
-   **Visualização de resultados** com porcentagens e barras de progresso
-   **Prevenção de votos duplicados** por usuário

### 🔄 Atualizações em Tempo Real

-   **SignalR** para notificações instantâneas
-   **Sincronização automática** de resultados de votação
-   **Notificações** para criação/edição/exclusão de enquetes e opções
-   **Interface reativa** que se atualiza automaticamente

### 🎨 Interface do Usuário

-   **Design responsivo** com Angular 20 e PrimeNG
-   **Tema customizado** com Tailwind CSS
-   **Componentes reutilizáveis** e modulares
-   **Animações suaves** e feedback visual
-   **Experiência de usuário otimizada**

## 🛠️ Tecnologias Utilizadas

### Backend (.NET 9)

-   **ASP.NET Core** - Framework web principal
-   **Entity Framework Core** - ORM para PostgreSQL
-   **SignalR** - Comunicação em tempo real
-   **BCrypt.NET** - Hash de senhas
-   **Redis** - Cache e sessões
-   **RabbitMQ** - Fila de mensagens para processamento assíncrono
-   **MongoDB** - Logs de auditoria
-   **Npgsql** - Driver PostgreSQL

### Frontend (Angular 20)

-   **Angular 20** - Framework principal
-   **PrimeNG** - Biblioteca de componentes UI
-   **Tailwind CSS** - Framework CSS utilitário
-   **SignalR Client** - Cliente para comunicação em tempo real
-   **Angular Reactive Forms** - Formulários reativos

### Infraestrutura

-   **PostgreSQL** - Banco de dados principal
-   **Redis** - Cache e gerenciamento de sessões
-   **RabbitMQ** - Message broker
-   **MongoDB** - Armazenamento de logs
-   **Docker & Docker Compose** - Containerização
-   **Nginx** - Servidor web para frontend

### Ferramentas de Desenvolvimento

-   **xUnit** - Testes unitários
-   **Moq** - Mocking para testes
-   **Node.js** - Testes de stress
-   **EntityFramework Migrations** - Controle de versão do banco

## 🏗️ Arquitetura

### Backend

```
enquetix/
├── Modules/
│   ├── Application/          # Serviços base e infraestrutura
│   │   ├── EntityFramework/  # Contexto e configurações do EF
│   │   ├── Redis/           # Serviços de cache
│   │   ├── MongoDBService   # Conexão MongoDB
│   │   └── RabbitMQService  # Gerenciador de filas
│   ├── Auth/                # Sistema de autenticação
│   │   ├── Controllers/     # Endpoints de login/logout
│   │   ├── Services/        # Lógica de autenticação
│   │   └── Middlewares/     # Middleware de autorização
│   ├── User/                # Gerenciamento de usuários
│   │   ├── Controllers/     # Endpoints CRUD usuários
│   │   ├── Services/        # Lógica de negócio
│   │   ├── Repository/      # Modelos e DTOs
│   │   └── DTOs/           # Objetos de transferência
│   ├── Poll/                # Sistema de enquetes
│   │   ├── Controllers/     # Endpoints de enquetes/opções/votos
│   │   ├── Services/        # Lógica de negócio
│   │   ├── Repository/      # Modelos de dados
│   │   ├── DTOs/           # Objetos de transferência
│   │   └── Hubs/           # SignalR Hubs
│   └── AuditLog/           # Sistema de auditoria
├── Migrations/             # Migrações do banco
└── Properties/            # Configurações da aplicação
```

### Frontend

```
enquetix.Front/src/
├── app/
│   ├── components/         # Componentes reutilizáveis
│   │   ├── login-form/    # Formulário de login/registro
│   │   ├── poll-form/     # Formulário de enquetes
│   │   ├── polls/         # Lista de enquetes
│   │   └── logo/          # Componente do logo
│   ├── pages/             # Páginas da aplicação
│   │   ├── index/         # Página inicial
│   │   ├── poll/          # Visualização de enquete
│   │   └── create-edit-poll/ # Criação/edição de enquete
│   ├── services/          # Serviços Angular
│   │   ├── api.service    # Comunicação com API
│   │   └── signalr.service # Cliente SignalR
│   └── environments/      # Configurações de ambiente
```

## 🚀 Como Executar

### Pré-requisitos

-   **Docker** e **Docker Compose**
-   **.NET 9 SDK** (para desenvolvimento)
-   **Node.js 18+** (para desenvolvimento frontend)
-   **PostgreSQL, Redis, RabbitMQ, MongoDB** (ou usar Docker)

### 🐳 Execução com Docker (Recomendado)

1. **Clone o repositório:**

```bash
git clone https://github.com/Lucas-Gardini/enquetix.git
cd enquetix
```

2. **Execute com Docker Compose:**

```bash
docker compose up --build
```

3. **Acesse a aplicação:**

-   **Frontend:** http://localhost
-   **Backend API:** http://localhost:5261
-   **RabbitMQ Management:** http://localhost:15672 (admin/supersecretpassword)

### 💻 Execução para Desenvolvimento

#### Backend (.NET)

```bash
cd enquetix

# Restaurar ferramentas e dependências
dotnet tool restore
dotnet restore

# Executar migrações
dotnet ef database update

# Executar aplicação
dotnet run
```

#### Frontend (Angular)

```bash
cd enquetix.Front

# Instalar dependências
npm install # ou bun install

# Executar em modo desenvolvimento
npm start # ou bun start
```

#### Serviços de Infraestrutura

```bash
# PostgreSQL
docker run -d --name postgres -p 5432:5432 -e POSTGRES_USER=admin -e POSTGRES_PASSWORD=supersecretpassword -e POSTGRES_DB=enquetix postgres:latest

# Redis
docker run -d --name redis -p 6379:6379 redis:latest redis-server --requirepass supersecretpassword

# RabbitMQ
docker run -d --name rabbitmq -p 5672:5672 -p 15672:15672 -e RABBITMQ_DEFAULT_USER=admin -e RABBITMQ_DEFAULT_PASS=supersecretpassword rabbitmq:management

# MongoDB
docker run -d --name mongo -p 27017:27017 -e MONGO_INITDB_ROOT_USERNAME=admin -e MONGO_INITDB_ROOT_PASSWORD=supersecretpassword mongo:latest
```

## 🧪 Testes

### Testes Unitários

```bash
cd enquetix.Test
dotnet test
```

### Testes de Stress

```bash
npm run test:manual
```

## 📋 Entity Framework

### Adicionar Nova Migration

```bash
dotnet ef migrations add NomeDaMigration
```

### Atualizar Banco de Dados

```bash
dotnet ef database update
```

### Remover Última Migration

```bash
dotnet ef migrations remove
```

## 🔧 Configuração

### Variáveis de Ambiente

```bash
# Strings de Conexão
ConnectionStrings__PostgreSql=Host=localhost;Port=5432;Database=enquetix;Username=admin;Password=supersecretpassword
ConnectionStrings__Redis=localhost:6379,password=supersecretpassword
ConnectionStrings__MongoDB=mongodb://admin:supersecretpassword@localhost:27017
ConnectionStrings__RabbitMQ=amqp://admin:supersecretpassword@localhost:5672
```

### Configuração de CORS

Por padrão, a aplicação permite requisições do frontend em:

-   http://localhost:4200 (desenvolvimento Angular)
-   http://localhost (produção)

## 🤝 Contribuição

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feat/X`)
3. Commit suas mudanças (`git commit -m 'feat: adicionado X'`)
4. Push para a branch (`git push origin feat/X`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE.txt) para mais detalhes.

## 👨‍💻 Autor

**Lucas Gardini**

-   Website: [lucasgardini.com](https://lucasgardini.com)
-   GitHub: [@Lucas-Gardini](https://github.com/Lucas-Gardini)

---

⭐ **Se este projeto foi útil para você, considere dar uma estrela!**

# Super Heroes API

Uma API REST desenvolvida em .NET Core 8 para gerenciamento de super-heróis, implementando operações CRUD completas com arquitetura hexagonal e princípios SOLID. Este projeto demonstra boas práticas de desenvolvimento de software, incluindo separação de responsabilidades, injeção de dependência e validações de negócio.

## 📝 Resumo do Projeto

Este projeto implementa uma API completa para gerenciamento de super-heróis, permitindo cadastrar, consultar, atualizar e excluir heróis e seus superpoderes. A aplicação foi desenvolvida com foco em:

- **Arquitetura Limpa**: Separação clara entre domínio, aplicação, infraestrutura e apresentação
- **Princípios SOLID**: Aplicação dos princípios de design orientado a objetos
- **Validações de Negócio**: Regras de validação implementadas na camada de domínio
- **Testes Unitários**: Cobertura de testes para garantir a qualidade do código
- **Documentação**: API documentada com Swagger/OpenAPI

## 🏗️ Arquitetura

Este projeto foi desenvolvido seguindo os princípios da **Arquitetura Hexagonal** e **SOLID**, organizando o código em camadas bem definidas:

### Estrutura do Projeto

```
SuperHeroesApi/
├── src/
│   ├── SuperHeroesApi.Application/     # Camada de Aplicação
│   │   ├── DTOs/                       # Data Transfer Objects
│   │   └── Mappers/                    # Conversão entre entidades e DTOs
│   │   └── Services/                   # Casos de uso e regras de negócio
│   ├── SuperHeroesApi.Domain/          # Camada de Domínio
│   │   ├── Entities/                   # Entidades de negócio
│   │   └── Interfaces/                 # Contratos dos repositórios
│   ├── SuperHeroesApi.Infrastructure/  # Camada de Infraestrutura
│   │   ├── Data/                       # Contexto do Entity Framework
│   │   └── Extensions/                 # Métodos de extensão para configuração e registro de serviços de infraestrutura
│   │   └── Repositories/               # Implementações dos repositórios
│   └── SuperHeroesApi.WebAPI/          # Camada de Apresentação
│       └── Controllers/                # Controllers da API REST
└── tests/
    └── SuperHeroesApi.Tests/           # Testes unitários
```

## 🧩 Modelo de Domínio

O projeto é composto pelas seguintes entidades principais:

### Hero

- Representa um super-herói com atributos como nome, nome de herói, data de nascimento, altura e peso
- Implementa validações de negócio para garantir a integridade dos dados
- Possui relacionamento muitos-para-muitos com Superpowers

### Superpower

- Representa um superpoder com nome e descrição
- Pode ser associado a múltiplos heróis

### HeroSuperpower

- Entidade de associação que implementa o relacionamento muitos-para-muitos entre Hero e Superpower

## 🛠️ Tecnologias Utilizadas

- **.NET Core 8**: Framework principal
- **Entity Framework Core**: ORM para acesso a dados
- **SQLite**: Banco de dados relacional, com esquema versionado via EF Core Migrations
- **Asp.Versioning**: Versionamento de API por segmento de URL (ex.: `/api/v1/heroes`)
- **Health Checks**: Endpoint `/health` para monitoramento de disponibilidade da API e do banco
- **Swagger/OpenAPI**: Documentação automática da API com suporte a XML comments
- **XUnit**: Framework de testes unitários
- **ASP.NET Core**: Framework web para APIs REST

## 📋 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Editor de código (Visual Studio, VS Code, etc.)

## 💾 Banco de Dados

Este projeto utiliza o **SQLite** como banco de dados relacional, com o esquema controlado por
**EF Core Migrations**:

- **Persistência real em arquivo** - Os dados são armazenados em `superheroes.db` (produção) ou
  `superheroes.dev.db` (desenvolvimento), configurável via `ConnectionStrings:DefaultConnection`
- **Migrations aplicadas automaticamente** - Ao iniciar, a aplicação executa `Database.Migrate()`
  para criar/atualizar o esquema
- **Seed Data** - 10 superpoderes são pré-cadastrados automaticamente via migration
- **Relacionamentos** - Suporta relacionamentos complexos entre entidades, com índice único no
  nome de herói

### Gerando novas migrations

```bash
cd src/SuperHeroesApi.Infrastructure
dotnet ef migrations add NomeDaMigration --output-dir Data/Migrations
```

### Testes

Os testes de repositório/serviço utilizam o provedor **InMemory** do EF Core para isolamento e
velocidade; testes dedicados de infraestrutura (`SqliteInfrastructureTests`) validam as
migrations e o comportamento real do SQLite.

## 🚀 Como Executar

### 1. Clone o repositório

```bash
git clone https://github.com/MarcosMuriloPJ/SuperHeroesApi.git
cd SuperHeroesApi
```

### 2. Restaure as dependências

```bash
dotnet restore
```

### 3. Execute os testes

```bash
dotnet test
```

### 4. Execute a aplicação

```bash
cd src/SuperHeroesApi.WebAPI
dotnet run
```

A API estará disponível em:

- **HTTP**: http://localhost:5000
- **HTTPS**: https://localhost:7183
- **Swagger UI**: http://localhost:5000 ou https://localhost:7183 (dependendo da configuração)

## 📚 Documentação da API

Todas as rotas são versionadas por segmento de URL (ex.: `/api/v1/heroes`). A versão atual é a `v1`.

### Endpoints Disponíveis

#### Super-heróis

- `GET /api/v1/heroes` - Lista super-heróis de forma paginada, com filtros e ordenação

  - Parâmetros de query: `page` (padrão 1), `pageSize` (padrão 10, máx. 50), `name`, `heroName`,
    `superpowerId`, `sortBy` (`Name`, `HeroName`, `Birthdate`, `Height` ou `Weight`) e
    `sortDescending`
  - Retorna um objeto paginado: `{ items, page, pageSize, totalCount, totalPages }`

- `GET /api/v1/heroes/{id}` - Obtém um super-herói por ID

  - Retorna detalhes completos de um herói específico
  - Retorna 404 quando o herói não é encontrado
  - Retorna 400 para IDs inválidos

- `POST /api/v1/heroes` - Cadastra um novo super-herói

  - Valida todos os campos obrigatórios
  - Verifica se o nome de herói já existe
  - Valida se os superpoderes informados existem
  - Retorna o herói criado com seu ID gerado

- `PUT /api/v1/heroes/{id}` - Atualiza um super-herói existente

  - Permite atualização parcial ou completa dos dados
  - Mantém as mesmas validações do cadastro
  - Retorna 404 quando o herói não é encontrado

- `DELETE /api/v1/heroes/{id}` - Exclui um super-herói
  - Remove o herói e suas associações com superpoderes
  - Retorna 404 quando o herói não é encontrado

#### Superpoderes

- `GET /api/v1/superpowers` - Lista todos os superpoderes disponíveis
  - Retorna nome, descrição e ID de cada superpoder

#### Monitoramento

- `GET /health` - Health check da aplicação e da conectividade com o banco de dados

### Exemplo de Uso

#### Cadastrar um novo herói:

```bash
curl -X POST http://localhost:5000/api/v1/heroes \
  -H "Content-Type: application/json" \
  -d '{
    "nome": "Clark Kent",
    "nomeHeroi": "Superman",
    "dataNascimento": "1980-06-18T00:00:00",
    "altura": 1.85,
    "peso": 80.0,
    "superpoderesIds": [1, 2]
  }'
```

#### Listar heróis paginados, filtrados e ordenados:

```bash
curl "http://localhost:5000/api/v1/heroes?page=1&pageSize=5&heroName=man&sortBy=HeroName&sortDescending=false"
```

## 🧪 Testes

O projeto inclui testes unitários abrangentes:

```bash
# Executar todos os testes
dotnet test

# Executar testes com relatório de cobertura
dotnet test --collect:"XPlat Code Coverage"
```

### Cobertura de Testes

- ✅ Testes de entidades de domínio
- ✅ Testes de serviços de aplicação
- ✅ Testes de repositórios
- ✅ Testes de validações de negócio
- ✅ Testes de cenários de erro

## 🔒 Validações e Regras de Negócio

### Validações Implementadas:

1. **Campos obrigatórios**: Nome, Nome do Herói, Data de Nascimento, Altura, Peso, Superpoderes
2. **Nome do herói único**: Não permite dois heróis com o mesmo nome
3. **Data de nascimento**: Não pode ser no futuro
4. **Altura e peso**: Devem ser valores positivos
5. **Superpoderes**: Pelo menos um deve ser selecionado e deve existir no banco

### Tratamento de Erros:

- **400 Bad Request**: Dados inválidos ou malformados
- **404 Not Found**: Recurso não encontrado
- **409 Conflict**: Conflito de dados (ex: nome de herói duplicado)
- **500 Internal Server Error**: Erros internos do servidor

## 🔄 Fluxo de Dados

1. **Requisição HTTP** → Controller
2. **Controller** → Application Service (DTO)
3. **Application Service** → Domain Repository (Interface)
4. **Repository** → Infrastructure (Entity Framework)
5. **Database** ← Entity Framework
6. **Response** ← Controller (DTO)

## 🧠 Camada de Aplicação

A camada de aplicação contém os serviços que implementam os casos de uso da aplicação:

### HeroService

- Implementa operações CRUD para heróis
- Realiza validações de negócio antes de persistir os dados
- Converte entre entidades de domínio e DTOs
- Gerencia o relacionamento entre heróis e superpoderes

### DTOs (Data Transfer Objects)

- **HeroDto**: Representa um herói para exibição
- **CreateHeroDto**: Contém dados para criação de um novo herói
- **UpdateHeroDto**: Contém dados para atualização de um herói existente
- **SuperpowerDto**: Representa um superpoder para exibição

## 👥 Contribuição

Este projeto foi desenvolvido como parte de um desafio técnico, demonstrando:

- Conhecimento em arquitetura de software
- Aplicação de princípios SOLID
- Desenvolvimento orientado a testes (TDD)
- Boas práticas de desenvolvimento .NET
- Documentação técnica completa

## 📄 Licença

Este projeto é de uso demonstrativo.

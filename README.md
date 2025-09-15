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
- **Entity Framework InMemory**: Banco de dados em memória para desenvolvimento e testes
- **Swagger/OpenAPI**: Documentação automática da API com suporte a XML comments
- **XUnit**: Framework de testes unitários
- **ASP.NET Core**: Framework web para APIs REST

## 📋 Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- Editor de código (Visual Studio, VS Code, etc.)

## 💾 Banco de Dados

Este projeto utiliza o **Entity Framework Core InMemory** como provedor de banco de dados, o que significa que:

- **Não é necessário configurar um servidor de banco de dados** - Todos os dados são armazenados em memória
- **Dados são inicializados automaticamente** - O banco é populado com dados de exemplo ao iniciar a aplicação
- **Ideal para desenvolvimento e testes** - Facilita o desenvolvimento sem dependências externas
- **Configuração otimizada** - Inclui EnableSensitiveDataLogging e NoTracking para melhor desempenho

### Características do Banco em Memória

- **Persistência temporária** - Os dados existem apenas durante a execução da aplicação
- **Seed Data** - 10 superpoderes são pré-cadastrados automaticamente
- **Relacionamentos** - Suporta relacionamentos complexos entre entidades
- **Testes isolados** - Cada teste utiliza uma instância isolada do banco de dados

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

### Endpoints Disponíveis

#### Super-heróis

- `GET /api/heroes` - Lista todos os super-heróis

  - Retorna uma lista completa de heróis com seus respectivos superpoderes
  - Suporta resposta vazia quando não há heróis cadastrados

- `GET /api/heroes/{id}` - Obtém um super-herói por ID

  - Retorna detalhes completos de um herói específico
  - Retorna 404 quando o herói não é encontrado
  - Retorna 400 para IDs inválidos

- `POST /api/heroes` - Cadastra um novo super-herói

  - Valida todos os campos obrigatórios
  - Verifica se o nome de herói já existe
  - Valida se os superpoderes informados existem
  - Retorna o herói criado com seu ID gerado

- `PUT /api/heroes/{id}` - Atualiza um super-herói existente

  - Permite atualização parcial ou completa dos dados
  - Mantém as mesmas validações do cadastro
  - Retorna 404 quando o herói não é encontrado

- `DELETE /api/heroes/{id}` - Exclui um super-herói
  - Remove o herói e suas associações com superpoderes
  - Retorna 404 quando o herói não é encontrado

#### Superpoderes

- `GET /api/superpowers` - Lista todos os superpoderes disponíveis
  - Retorna nome, descrição e ID de cada superpoder

### Exemplo de Uso

#### Cadastrar um novo herói:

```bash
curl -X POST http://localhost:5000/api/heroes \
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

#### Listar todos os heróis:

```bash
curl http://localhost:5000/api/heroes
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

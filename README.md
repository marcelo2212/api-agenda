# Agenda de Contatos - API .NET 8

API desenvolvida em .NET 8 com arquitetura em camadas (API, Application, Domain, Infrastructure), mensageria assíncrona com RabbitMQ, Entity Framework Core com PostgreSQL e validação com FluentValidation.

---

## Tecnologias Utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- RabbitMQ
- MediatR (CQRS)
- AutoMapper
- FluentValidation
- Dapper
- Docker + Docker Compose
- xUnit + Moq (Testes automatizados)

---

## Funcionalidades

- CRUD completo de contatos via filas RabbitMQ:
  - `contacts.create` – Criação de contato
  - `contacts.getall` – Listagem de todos os contatos
  - `contacts.getbyid` – Busca por ID
  - `contacts.update` – Atualização de contato
  - `contacts.delete` – Exclusão de contato

- Arquitetura desacoplada com `BackgroundService` para cada consumer.

---

## Estrutura do Projeto
```bash
├── Agenda.API/ # Camada de apresentação (Swagger, Startup)
├── Agenda.Application/ # Casos de uso, validação, DTOs e comandos
├── Agenda.Domain/ # Entidades e interfaces de domínio
├── Agenda.Infrastructure/ # Acesso a dados, configuração RabbitMQ, EF, Dapper
├── Agenda.Tests/ # Testes unitários e de integração
```


---

## Pré-requisitos

- .NET SDK 8+
- Docker e Docker Compose
- PostgreSQL (usado via Docker)
- RabbitMQ (usado via Docker)

---

## Executando com Docker

```bash
docker-compose up
```
---

## Acessos Locais

- API Swagger: http://localhost:5000/swagger

- RabbitMQ Management UI: http://localhost:15672

  - Usuário: guest
  - Senha: guest

- PostgreSQL: localhost:5432

- Banco: agenda

  - Usuário: postgres
  - Senha: postgres
  - 
---

## Rodando os Testes

```bash
dotnet test
```


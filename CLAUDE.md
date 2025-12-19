# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Endatix is an open-source form management backend API designed to integrate with SurveyJS frontend libraries. It provides REST API endpoints for CRUD operations on forms, templates, submissions, themes, and custom form fields with multi-tenancy support.

## Build and Test Commands

```bash
# Build the entire solution
dotnet build

# Run all tests
dotnet test

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage;Format=cobertura"

# Run a specific test project
dotnet test tests/Endatix.Core.Tests
dotnet test tests/Endatix.Api.Tests
dotnet test tests/Endatix.Infrastructure.Tests

# Run a single test by name filter
dotnet test --filter "FullyQualifiedName~FormTests"

# Run the WebHost for local development (uses SQL Server)
dotnet run --project src/Endatix.WebHost
```

## Docker

```bash
# Start all services (API, Hub, SQL Server)
docker-compose up -d

# API runs on port 5001, Hub on port 3000, SQL Server on port 1433
```

## Architecture

The solution follows **Clean Architecture** with **Domain-Driven Design** and **Vertical Slice Architecture**:

### Project Dependencies (inner to outer)
- **Endatix.Core** - Domain layer with entities, aggregate roots, domain events, use cases (Commands/Queries via MediatR). No dependencies.
- **Endatix.Framework** - Shared abstractions and extensibility points. No dependencies.
- **Endatix.Infrastructure** - External integrations (EF Core, Identity, Email). Depends on Core & Framework.
- **Endatix.Persistence.SqlServer** / **Endatix.Persistence.PostgreSql** - Database-specific implementations. Depend on Infrastructure.
- **Endatix.Api** - FastEndpoints-based REST API endpoints. Depends on Core & Infrastructure.
- **Endatix.Api.Host** - API hosting configuration as NuGet package.
- **Endatix.Hosting** - Extension methods for easy setup (`ConfigureEndatix()`, `UseEndatix()`).
- **Endatix.WebHost** - Minimal startup project for debugging/testing.

### Key Patterns

**Use Cases (CQRS with MediatR)**
- Located in `src/Endatix.Core/UseCases/{Feature}/{Operation}/`
- Commands for write operations: `{Operation}Command.cs` + `{Operation}Handler.cs`
- Queries for read operations: `{Operation}Query.cs` + `{Operation}Handler.cs`
- Each returns `Result<T>` using Ardalis.Result pattern

**API Endpoints (FastEndpoints)**
- Located in `src/Endatix.Api/Endpoints/{Feature}/`
- Each endpoint is a class with request, response, and validator files
- Naming: `{Operation}.cs`, `{Operation}.{Operation}Request.cs`, `{Operation}.{Operation}Validator.cs`

**Domain Entities**
- Located in `src/Endatix.Core/Entities/`
- Core entities: Form, FormDefinition, Submission, Theme, FormTemplate, CustomQuestion
- All inherit from `TenantEntity` for multi-tenant isolation

**Domain Events**
- Located in `src/Endatix.Core/Events/`
- Handlers in `src/Endatix.Core/Handlers/`
- Events: FormCreated, FormUpdated, FormDeleted, SubmissionCompleted, etc.

**Specifications (Ardalis.Specification)**
- Located in `src/Endatix.Core/Specifications/`
- Used for complex queries with filtering, paging, and includes

## Configuration

Configuration is in `appsettings.json` under the `Endatix` section:
- `Auth.Providers.EndatixJwt` - JWT settings
- `Data.EnableAutoMigrations` - Database migration settings
- `Cors.CorsPolicies` - CORS configuration
- `WebHooks.Tenants` - Webhook configuration per tenant
- `Integrations.Email` - SendGrid/SMTP settings

## Testing

- Uses **xUnit v3** with **FluentAssertions** and **NSubstitute** for mocking
- Test naming: `{MethodName}_{Scenario}_{ExpectedResult}`
- API tests use **FastEndpoints.Testing** for endpoint testing
- Tests mirror source structure: `tests/Endatix.{Project}.Tests/`

## Tech Stack

- .NET 10.0
- Entity Framework Core 10
- FastEndpoints for API
- MediatR for CQRS
- FluentValidation
- Serilog for logging
- PostgreSQL or SQL Server

## Hosting Setup

Minimal setup in `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureEndatix();
var app = builder.Build();
app.UseEndatix();
app.Run();
```

Custom configuration:
```csharp
builder.Host.ConfigureEndatix(endatix => {
    endatix.UseSqlServer<AppDbContext>();
    endatix.Infrastructure.Messaging.Configure(options => {
        options.IncludeLoggingPipeline = true;
    });
});
```

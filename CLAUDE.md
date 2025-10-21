# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Endatix Platform is an open-source data collection and management library for .NET, designed for building secure, scalable form-centric applications that work with SurveyJS. The codebase uses **Clean Architecture**, **Domain-Driven Design (DDD)**, and **Vertical Slice Architecture**.

**Key technologies**: .NET 9.0, Entity Framework Core 9, FastEndpoints, MediatR, Serilog, NSwag (Swagger)

## Architecture

### Project Structure & Dependencies

The solution follows clean architecture dependency rules where inner layers have no dependencies on outer layers:

- **Endatix.Core** - Core domain layer with entities, aggregates, domain events, and use cases. **No external dependencies**.
- **Endatix.Framework** - Common plugin and extensibility points. **No dependencies**.
- **Endatix.Infrastructure** - Infrastructure implementations for Core abstractions. Depends on Core & Framework.
- **Endatix.Persistence.SqlServer** - MS SQL Server specific database logic. Depends on Infrastructure.
- **Endatix.Persistence.PostgreSql** - PostgreSQL specific database logic. Depends on Infrastructure.
- **Endatix.Api** - REST API endpoints using FastEndpoints. Depends on Core & Infrastructure.
- **Endatix.Hosting** - Hosting utilities with builder pattern for configuration. Depends on Framework, Infrastructure & persistence packages.
- **Endatix.Api.Host** - Zero-code host project combining all packages for easy deployment.
- **Endatix.WebHost** - Zero-code debugging/testing host. Depends on Hosting & Api.

### Key Architectural Patterns

**Vertical Slice Architecture**: Features are organized by business capability rather than technical concerns:
- `src/Endatix.Core/UseCases/` - Business use cases organized by feature (FormDefinitions, Submissions, Tenants, etc.)
- `src/Endatix.Api/Endpoints/` - API endpoints organized by feature matching use cases
- Each feature contains its own handlers, validators, and models

**Domain Events & MediatR**:
- Domain events defined in `src/Endatix.Core/Events/`
- Event handlers in `src/Endatix.Core/Handlers/`
- Extensible event system for webhooks and custom automation

**Specifications Pattern**:
- Specifications in `src/Endatix.Core/Specifications/` using Ardalis.Specification
- Used for reusable query logic across repositories

**Identity & Multi-tenancy**:
- Custom ASP.NET Core Identity in `src/Endatix.Infrastructure/Identity/`
- Multi-tenant support via `ITenantOwned` interface and `TenantEntity` base class
- Multiple authentication providers supported (JWT, OAuth, etc.)
- Tenant management APIs available at `/tenants` (Create, List, Get, Update, Delete)

## Common Development Commands

### Building & Testing

```bash
# Restore dependencies
dotnet restore

# Build entire solution
dotnet build

# Build specific project
dotnet build src/Endatix.Core/Endatix.Core.csproj

# Run all tests with coverage
dotnet test --collect:"XPlat Code Coverage;Format=cobertura" --results-directory:".coverage"

# Run tests for specific project
dotnet test tests/Endatix.Core.Tests/Endatix.Core.Tests.csproj

# Run a single test
dotnet test --filter "FullyQualifiedName~MethodName_StateUnderTest_ExpectedBehavior"
```

### Running the Application

```bash
# Run the default web host (development)
dotnet run --project src/Endatix.WebHost/Endatix.WebHost.csproj

# Run with specific environment
dotnet run --project src/Endatix.WebHost/Endatix.WebHost.csproj --environment Production
```

**Default endpoints**:
- API: http://localhost:5000 or https://localhost:5001
- Swagger UI: https://localhost:5001/swagger

### Database Management

**Connection string** (SQL Server by default): Configured in `appsettings.json` under `ConnectionStrings:DefaultConnection`

**Auto-migrations**: Enabled by default via `Endatix.Data.EnableAutoMigrations` configuration setting

```bash
# Create a new migration (from root directory)
dotnet ef migrations add MigrationName --project src/Endatix.Persistence.SqlServer --startup-project src/Endatix.WebHost

# For PostgreSQL
dotnet ef migrations add MigrationName --project src/Endatix.Persistence.PostgreSql --startup-project src/Endatix.WebHost

# Manually apply migrations
dotnet ef database update --project src/Endatix.Persistence.SqlServer --startup-project src/Endatix.WebHost
```

### Docker Setup

```bash
# Start all services (API + Database)
docker compose up -d

# Stop services
docker compose stop

# View logs
docker compose logs

# Teardown
docker compose down
```

## Configuration

### Central Package Management

This solution uses **Central Package Management**:
- Package versions defined in `Directory.Packages.props`
- `ManagePackageVersionsCentrally` is enabled
- When adding packages, only specify package name in `.csproj` - version comes from central file

### Key Configuration Sections

**appsettings.json structure**:
```json
{
  "ConnectionStrings": { ... },
  "Serilog": { ... },
  "Endatix": {
    "Auth": { ... },           // JWT, OAuth providers
    "Data": { ... },            // Migrations, seeding
    "Cors": { ... },
    "WebHooks": { ... },
    "Integrations": { ... },    // Email (SendGrid, SMTP)
    "EmailTemplates": { ... }
  }
}
```

**Configuration precedence**: appsettings.json values override code defaults, which override class defaults.

## Testing Guidelines

### Testing Stack
- **XUnit** (v3) - Test framework
- **NSubstitute** - Mocking
- **FluentAssertions** - Assertions

### Test Naming Convention
`MethodName_StateUnderTest_ExpectedBehavior`

### Test Organization
- Tests mirror the structure of the code they test
- Start with failing cases, then successful cases
- Sample test data available in `SampleData` class constants

### Key Testing Rules
1. Use file-scoped namespaces with semicolon style
2. Assert objects are not null before asserting properties
3. For Guard clause exceptions: use `ErrorMessages.GetErrorMessage(fieldName, ErrorType.Xxx)`
4. When mock result is used, don't check for `Received()` call
5. When mock result is not used, check for `Received()` call with correct parameters
6. Don't assert types when clear from method signature
7. Arrange variables before method calls - don't pass literals directly
8. Use `var` for local variables when type is clear

### Test Example Template
See `tests/README.md` for the full Cursor AI prompt template to generate tests following project conventions.

## Code Organization Standards

### File-scoped Namespaces
Always use file-scoped namespace declarations:
```csharp
namespace Endatix.Core.Entities;

public class MyEntity { ... }
```

### Entity Framework Configuration
- Entity configurations in `src/Endatix.Infrastructure/Data/Config/`
- Use `IEntityTypeConfiguration<T>` pattern
- Database-specific configurations in respective persistence projects

### API Endpoints (FastEndpoints)
- Endpoint classes in `src/Endatix.Api/Endpoints/[Feature]/`
- Group related endpoints by feature (Auth, Forms, Submissions, Tenants, etc.)
- Use FastEndpoints conventions for versioning, validation, and security
- All endpoints return typed results using `TypedResultsBuilder` for consistent error handling
- Endpoint signature types must match `SetTypedResults<>` order for implicit conversion to work

### Domain Events
- Event definitions: `src/Endatix.Core/Events/`
- Event handlers: `src/Endatix.Core/Handlers/` and `src/Endatix.Infrastructure/Features/`
- Webhook integration via configuration in `Endatix:WebHooks`

## Authentication & Authorization

**Multiple provider support**: Configured in `Endatix:Auth:Providers` section
- Default provider: EndatixJwt (built-in JWT)
- Extensible to support OAuth, OIDC, etc.

**User context**:
- Access via `IUserContext` abstraction
- Provides current user ID, email, roles, tenant info

**Role-based security**:
- Roles defined in `RoleNames` class
- Claims in `ClaimNames` class

## Extensibility & Plugins

**Plugin initialization**: Implement `IPluginInitializer` interface
- Register in DI during startup
- Accessed via `Endatix.Framework` package

**Custom event handlers**: See `samples/Endatix.Samples.CustomEventHandlers` for examples

**Configuration sections**: Implement `IHasConfigSection` to auto-bind configuration

## Sample Projects

- **Endatix.Samples.SelfHosted** - NuGet package integration example
- **Endatix.Samples.WebApp** - Project reference integration example
- **Endatix.Samples.CustomEventHandlers** - Custom automation workflows

## Important Development Notes

- **Nullable reference types enabled**: All projects have `<Nullable>enable</Nullable>`
- **Implicit usings enabled**: Common namespaces auto-imported
- **Snowflake ID generation**: Used for all entity IDs via `IIdGenerator`
- **Email verification**: Token-based system in `src/Endatix.Infrastructure/Identity/EmailVerification/`
- **Data export**: CSV export functionality in `src/Endatix.Infrastructure/Exporting/`

## Hosting Patterns

The recommended hosting setup uses builder pattern:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Host.ConfigureEndatix(endatix => endatix
    .WithApi(api => api.AddSwagger().AddVersioning())
    .WithPersistence(db => db.UseSqlServer<AppDbContext>().EnableAutoMigrations())
    .WithSecurity(security => security.UseJwtAuthentication()));

var app = builder.Build();
app.UseEndatix();
app.Run();
```

Simpler version (all defaults):
```csharp
builder.Host.ConfigureEndatix();
app.UseEndatix();
```

## Multi-Tenancy

The application has built-in multi-tenancy support with full CRUD APIs for tenant management.

### Tenant Management

**Available Endpoints** (all require authentication):
- `POST /tenants` - Create a new tenant
- `GET /tenants` - List all tenants
- `GET /tenants/{id}` - Get a specific tenant by ID
- `PUT /tenants/{id}` - Update tenant details (name, description)
- `DELETE /tenants/{id}` - Soft delete a tenant

**Tenant Entity** ([src/Endatix.Core/Entities/Tenant.cs](src/Endatix.Core/Entities/Tenant.cs)):
- Properties: `Id`, `Name`, `Description`
- Inherits from `BaseEntity` (soft delete support, audit fields)
- Update methods: `UpdateName()`, `UpdateDescription()`

**Use Cases** ([src/Endatix.Core/UseCases/Tenants/](src/Endatix.Core/UseCases/Tenants/)):
- Each operation has a command/query and handler following CQRS pattern
- Create, List, Get, Update, Delete

**API Implementation** ([src/Endatix.Api/Endpoints/Tenants/](src/Endatix.Api/Endpoints/Tenants/)):
- FastEndpoints with typed request/response models
- Consistent error handling via `TypedResultsBuilder`
- Returns appropriate HTTP status codes (200 OK, 201 Created, 404 NotFound, 400 BadRequest)

### Testing Tenant APIs

**Via Swagger UI**:
1. Run the application: `dotnet run --project src/Endatix.WebHost/Endatix.WebHost.csproj`
2. Navigate to https://localhost:5001/swagger
3. Find the "Tenants" section with all CRUD operations

**Via curl**:
```bash
# List all tenants
curl -X GET "https://localhost:5001/tenants" -H "accept: application/json"

# Create a new tenant
curl -X POST "https://localhost:5001/tenants" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{"name":"Acme Corp","description":"Primary tenant for Acme Corporation"}'

# Get tenant by ID
curl -X GET "https://localhost:5001/tenants/1" -H "accept: application/json"

# Update tenant
curl -X PUT "https://localhost:5001/tenants/1" \
  -H "accept: application/json" \
  -H "Content-Type: application/json" \
  -d '{"name":"Acme Corporation","description":"Updated description"}'

# Delete tenant (soft delete)
curl -X DELETE "https://localhost:5001/tenants/1" -H "accept: application/json"
```

### Local Development with Azure PostgreSQL

To test locally with the Azure PostgreSQL database, update [src/Endatix.WebHost/appsettings.Development.json](src/Endatix.WebHost/appsettings.Development.json):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=caif-backend-db.postgres.database.azure.com;Database=endatix;Username=caifadmin;Password=XUP747Nr5sih7LW4ydrR;SslMode=Require",
    "DefaultConnection_DbProvider": "PostgreSql"
  }
}
```

---
applyTo: '**/Company.API.Presentation.*/**/*.cs, **/Company.API.WebServices.*/**/*.cs, **/Company.API.WebApp.*/**/*.cs, **/Company.API.Domain.*/**/*.cs, **/Company.API.Application.*/**/*.cs, **/Company.API.Infrastructure.*/**/*.cs'
---

# Clean Architecture

This repository follows Clean Architecture with strict layer separation. When adding new code, use this guide to determine the correct layer and project.

## Layer Mapping

| Layer | Project Pattern |
|-------|----------------|
| Domain | `**/Company.API.Domain.*` |
| Application | `**/Company.API.Application.*` |
| Infrastructure | `**/Company.API.Infrastructure.*` |
| Presentation | `**/Company.API.WebApp.*` |

## Dependency Rule

Dependencies flow **inward only**:

```
Presentation → Application → Domain
Infrastructure → Application → Domain
```

Infrastructure and Presentation both depend on Application, but never on each other. Domain has zero outward dependencies.

## What Goes Where

### Domain (`Company.API.Domain.*`)
- Dto Models
- Request Models
- Response Models
- Value objects
- Enums
- Domain-specific exceptions and errors
- Validation attributes and rules
- Domain constants and options (strongly-typed configuration models)

**Examples**: `UserDto.cs`, `CreateOrderRequest.cs`, `OrderStatusEnum.cs`, `InvalidOrderException.cs`, `EmailAddress.cs` (value object)

### Application (`Company.API.Application.*`)
- Service interfaces (ports) that Infrastructure will implement
- Application service implementations (use case orchestration)
- Object mapping definitions (e.g., Mapperly mappers)
- Extension methods that support application logic
- Factory classes for constructing domain objects
- Attribute definitions used by application services

**Examples**: `IOrderRepository.cs` (interface), `OrderService.cs` (application service), `OrderMapper.cs`, `IEmailService.cs` (port for Infrastructure)

### Infrastructure (`Company.API.Infrastructure.*`)
- Repository implementations (Entity Framework)
- DbContext and entity configurations
- External HTTPS service clients
- Caching implementations
- Messaging/queue producers and consumers (e.g., Azure Service Bus)
- Health check implementations
- Infrastructure-specific models (e.g., EF entity models distinct from domain models)
- Infrastructure mappers (e.g., Mapperly mappers for EF entities ↔ domain models)
- Sandbox implementations (see below)

**Examples**: `OrderRepository.cs`, `ApplicationDbContext.cs`, `OrderEntityConfiguration.cs`, `EmailService.cs`, `PaymentGatewayClient.cs`, `DatabaseHealthCheck.cs`

### Sandbox Implementations (`**/Sandbox/*`)

Files under `Sandbox/` folders within Infrastructure are mock implementations for the deployed sandbox environment. They implement Application interfaces but mock external network calls (third-party APIs, external services) instead of reaching real resources. Sandbox classes must mirror the interface contract, avoid real network calls or external side effects, but may access the database via Entity Framework.

### Presentation (`Company.API.WebApp.*`)
- API controllers and minimal API endpoints
- Middleware
- Startup/extension methods for registering services (`IServiceCollection` extensions)
- Swagger/OpenAPI configuration
- Authentication and authorization setup
- HTTPS request/response models specific to the API surface

**Examples**: `OrdersController.cs`, `Program.cs`, `ExceptionHandlingMiddleware.cs`, `ServiceCollectionExtensions.cs`

## Dependency Injection Wiring

The Presentation layer wires all dependencies via dependency injection in `Program.cs` or extension methods. Application defines interfaces, Infrastructure implements them, and Presentation connects them:

```csharp
// In Company.API.WebApp.*/Program.cs or extension methods
builder.Services.AddApplicationServices();    // Registers Application layer services
builder.Services.AddInfrastructureServices();  // Registers Infrastructure implementations
```

**Example**: `IOrderRepository` (Application) → `OrderRepository` (Infrastructure) → wired via `services.AddScoped<IOrderRepository, OrderRepository>()` in Infrastructure's `AddInfrastructureServices()`.

## Common Pitfalls

- **Do not** place infrastructure implementations (EF Core `DbContext`, repositories) or service classes in Domain or Application. Domain contains only data structures and embedded domain logic; services belong in Application or Infrastructure.
- **Do not** place business logic in controllers or infrastructure repositories — keep business logic in Domain entities or Application services.
- **Do not** reference Infrastructure projects from Application — Application defines interfaces, Infrastructure implements them, Presentation wires them via DI.
- **Do not** place HTTP-specific types (e.g., `IHttpContextAccessor`, `HttpClient`) in Domain or Application.

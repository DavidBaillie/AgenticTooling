---
name: controller-service-workflow
description: 'Controller-Service-Repository workflow pattern for ASP.NET database-backed operations. Use when creating new controller actions that perform database work, refactoring existing controller/repository code to add a service layer, or reviewing architectural compliance. Ensures HTTP concerns stay in controllers, business logic in services, and persistence in repositories.'
---

# Controller Service Workflow

Use this skill whenever creating, updating, or reviewing a controller action that performs database-backed work.

## Quick Reference

- **Complete Example**: See [classroom-workflow-example.cs](examples/classroom-workflow-example.cs) for a full working implementation
- **Common Mistakes**: See [anti-patterns.md](references/anti-patterns.md) for what NOT to do

## Architecture Flow

The expected flow is:

```text
Controller -> Application service interface -> Application service -> Application repository interface -> Infrastructure repository -> database
```

This keeps HTTP concerns in the Presentation layer, business decisions in the Application layer, and database persistence in the Infrastructure layer.

## Controller Rules

- Controllers must depend on service interfaces, not repository interfaces.
- Controllers translate HTTP input into service calls and map service results into HTTP responses.
- Controllers should not perform business validation, database checks, state-transition decisions, or persistence logic.
- If an existing controller injects a repository for CRUD/database actions, refactor it to inject a service interface instead.
- Keep controller result mapping stable when introducing the service layer unless the user explicitly asks for behavior changes.

Example controller dependency:

```csharp
public sealed class ClassroomController(IClassroomService service) : ControllerBase
```

## Service Rules

- Every database-backed controller workflow should have a service class in the Application layer.
- Every service class must have a matching interface in the Application layer.
- Service interfaces use the `I` prefix and `Service` postfix, for example `IClassroomService`.
- Service implementations use the `Service` postfix, for example `ClassroomService`.
- Service classes perform all business validation and orchestration.
- Service classes handle business validation and throw appropriate exceptions for validation failures.
- Service methods may forward simple read/create operations directly when no business validation is required.
- Service classes should default to `AddTransient<IService, Service>()` registration.

Typical service responsibilities:

- Check whether a record exists before update or delete.
- Check whether a unique code/name/number is already used based on the DbContext configuration.
- Check whether related records prevent a state change.
- Throw validation exceptions when business rules are violated.
- Return null for not found cases.

## Repository Rules

- Every repository must have a matching interface in the Application layer.
- Repository interfaces use the `I` prefix and `Repository` postfix, for example `IClassroomRepository`.
- Repository implementations live in the Infrastructure layer and use the `Repository` postfix.
- Repositories should default to `AddTransient<IRepository, Repository>()` registration unless an existing local pattern requires another lifetime.
- Repositories perform database persistence and database queries only.
- Repositories should not perform business validation or decide whether an operation is allowed.
- Repositories may expose small helper query methods needed by services, such as `ExistsWithCodeAsync`, `HasEnrolledStudents`, or `ExistsActiveAsync`.
- Helper methods should answer database facts, not encode business decisions.

Good repository helper shape:

```csharp
Task<bool> HasEnrolledStudents(Guid id, CancellationToken cancellationToken);
```

Avoid repository methods that perform business validation:

```csharp
// Avoid this - business logic in repository
public async Task DeleteAsync(Guid id, CancellationToken ct)
{
    if (await HasEnrolledStudentsAsync(id, ct))
        throw new InvalidOperationException("Cannot delete with students");
    // delete...
}
```

Prefer keeping the repository delete/update CRUD-shaped and let the service throw validation exceptions when helper query results require it.

## Refactor Checklist

When moving an existing controller/repository workflow into this pattern:

1. Add `I<Entity>Service` in the Application layer.
2. Add `<Entity>Service` in the Application layer.
3. Move validation and state-transition rules from the repository into the service.
4. Add repository helper query methods for database facts the service needs.
5. Simplify repository CRUD methods so they only persist, retrieve, update, delete, or return database errors.
6. Update the controller to inject and call the service interface.
7. Register both service and repository with transient lifetimes by default.
8. Run focused controller/service tests that cover happy paths and moved validation rules.

## Validation Placement Guide

| Rule type | Location |
|-----------|----------|
| HTTP route/body binding | Controller |
| Data annotation model validation | Domain request model / ASP.NET model validation |
| Business rule validation | Application service |
| Existence checks used for business decisions | Repository helper queried by service |
| Relationship checks used for business decisions | Repository helper queried by service |
| EF Core persistence errors | Repository |
| HTTP status mapping | Controller |

## Reference Implementation

See [classroom-workflow-example.cs](examples/classroom-workflow-example.cs) for a complete working example that demonstrates:

- Controller depending on service interface
- Service performing business validation
- Repository exposing helper query methods
- Proper layer separation (Controller → Service → Repository → Database)
- Nullable return types and exception handling
- Dependency injection registration

The example shows:
- `ClassroomController` injects `IClassroomService`
- `IClassroomService` lives in `Application.Education.Interfaces`
- `ClassroomService` lives in `Application.Education.Services`
- `ClassroomService` performs validation (preventing deletion when students are enrolled)
- `IClassroomRepository` exposes helper facts (`HasEnrolledStudents`, `ExistsAsync`)
- `ClassroomRepository` performs CRUD and helper queries only
- Both service and repository are registered as transient dependencies

## Additional Resources

- **[Anti-Patterns](references/anti-patterns.md)** - Common mistakes and how to avoid them

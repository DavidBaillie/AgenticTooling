---
applyTo: '**/Controllers/*.cs, **/*Controller.cs'
---

# API Controller Conventions

This guide defines the conventions for building controllers across all `Company.API.WebApp.*` projects. Always follow these rules when creating or reviewing controller code.

## Naming

- All controllers **must** use the `Controller` postfix (e.g., `StudentsController`, `LivingstonCLVSController`).
- All asynchronous action methods **should** use the `Async` suffix (e.g., `GetAsync`, `CreateAsync`, `UpdateAsync`).

```csharp
public sealed class StudentsController(...) : ControllerBase
```

## Required Class-Level Attributes

Every controller must be decorated with the following attributes:

```csharp
[ApiController]
[ApiExplorerSettings(GroupName = "<group-name>")]
[Route("api/v{version:apiVersion}/[controller]")]
```

- **`[ApiController]`** — enables model binding, automatic 400 responses, and other MVC conventions.
- **`[ApiExplorerSettings(GroupName = "...")]`** — controls which Swagger/OpenAPI document the controller appears in. If you are unsure which group to use, **ask the developer before proceeding**.
- **`[Route(...)]`** — defines the base route template for all actions in the controller. Must include the API version segment `v{version:apiVersion}`.

## Authentication & Authorization

Controllers requiring authentication must use the `[Authorize]` attribute. Apply at class level when all actions require it, or at method level for granular control.

- `[Authorize]` — basic authentication requirement
- `[Authorize(Policy = "PolicyName")]` — policy-based authorization
- `[Authorize(Roles = "Admin,Manager")]` — role-based authorization
- `[AllowAnonymous]` — override class-level authorization for specific actions
- Always validate policies are registered in DI before use

## API Versioning

All controller routes must include the API version segment:

```csharp
[Route("api/v{version:apiVersion}/students")]
```

- The `{version:apiVersion}` placeholder is populated by the API versioning middleware.
- Explicitly specify which API version(s) the controller supports using `[ApiVersion("1.0")]` if needed.
- Maintain backward compatibility or clearly communicate breaking changes across versions.

## OpenAPI Method Decoration

Every action method must be decorated with the appropriate OpenAPI attributes. Use `System.Net.Mime.MediaTypeNames` constants rather than raw strings.

```csharp
[Produces(MediaTypeNames.Application.Json)]
[Consumes(MediaTypeNames.Application.Json)]
[ProducesResponseType<MyResponseDto>(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
```

- `[Consumes(...)]` required on POST/PUT/PATCH; omit on GET/DELETE
- `[ProducesResponseType<T>(...)]` use generic form for typed responses; non-generic for status-only (404, 204)
- Document **all** possible HTTP status codes

## XML Documentation Comments

All action methods should include XML documentation comments to enhance Swagger/OpenAPI documentation and IntelliSense support.

- Use `<summary>` to describe what the action does
- Use `<param>` for each parameter
- Use `<response code="XXX">` to document each HTTP status code
- Ensure XML documentation is enabled: `<GenerateDocumentationFile>true</GenerateDocumentationFile>`

## Common HTTP Status Codes

Use the appropriate status code for each scenario:

| Code | Constant | Usage |
|------|----------|-------|
| 200 | `StatusCodes.Status200OK` | Successful GET, PUT, or PATCH |
| 201 | `StatusCodes.Status201Created` | Successful POST (resource created) |
| 204 | `StatusCodes.Status204NoContent` | Successful DELETE or PUT with no response body |
| 400 | `StatusCodes.Status400BadRequest` | Validation errors, malformed requests |
| 401 | `StatusCodes.Status401Unauthorized` | Authentication required or failed |
| 403 | `StatusCodes.Status403Forbidden` | Authenticated but insufficient permissions |
| 404 | `StatusCodes.Status404NotFound` | Resource not found |
| 409 | `StatusCodes.Status409Conflict` | Business rule violation or duplicate resource |
| 422 | `StatusCodes.Status422UnprocessableEntity` | Semantic validation errors |
| 500 | `StatusCodes.Status500InternalServerError` | Unhandled exceptions (automatic) |

The `[ApiController]` attribute automatically returns 400 for model validation failures.

## Return Types

All action methods must return either `ActionResult` (untyped, for multiple unrelated response types) or `ActionResult<T>` (typed, preferred for single success type).

## Model Binding Sources

Use explicit model binding attributes to specify where parameter values come from:

- **`[FromRoute]`** — route parameters (e.g., `{id}`)
- **`[FromQuery]`** — query string parameters (e.g., `?page=1`)
- **`[FromBody]`** — request body (JSON/XML). Only one per action.
- **`[FromHeader]`** — HTTP headers. Use `Name` property to specify header name.
- **`[FromForm]`** — form data (multipart/form-data or application/x-www-form-urlencoded)
- **`[FromServices]`** — DI container injection in action method. Use sparingly; prefer constructor injection.

The `CancellationToken` parameter does not require a binding attribute; ASP.NET Core automatically provides it from `HttpContext.RequestAborted`.

## CancellationToken Parameter

All action methods that perform asynchronous operations **must** accept a `CancellationToken` parameter and pass it to service calls:

```csharp
public async Task<ActionResult<StudentDto>> GetAsync(
    [FromRoute] Guid id,
    CancellationToken cancellationToken)
{
    return Ok(await studentService.GetStudentAsync(id, cancellationToken));
}
```

ASP.NET Core automatically provides the token from `HttpContext.RequestAborted`. Always pass it through to service methods and database operations.

## GET by Id — Named Route Constant

When a controller exposes a GET-by-Id endpoint, it **must** be assigned a named route stored in a `private const string`:

```csharp
private const string GET_STUDENT = "GetStudent";

[HttpGet("{id:guid}", Name = GET_STUDENT)]
public async Task<ActionResult<StudentDto>> GetAsync(...)
```

This constant is reused in POST/PUT actions that return `CreatedAtRoute`.

## Route Constraints

Use route constraints to validate route parameter formats and improve routing precision. Common constraints: `:guid`, `:int`, `:long`, `:decimal`, `:alpha`, `:min(n)`, `:max(n)`, `:length(n)`, `:minlength(n)`, `:maxlength(n)`.

Always use `:guid` for GUID-based ID parameters to prevent invalid formats from reaching your action methods.

## PUT Methods — Route Id Authority

PUT methods must always take the resource `id` from the route and **never** trust an `id` provided inside the request body or update model. If the update model carries an `Id` property, ignore it and use the route value exclusively. If a developer received a model from a PUT endpoint's body and it contains a field called `Id`, ask the developer if the field can be removed.

```csharp
[HttpPut("{id:guid}")]
public async Task<ActionResult> UpdateAsync([FromRoute] Guid id, [FromBody] UpdateMyResourceRequest request, ...)
{
    // Always use `id` from the route — never request.Id
}
```

## PATCH Methods — Partial Updates

PATCH methods allow partial updates. Like PUT, the resource `id` must come from the route. Use `JsonPatchDocument<T>` for RFC 6902 JSON Patch support. Consider whether your API needs PATCH or if PUT is sufficient.

## DELETE Methods — No Content Response

DELETE methods should return `204 No Content` upon successful deletion. Like PUT methods, DELETE must take the resource `id` from the route.

```csharp
[HttpDelete("{id:guid}")]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public async Task<ActionResult> DeleteAsync(
    [FromRoute] Guid id,
    CancellationToken cancellationToken)
{
    await myService.DeleteAsync(id, cancellationToken);
    return NoContent();
}
```

- Return `NoContent()` (204) after successful deletion — do not return the deleted resource unless specifically required by the API contract.

## No Business Logic in Controllers

Controllers must **only** translate HTTP input into service calls and map the result to the correct HTTP response. Do not place validation logic, domain calculations, or data transformations inside a controller action.

> **Exception:** When a trivial guard (e.g., a quick date-range or pagination check) would otherwise require a dedicated application service method, it may live in the controller. **Ask the developer before breaking this rule.**

## Input Model Validation

When an action accepts a model from the request body (`[FromBody]`) or query string (`[FromQuery]`), inspect every property of that model for validation attributes (e.g., `[Required]`, `[Range]`, `[StringLength]`).

If any property lacks a validation attribute, **ask the developer** whether this was intentional and whether a validation attribute should be added before proceeding.

The `[Required]` attribute should never be used on non-nullable value types (e.g., `int`, `double`, `bool`, `DateTime`). It checks for null, and non-nullable value types can never be null, so the validation will never trigger. Use `[Required]` only on nullable types: reference types (e.g., `string`, custom classes) and nullable value types (e.g., `int?`, `DateTime?`). If a developer includes the attribute on a non-nullable value type, inform them of this issue.

All `enum` properties should have the `[EnumIsDefined]` attribute. If a model is being validated and there is a property that is an `enum` and it does not have this attribute, add it and inform the developer.

## Dependency Injection

Controllers must use primary constructor injection to receive dependencies:

```csharp
public sealed class StudentsController(IStudentService studentService) : ControllerBase
{
    // Use studentService directly in action methods
}
```

- Dependencies are automatically available as readonly fields.
- Do not use property injection or service locator patterns.
- Keep constructor parameters to a reasonable number (typically 1-3 services).

## Full Example

```csharp
using System.Net.Mime;
using Company.API.Application.Students.Interfaces.Student;
using Company.API.Domain.Students.Models.Students;
using Company.API.WebApp.Common.Controllers;
using Microsoft.AspNetCore.Mvc;

namespace Company.API.WebApp.Students.Controllers.Students;

[ApiController]
[ApiExplorerSettings(GroupName = "students")]
[Route("api/v{version:apiVersion}/students")]
public sealed class StudentsController(IStudentService studentService) : ControllerBase
{
    private const string GET_STUDENT_BY_ID = "GetStudentById";

    [HttpGet("{id:guid}", Name = GET_STUDENT_BY_ID)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<StudentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        return Ok(await studentService.GetStudentAsync(id, cancellationToken));
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<StudentDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult> CreateAsync(
        [FromBody] CreateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var student = await studentService.CreateStudentAsync(request, cancellationToken);
        return CreatedAtRoute(GET_STUDENT_BY_ID, new { id = student.Id }, student);
    }

    [HttpPut("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<StudentDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> UpdateAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateStudentRequest request,
        CancellationToken cancellationToken)
    {
        var student = await studentService.UpdateStudentAsync(id, request, cancellationToken);
        return Ok(student);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await studentService.DeleteStudentAsync(id, cancellationToken);
        return NoContent();
    }
}
```

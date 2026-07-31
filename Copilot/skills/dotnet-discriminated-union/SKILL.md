---
name: dotnet-discriminated-union
description: 'OneOf discriminated union pattern for C# return types. Use when implementing methods with multiple distinct outcomes (Success, NotFound, Error), handling OneOf result types, creating service methods that return typed errors instead of exceptions, working with result types in controllers or application services, or reviewing code that uses the OneOf library. Provides patterns for defining, returning, and consuming OneOf<T0, T1, ...Tn> types with compile-time exhaustiveness checking.'
---

# Discriminated Unions with OneOf

This repository uses the [OneOf](https://github.com/mcintyre321/OneOf) library (v3, NuGet: `OneOf`) to express methods that may produce multiple distinct outcome types. It is a compile-time-checked alternative to exceptions for control flow, polymorphism via a common base type, or returning `object`.

The `OneOf` package is declared globally in `Directory.Build.props` — **do not add it to individual `.csproj` files**.

---

## When to Use OneOf

Use `OneOf<T0, T1, ...Tn>` as a method's return type whenever the method has two or more meaningfully different outcomes that the caller must handle. Common scenarios:

- A resource may or may not exist (`NotFound`)
- An operation may succeed or fail with a typed error (`Error<T>`)
- A value may be absent (`None`)
- A caller needs to take different code paths depending on the result

---

## Package Namespaces

```csharp
using OneOf;         // OneOf<T0, T1, ...>, OneOfBase<T0, T1>
using OneOf.Types;   // Success, NotFound, Error<T>, None, Yes, No, True, False
```

---

## Built-in Types from `OneOf.Types`

| Type | Use when |
|------|----------|
| `Success` | Operation completed with no meaningful value to return |
| `NotFound` | A requested resource does not exist |
| `Error<T>` | An error occurred; `T` carries the error detail (e.g. `Error`, `Error<Exception>`) |
| `None` | A value is explicitly absent (option type) |
| `Yes` / `No` | Boolean-flavoured outcomes where context is needed |

---

## Defining a Return Type

```csharp
// Application service interface — Application layer
Task<OneOf<StudentDto, NotFound>> GetAsync(Guid id, CancellationToken cancellationToken);

Task<OneOf<StudentDto, NotFound, Error<Exception>>> UpdateAsync(Guid id, StudentDto dto, CancellationToken cancellationToken);

Task<OneOf<Success, Error>> ValidateAsync(StudentDto student, CancellationToken cancellationToken);
```

Document each case in the XML `<returns>` summary using bullet points:

```csharp
/// <returns>
/// - <typeparamref name="Dto" /> when updated
/// - <see cref="NotFound" /> if no entity with the given Id exists
/// - <see cref="Error{T}"/> of <see cref="Exception" /> if the update failed
/// </returns>
```

---

## Returning Values

Implicit conversions allow returning any union case type directly — no wrapping needed:

```csharp
public async Task<OneOf<decimal, None, Error>> ApplyDamageAsync(...)
{
    if (health == damage)
        return value;           // implicit: decimal → OneOf<decimal, None, Error>

    if (isImmune)
        return new None();      // implicit: None → OneOf<decimal, None, Error>

    return new Error();
}
```

---

## Consuming Results — Preferred Patterns

### `TryPickT𝑥` — preferred for branching control flow

Use `TryPickT𝑥` when you want to handle one specific case and continue with the happy path. The out parameter named `_` discards cases you don't need. Never use the `var` keyword when handling a specific outcome from a return.

```csharp
// Returns true if the value IS T1 (the picked case). The remainder is the other cases.
if (result.TryPickT1(out NotFound _, out StudentDto student))
    return NotFound();

// Continue with `student` on the happy path
return Ok(student);
```

```csharp
// Pick the error case; continue with the success value
if (result.TryPickT1(out Error<Exception> error, out StudentDto student))
{
    logger.LogError(error.Value, "Failed to retrieve student.");
    return InternalServerError();
}
```

For three-case unions, chain `TryPickT𝑥` calls:

```csharp
if (result.TryPickT1(out NotFound _, out OneOf<StudentDto, Error<Exception>> dtoOrError))
    return NotFound();

if (dtoOrError.TryPickT1(out Error<Exception> error, out StudentDto dto))
{
    logger.LogError(error.Value, "Unexpected error.");
    return InternalServerError();
}

return Ok(dto);
```

### `.Match(...)` — preferred when mapping all cases to a single return type

Use `.Match` in controllers or services when every case maps to a result and exhaustive handling is required at compile time.

```csharp
return result.Match<ActionResult>(
    validResult  => Ok(new ValidateCityPostalCodeDto { IsValid = validResult.IsValid }),
    ewsFailure   => InternalServerError()
);
```

### `.Switch(...)` — for side effects with no return value

```csharp
result.Switch(
    success => logger.LogInformation("Operation succeeded."),
    error   => logger.LogError("Operation failed: {Error}", error.Value)
);
```

### `.MapT𝑥(...)` — transform one case while preserving others

```csharp
// Transform T0 (the rate) into a converted decimal; NotFound and Error pass through unchanged
return (await exchangeRateRepository.GetAsync(fromCode, toCode, cancellationToken))
    .MapT0(rate => value * rate);
```

---

## Custom Union Case Types

When the built-in `OneOf.Types` do not convey sufficient meaning, define a custom struct in the **Application layer** under an `OneOfTypes/` folder.

**Location:** `/Company.API.Application.{Domain}/OneOfTypes/MyCustomCase.cs`

```csharp
namespace Company.API.Application.Shipping.OneOfTypes;

/// <summary>
/// Represents an action that was rejected because it is not permitted given the current state of the resource.
/// Should result in a <c>400 Bad Request</c>.
/// </summary>
public struct InvalidAction(string reason)
{
    public string Reason { get; set; } = reason;
}
```

Use it in signatures just like any built-in type:

```csharp
Task<OneOf<EnrollmentDto, NotFound, InvalidAction>> AssignStudentToClassroomAsync(Guid studentId, Guid classroomId, CancellationToken cancellationToken);
```

Rules for custom case types:
- Use `struct`, not `class`, unless reference semantics are required.
- Give the type a name that describes the **outcome**, not the error text.
- Always include an XML summary documenting what it means and the expected HTTP status code if it surfaces in a controller.
- Do **not** inherit from `Exception` — OneOf is the alternative to exception-based control flow.

---

## Anti-Patterns to Avoid

| Anti-pattern | Preferred alternative |
|---|---|
| `throw` for expected, recoverable conditions | Return `Error<T>` or a custom case type |
| `null` to signal absence | Return `OneOf<T, None>` or `OneOf<T, NotFound>` |
| `bool` return with `out` parameter for the result | Return `OneOf<T, ErrorCase>` |
| Catching a OneOf result and ignoring cases | Always handle all cases via `Match`, `Switch`, or chained `TryPickT𝑥` |
| Adding OneOf to Domain models or domain entities | Keep OneOf in Application and Infrastructure layers only |

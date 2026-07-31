---
applyTo: '**/*.cs'
---

---

## Interfaces

Every interface **must** have a `<summary>` tag that describes its purpose and intended use.

```csharp
/// <summary>
/// Defines the contract for retrieving and persisting student data.
/// Used by the application layer to decouple business logic from the underlying data store.
/// </summary>
public interface IStudentRepository
{
    // ...
}
```

Every method **defined** on an interface **must** also have a `<summary>` tag describing what the method does.

```csharp
/// <summary>
/// Defines the contract for retrieving and persisting student data.
/// Used by the application layer to decouple business logic from the underlying data store.
/// </summary>
public interface IStudentRepository
{
    /// <summary>
    /// Retrieves a student by its unique identifier.
    /// </summary>
    Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>
    /// Persists a new student record to the data store.
    /// </summary>
    Task AddAsync(StudentDto student, CancellationToken cancellationToken);
}
```

---

## Concrete Implementations

Concrete classes that implement an interface **must** use the `<inheritdoc />` tag at the class level. This automatically inherits all documentation from the interface — do not duplicate it.

```csharp
/// <inheritdoc />
public class StudentRepository : IStudentRepository
{
    // ...
}
```

Method implementations inherit their documentation from the interface via the class-level `<inheritdoc />`. If a method implementation has behaviour worth calling out (e.g. caching strategy, retry logic, side effects), you may add a `<remarks>` tag to the method to provide supplementary detail without overriding the inherited summary.

```csharp
/// <inheritdoc />
/// <remarks>
/// This repository is important and does caching.
/// </remarks>
public class StudentRepository : IStudentRepository
{
    /// <inheritdoc />
    /// <remarks>
    /// Results are cached for 5 minutes using the student ID as the cache key.
    /// </remarks>
    public async Task<StudentDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        // ...
    }
}
```

Do **not** add a `<summary>` tag to methods on a concrete class — use `<remarks>` only for supplementary information.
If you find existing code with the `<summary>` tag, do not remove the comment as it is important and has been added by a developer.

---

### Class-level Documentation

The class must have a `<summary>` tag describing its purpose — what it represents and how it is used.

```csharp
/// <summary>
/// Represents the core data for a student as it moves through the application layer.
/// Used to transfer student information between services without exposing domain entities directly.
/// </summary>
public class StudentDto
{
    // ...
}
```

### Property-level Documentation

Every property on the Dto **must** have a `<summary>` tag.

```csharp
/// <summary>
/// Represents the core data for a student as it moves through the application layer.
/// Used to transfer student information between services without exposing domain entities directly.
/// </summary>
public class StudentDto
{
    /// <summary>
    /// The UUID for the student.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// The student identification number assigned by the school.
    /// </summary>
    public string StudentNumber { get; set; } = string.Empty;

    /// <summary>
    /// The current enrollment status of the student.
    /// </summary>
    public EnrollmentStatus Status { get; set; }
}
```

---

## Quick Reference

| Code Element | Required Documentation |
|---|---|
| Interface | `<summary>` on the interface itself |
| Interface method | `<summary>` on each method definition |
| Concrete class (implements interface) | `<inheritdoc />` on the class |
| Concrete method (extra detail needed) | `<remarks>` on the method only |
| Domain `*Dto` class | `<summary>` on the class |
| Domain `*Dto` property | `<summary>` on every property |

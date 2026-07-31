# Anti-Patterns to Avoid

This reference shows common mistakes when implementing the Controller-Service-Repository pattern.

## ❌ Anti-Pattern 1: Controller Depends on Repository

**Wrong:**
```csharp
// Controller directly injecting repository - bypasses service layer
public sealed class ClassroomController(IClassroomRepository repository) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        await repository.DeleteAsync(id, ct);
        return NoContent();
    }
}
```

**Why it's wrong:**
- Business validation (students enrolled?) is skipped
- Controller performs database operations directly
- Violates layer separation

**Correct:**
```csharp
// Controller depends on service interface
public sealed class ClassroomController(IClassroomService service) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken ct)
    {
        try
        {
            await service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

---

## ❌ Anti-Pattern 2: Repository Performs Business Validation

**Wrong:**
```csharp
// Repository making business decisions
public sealed class ClassroomRepository(AppDbContext context) : IClassroomRepository
{
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        // Business logic in repository - WRONG layer!
        var hasStudents = await context.Enrollments
            .AnyAsync(e => e.ClassroomId == id && e.IsActive, ct);
            
        if (hasStudents)
        {
            throw new ValidationException("Cannot delete classroom with enrolled students.");
        }
        
        await context.Classrooms.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
    }
}
```

**Why it's wrong:**
- Business rules belong in the service layer
- Repository is coupled to business logic
- Difficult to test and reuse

**Correct:**
```csharp
// Repository provides database facts, service makes business decisions
public sealed class ClassroomRepository(AppDbContext context) : IClassroomRepository
{
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        await context.Classrooms.Where(c => c.Id == id).ExecuteDeleteAsync(ct);
    }
    
    // Helper method for service to query database facts
    public async Task<bool> HasEnrolledStudentsAsync(Guid classroomId, CancellationToken ct)
    {
        return await context.Enrollments
            .AnyAsync(e => e.ClassroomId == classroomId && e.IsActive, ct);
    }
}

// Service uses helper to make business decision
public sealed class ClassroomService(IClassroomRepository repository) : IClassroomService
{
    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var hasStudents = await repository.HasEnrolledStudentsAsync(id, ct);
        if (hasStudents)
        {
            throw new ValidationException("Cannot delete classroom with enrolled students.");
        }
        
        await repository.DeleteAsync(id, ct);
    }
}
```

---

## ❌ Anti-Pattern 3: Missing Service Layer

**Wrong:**
```csharp
// Controller performs validation and business logic
public sealed class ClassroomController(IClassroomRepository repository) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateAsync(CreateClassroomRequest request, CancellationToken ct)
    {
        // Business validation in controller - WRONG layer!
        var codeExists = await repository.ExistsWithCodeAsync(request.Code, ct);
        if (codeExists)
        {
            return BadRequest($"Classroom code '{request.Code}' is already in use.");
        }
        
        var classroom = new Classroom { /* ... */ };
        await repository.CreateAsync(classroom, ct);
        return CreatedAtRoute(GET_CLASSROOM, new { id = classroom.Id }, classroom);
    }
}
```

**Why it's wrong:**
- Business logic is in the controller (HTTP layer)
- Difficult to reuse validation in other contexts
- Tight coupling between HTTP and business logic

**Correct:**
```csharp
// Service layer handles business validation
public sealed class ClassroomService(IClassroomRepository repository) : IClassroomService
{
    public async Task<ClassroomDto> CreateAsync(
        CreateClassroomRequest request, 
        CancellationToken ct)
    {
        var codeExists = await repository.ExistsWithCodeAsync(request.Code, ct);
        if (codeExists)
        {
            throw new ValidationException($"Classroom code '{request.Code}' is already in use.");
        }
        
        var classroom = new Classroom { /* ... */ };
        await repository.CreateAsync(classroom, ct);
        return mapper.ToDto(classroom);
    }
}

// Controller handles exceptions and maps to HTTP responses
public sealed class ClassroomController(IClassroomService service) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> CreateAsync(CreateClassroomRequest request, CancellationToken ct)
    {
        try
        {
            var dto = await service.CreateAsync(request, ct);
            return CreatedAtRoute(GET_CLASSROOM, new { id = dto.Id }, dto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

---

## ❌ Anti-Pattern 4: Repository Returns Business Outcomes

**Wrong:**
```csharp
public interface IClassroomRepository
{
    // Repository performing business validation - too high level!
    Task UpdateAsync(Classroom classroom, CancellationToken ct); // Throws validation exceptions
}
```

**Why it's wrong:**
- Repository is deciding business outcomes
- Mixes infrastructure concerns with application concerns
- Service layer becomes a pass-through

**Correct:**
```csharp
// Repository focuses on database operations
public interface IClassroomRepository
{
    Task UpdateAsync(Classroom classroom, CancellationToken ct);
    Task<bool> ExistsAsync(Guid id, CancellationToken ct);
}

// Service interprets repository results and throws exceptions for business failures
public sealed class ClassroomService(IClassroomRepository repository) : IClassroomService
{
    public async Task UpdateAsync(Guid id, UpdateRequest request, CancellationToken ct)
    {
        var exists = await repository.ExistsAsync(id, ct);
        if (!exists)
        {
            throw new NotFoundException($"Classroom with ID {id} not found.");
        }
        
        var classroom = new Classroom { /* ... */ };
        await repository.UpdateAsync(classroom, ct);
    }
}
```

---

## ❌ Anti-Pattern 5: Service Without Interface

**Wrong:**
```csharp
// Concrete service class, no abstraction
public sealed class ClassroomService { /* ... */ }

// Controller depends on concrete implementation
public sealed class ClassroomController(ClassroomService service) : ControllerBase
{
    // ...
}
```

**Why it's wrong:**
- Tight coupling to implementation
- Difficult to test (can't mock)
- Violates dependency inversion principle

**Correct:**
```csharp
// Service interface in Application layer
public interface IClassroomService { /* ... */ }

// Service implementation in Application layer
public sealed class ClassroomService : IClassroomService { /* ... */ }

// Controller depends on abstraction
public sealed class ClassroomController(IClassroomService service) : ControllerBase
{
    // ...
}

// DI registration
services.AddTransient<IClassroomService, ClassroomService>();
```

---

## Key Principles Summary

✅ **DO:**
- Controllers depend on service interfaces
- Services perform all business validation and throw exceptions for failures
- Repositories provide database facts via helper methods
- Services interpret database facts and throw appropriate exceptions
- Use interfaces for both services and repositories
- Return null for not found cases from read operations

❌ **DON'T:**
- Have controllers depend on repositories directly
- Put business logic or validation exceptions in repositories
- Have repositories throw business validation exceptions
- Skip the service layer
- Depend on concrete implementations

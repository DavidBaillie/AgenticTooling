using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;

namespace Company.API.WebServices.Education.Controllers;

/// <summary>
/// Example controller demonstrating the Controller-Service-Repository workflow.
/// This shows:
/// - Controller depends on service interface, not repository
/// - Controller handles HTTP concerns only (routing, status codes)
/// - Service performs business validation and throws exceptions
/// - Repository handles database operations
/// - Clean separation of concerns across layers
/// </summary>
[ApiController]
[ApiExplorerSettings(GroupName = "education")]
[Route("api/classrooms")]
public sealed class ClassroomController(IClassroomService classroomService) : ControllerBase
{
    private const string GET_CLASSROOM = "GetClassroom";

    [HttpGet("{id:guid}", Name = GET_CLASSROOM)]
    [Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ClassroomDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClassroomDto>> GetAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var classroom = await classroomService.GetAsync(id, cancellationToken);
        return classroom is not null ? Ok(classroom) : NotFound();
    }

    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json), Produces(MediaTypeNames.Application.Json)]
    [ProducesResponseType<ClassroomDto>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> CreateAsync(
        [FromBody] CreateClassroomRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var dto = await classroomService.CreateAsync(request, cancellationToken);
            return CreatedAtRoute(GET_CLASSROOM, new { id = dto.Id }, dto);
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> DeleteAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        try
        {
            // Notice: Service handles business validation (students enrolled check)
            await classroomService.DeleteAsync(id, cancellationToken);
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

// ============================================================================
// APPLICATION LAYER - Service Interface
// ============================================================================

namespace Company.API.Application.Education.Interfaces;

/// <summary>
/// Service interface defining classroom business operations.
/// Lives in Application layer.
/// Returns null for not found cases, throws exceptions for validation failures.
/// </summary>
public interface IClassroomService
{
    /// <summary>Returns classroom or null if not found.</summary>
    Task<ClassroomDto?> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <summary>Creates classroom. Throws ValidationException if code already exists.</summary>
    Task<ClassroomDto> CreateAsync(CreateClassroomRequest request, CancellationToken cancellationToken);

    /// <summary>Deletes classroom. Throws NotFoundException if not found, ValidationException if has enrolled students.</summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}

// ============================================================================
// APPLICATION LAYER - Service Implementation
// ============================================================================

namespace Company.API.Application.Education.Services;

/// <summary>
/// Service implementation performing business validation and orchestration.
/// Lives in Application layer.
/// Depends on repository interface (abstraction).
/// </summary>
public sealed class ClassroomService(
    IClassroomRepository classroomRepository,
    IClassroomMapper mapper) : IClassroomService
{
    public async Task<ClassroomDto?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        // Simple read - forward to repository
        var classroom = await classroomRepository.GetByIdAsync(id, cancellationToken);
        return classroom is not null ? mapper.ToDto(classroom) : null;
    }

    public async Task<ClassroomDto> CreateAsync(
        CreateClassroomRequest request,
        CancellationToken cancellationToken)
    {
        // Business validation: Check if code is already used
        var codeExists = await classroomRepository.ExistsWithCodeAsync(request.Code, cancellationToken);
        if (codeExists)
        {
            throw new ValidationException($"Classroom code '{request.Code}' is already in use.");
        }

        // Validation passed - create entity and persist
        var classroom = new Classroom
        {
            Id = Guid.NewGuid(),
            Code = request.Code,
            Name = request.Name,
            Capacity = request.Capacity,
            IsActive = true
        };

        await classroomRepository.CreateAsync(classroom, cancellationToken);
        return mapper.ToDto(classroom);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        // Business validation: Check if classroom exists
        var exists = await classroomRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
        {
            throw new NotFoundException($"Classroom with ID {id} not found.");
        }

        // Business validation: Check if students are enrolled
        var hasStudents = await classroomRepository.HasEnrolledStudentsAsync(id, cancellationToken);
        if (hasStudents)
        {
            throw new ValidationException("Cannot delete classroom with enrolled students.");
        }

        // Validation passed - perform delete
        await classroomRepository.DeleteAsync(id, cancellationToken);
    }
}

// ============================================================================
// APPLICATION LAYER - Repository Interface
// ============================================================================

namespace Company.ICP.API.Application.Education.Interfaces;

/// <summary>
/// Repository interface defining classroom persistence operations.
/// Lives in Application layer (abstraction).
/// Implemented inrastructure layer.
/// </summary>
public interface IClassroomRepository
{
    Task<Classroom?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task CreateAsync(Classroom classroom, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);

    // Helper query methods for service validation
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsWithCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> HasEnrolledStudentsAsync(Guid classroomId, CancellationToken cancellationToken);
}

// ============================================================================
// INFRASTRUCTURE LAYER - Repository Implementation
// ============================================================================

namespace Company.API.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation performing database operations only.
/// Lives in Infrastructure layer.
/// No business validation - that's the service's job.
/// </summary>
public sealed class ClassroomRepository(AppDbContext context) : IClassroomRepository
{
    public async Task<Classroom?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Classrooms
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task CreateAsync(Classroom classroom, CancellationToken cancellationToken)
    {
        context.Classrooms.Add(classroom);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        await context.Classrooms
            .Where(c => c.Id == id)
            .ExecuteDeleteAsync(cancellationToken);
    }

    // Helper methods answer database facts for service decisions
    public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await context.Classrooms
            .AsNoTracking()
            .AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsWithCodeAsync(string code, CancellationToken cancellationToken)
    {
        return await context.Classrooms
            .AsNoTracking()
            .AnyAsync(c => c.Code == code, cancellationToken);
    }

    public async Task<bool> HasEnrolledStudentsAsync(Guid classroomId, CancellationToken cancellationToken)
    {
        return await context.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.ClassroomId == classroomId && e.IsActive, cancellationToken);
    }
}

// ============================================================================
// CUSTOM EXCEPTIONS
// ============================================================================

namespace Company.API.Application.Common.Exceptions;

public class ValidationException : Exception
{
    public ValidationException(string message) : base(message) { }
}

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

// ============================================================================
// DEPENDENCY INJECTION REGISTRATION
// ============================================================================

namespace Company.API.WebServices.Extensions;

public static class ServiceRegistration
{
    public static IServiceCollection AddEducationServices(this IServiceCollection services)
    {
        // Register service and repository with transient lifetime
        services.AddTransient<IClassroomService, ClassroomService>();
        services.AddTransient<IClassroomRepository, ClassroomRepository>();

        return services;
    }
}

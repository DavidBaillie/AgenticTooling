using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;

namespace Company.API.WebApi.Controllers;

/// <summary>
/// Example controller demonstrating OneOf discriminated union patterns for handling
/// multiple distinct outcome types with compile-time exhaustiveness checking.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StudentController(IStudentService studentService, ILogger<StudentController> logger)
    : ControllerBase
{
    /// <summary>
    /// Gets a student by ID.
    /// Demonstrates the Match pattern for exhaustive mapping to ActionResult.
    /// </summary>
    /// <param name="id">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// - 200 OK with student data
    /// - 404 Not Found if the student does not exist
    /// </returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<StudentDto>> GetAsync(Guid id, CancellationToken cancellationToken) =>
        (await studentService.GetAsync(id, cancellationToken)).Match<ActionResult>(
            student => Ok(student),
            _ => NotFound());

    /// <summary>
    /// Updates a student.
    /// Demonstrates Match pattern for three-case unions.
    /// </summary>
    /// <param name="id">The student identifier.</param>
    /// <param name="dto">The updated student data.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// - 200 OK with updated student data
    /// - 404 Not Found if the student does not exist
    /// - 500 Internal Server Error if the update failed
    /// </returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(StudentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<StudentDto>> UpdateAsync(
        Guid id,
        [FromBody] StudentDto dto,
        CancellationToken cancellationToken)
    {
        return (await studentService.UpdateAsync(id, dto, cancellationToken)).Match<ActionResult>(
            student => Ok(student),
            _ => NotFound(),
            error =>
            {
                logger.LogError(error.Value, "Failed to update student {StudentId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            });
    }

    /// <summary>
    /// Validates a student's data.
    /// Demonstrates the Match pattern for exhaustive handling when mapping to a single return type.
    /// </summary>
    /// <param name="dto">The student data to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// - 200 OK with validation result
    /// - 500 Internal Server Error if validation service failed
    /// </returns>
    [HttpPost("validate")]
    [ProducesResponseType(typeof(ValidationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ValidationResultDto>> ValidateAsync(
        [FromBody] StudentDto dto,
        CancellationToken cancellationToken)
    {
        // Match maps all cases to a single ActionResult type
        return (await studentService.ValidateAsync(dto, cancellationToken)).Match<ActionResult>(
            success => Ok(new ValidationResultDto { IsValid = true }),
            error => StatusCode(StatusCodes.Status500InternalServerError)
        );
    }

    /// <summary>
    /// Enrolls a student in a classroom.
    /// Demonstrates handling custom OneOf case types (InvalidAction) with Match.
    /// </summary>
    /// <param name="studentId">The student identifier.</param>
    /// <param name="classroomId">The classroom identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// - 200 OK with enrollment data
    /// - 404 Not Found if student or classroom does not exist
    /// - 400 Bad Request if the enrollment action is not permitted
    /// </returns>
    [HttpPost("{studentId:guid}/enroll/{classroomId:guid}")]
    [ProducesResponseType(typeof(EnrollmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EnrollmentDto>> EnrollAsync(
        Guid studentId,
        Guid classroomId,
        CancellationToken cancellationToken)
    {
        return (await studentService.AssignStudentToClassroomAsync(studentId, classroomId, cancellationToken))
            .Match<ActionResult<EnrollmentDto>>(
                enrollment => Ok(enrollment),
                _ => NotFound(),
                invalidAction => BadRequest(new ProblemDetails
                {
                    Title = "Invalid enrollment action",
                    Detail = invalidAction.Reason,
                    Status = StatusCodes.Status400BadRequest
                }));
    }

    /// <summary>
    /// Lists all students with optional filtering.
    /// Demonstrates Match with side effects using Switch.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// - 200 OK with list of students
    /// - 500 Internal Server Error if retrieval failed
    /// </returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<StudentDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<IEnumerable<StudentDto>>> ListAsync(CancellationToken cancellationToken)
    {
        // Match for the response
        return (await studentService.ListAsync(cancellationToken)).Match<ActionResult>(
            students => Ok(students),
            _ => StatusCode(StatusCodes.Status500InternalServerError)
        );
    }

    /// <summary>
    /// Deletes a student by ID.
    /// Demonstrates Success type for operations with no meaningful return value using Match.
    /// </summary>
    /// <param name="id">The student identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// - 204 No Content if deleted successfully
    /// - 404 Not Found if the student does not exist
    /// - 500 Internal Server Error if deletion failed
    /// </returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        return (await studentService.DeleteAsync(id, cancellationToken)).Match<ActionResult>(
            _ => NoContent(),
            _ => NotFound(),
            error =>
            {
                logger.LogError(error.Value, "Failed to delete student {StudentId}.", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            });
    }
}

// Application service interface demonstrating OneOf return signatures
public interface IStudentService
{
    /// <returns>
    /// - <see cref="StudentDto"/> if found
    /// - <see cref="NotFound"/> if no student with the given Id exists
    /// </returns>
    Task<OneOf<StudentDto, NotFound>> GetAsync(Guid id, CancellationToken cancellationToken);

    /// <returns>
    /// - <see cref="StudentDto"/> when updated
    /// - <see cref="NotFound"/> if no entity with the given Id exists
    /// - <see cref="Error{T}"/> of <see cref="Exception"/> if the update failed
    /// </returns>
    Task<OneOf<StudentDto, NotFound, Error<Exception>>> UpdateAsync(
        Guid id,
        StudentDto dto,
        CancellationToken cancellationToken);

    /// <returns>
    /// - <see cref="Success"/> when validation completes
    /// - <see cref="Error"/> if validation service failed
    /// </returns>
    Task<OneOf<Success, Error>> ValidateAsync(StudentDto student, CancellationToken cancellationToken);

    /// <returns>
    /// - <see cref="EnrollmentDto"/> when enrolled
    /// - <see cref="NotFound"/> if student or classroom does not exist
    /// - <see cref="InvalidAction"/> if enrollment is not permitted
    /// </returns>
    Task<OneOf<EnrollmentDto, NotFound, InvalidAction>> AssignStudentToClassroomAsync(
        Guid studentId,
        Guid classroomId,
        CancellationToken cancellationToken);

    /// <returns>
    /// - <see cref="IEnumerable{StudentDto}"/> with all students
    /// - <see cref="Error{T}"/> of <see cref="Exception"/> if retrieval failed
    /// </returns>
    Task<OneOf<IEnumerable<StudentDto>, Error<Exception>>> ListAsync(CancellationToken cancellationToken);

    /// <returns>
    /// - <see cref="Success"/> when deleted
    /// - <see cref="NotFound"/> if no student with the given Id exists
    /// - <see cref="Error{T}"/> of <see cref="Exception"/> if deletion failed
    /// </returns>
    Task<OneOf<Success, NotFound, Error<Exception>>> DeleteAsync(Guid id, CancellationToken cancellationToken);
}

// DTOs
public record StudentDto(Guid Id, string FirstName, string LastName, string Email);
public record EnrollmentDto(Guid EnrollmentId, Guid StudentId, Guid ClassroomId, DateTime EnrolledAt);
public record ValidationResultDto(bool IsValid);

// Custom OneOf case type (should be in Application layer under OneOfTypes/ folder)
/// <summary>
/// Represents an action that was rejected because it is not permitted given the current state of the resource.
/// Should result in a <c>400 Bad Request</c>.
/// </summary>
public struct InvalidAction
{
    public string Reason { get; set; }

    public InvalidAction(string reason)
    {
        Reason = reason;
    }
}

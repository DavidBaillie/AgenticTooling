using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Education.Data.Repositories;

/// <summary>
/// Repository demonstrating EF Core best practices:
/// AsNoTracking, eager loading, split queries, pagination, compiled queries, projections
/// </summary>
public interface IStudentRepository
{
    Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Student?> GetWithEnrollmentsAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<StudentDto>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken ct = default);
    Task<List<StudentSummaryDto>> GetActiveStudentsAsync(CancellationToken ct = default);
    Task<Guid> CreateAsync(Student student, CancellationToken ct = default);
    Task UpdateAsync(Student student, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
    Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default);
    Task<int> GetEnrollmentCountAsync(Guid studentId, CancellationToken ct = default);
}

public class StudentRepository(StudentDbContext context) : IStudentRepository
{
    /// <summary>
    /// Simple read-only query using AsNoTracking for better performance.
    /// </summary>
    public async Task<Student?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Students
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <summary>
    /// Eager loading with Include/ThenInclude. AsSplitQuery avoids cartesian explosion.
    /// </summary>
    public async Task<Student?> GetWithEnrollmentsAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Students
            .AsNoTracking()
            .Include(s => s.Enrollments)
                .ThenInclude(e => e.Course)
            .AsSplitQuery()
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    /// <summary>
    /// Pagination with Skip/Take to prevent loading large result sets.
    /// </summary>
    public async Task<PagedResult<StudentDto>> GetPagedAsync(
        int pageNumber,
        int pageSize,
        CancellationToken ct = default)
    {
        var query = context.Students
            .AsNoTracking()
            .OrderBy(s => s.LastName)
            .ThenBy(s => s.FirstName);

        var totalCount = await query.CountAsync(ct);
        var students = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(s => new StudentDto
            {
                Id = s.Id,
                FullName = s.FirstName + " " + s.LastName,
                Email = s.Email,
                EnrollmentDate = s.EnrollmentDate,
                EnrollmentCount = s.Enrollments.Count
            })
            .ToListAsync(ct);

        return new PagedResult<StudentDto>
        {
            Items = students,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Projection retrieves only required fields for better efficiency.
    /// </summary>
    public async Task<List<StudentSummaryDto>> GetActiveStudentsAsync(CancellationToken ct = default)
    {
        return await context.Students
            .AsNoTracking()
            .Where(s => s.Enrollments.Any(e => e.Course.IsActive))
            .Select(s => new StudentSummaryDto
            {
                Id = s.Id,
                FullName = s.FirstName + " " + s.LastName,
                ActiveEnrollmentCount = s.Enrollments.Count(e => e.Course.IsActive)
            })
            .ToListAsync(ct);
    }

    public async Task<Guid> CreateAsync(Student student, CancellationToken ct = default)
    {
        student.Id = Guid.NewGuid();
        context.Students.Add(student);
        await context.SaveChangesAsync(ct);
        return student.Id;
    }

    public async Task UpdateAsync(Student student, CancellationToken ct = default)
    {
        context.Students.Update(student);
        await context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// ExecuteDeleteAsync (EF Core 7+) is more efficient than loading entity first.
    /// </summary>
    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await context.Students
            .Where(s => s.Id == id)
            .ExecuteDeleteAsync(ct);
    }

    public async Task<bool> ExistsWithEmailAsync(string email, CancellationToken ct = default)
    {
        return await context.Students
            .AsNoTracking()
            .AnyAsync(s => s.Email == email, ct);
    }

    public async Task<int> GetEnrollmentCountAsync(Guid studentId, CancellationToken ct = default)
    {
        return await context.Enrollments
            .AsNoTracking()
            .CountAsync(e => e.StudentId == studentId, ct);
    }
}

// ============================================================================
// Compiled Queries
// ============================================================================

/// <summary>
/// Compiled queries are compiled once and reused, improving performance for frequently executed queries.
/// </summary>
public static class StudentCompiledQueries
{
    private static readonly Func<StudentDbContext, Guid, Task<Student?>> GetByIdQuery =
        EF.CompileAsyncQuery((StudentDbContext context, Guid id) =>
            context.Students
                .AsNoTracking()
                .FirstOrDefault(s => s.Id == id));

    private static readonly Func<StudentDbContext, string, IAsyncEnumerable<Student>> GetByLastNameQuery =
        EF.CompileAsyncQuery((StudentDbContext context, string lastName) =>
            context.Students
                .AsNoTracking()
                .Where(s => s.LastName == lastName)
                .OrderBy(s => s.FirstName));

    public static async Task<Student?> GetByIdAsync(
        StudentDbContext context,
        Guid id,
        CancellationToken ct = default)
    {
        return await GetByIdQuery(context, id);
    }

    public static async Task<List<Student>> GetByLastNameAsync(
        StudentDbContext context,
        string lastName,
        CancellationToken ct = default)
    {
        var results = new List<Student>();
        await foreach (var student in GetByLastNameQuery(context, lastName).WithCancellation(ct))
        {
            results.Add(student);
        }
        return results;
    }
}

// ============================================================================
// DTOs
// ============================================================================

public class StudentDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public int EnrollmentCount { get; set; }
}

public class StudentSummaryDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int ActiveEnrollmentCount { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}

// Placeholder entities for compilation
public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Enrollment
{
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

public class Course
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class StudentDbContext(DbContextOptions<StudentDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
}

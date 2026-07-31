using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Education.Data.Queries;

/// <summary>
/// Query examples demonstrating EF Core best practices:
/// - Avoiding N+1 query problems with proper Include usage
/// - Using projection to reduce data transfer
/// - Database functions for complex operations
/// - Raw SQL with parameterization for security
/// - Query filters and specifications pattern
/// - IQueryable composition
/// </summary>
public class CourseQueryService(StudentDbContext context)
{
    private readonly StudentDbContext _context = context;

    // ========================================================================
    // AVOIDING N+1 QUERY PROBLEMS
    // ========================================================================

    /// <summary>
    /// BAD: N+1 problem - queries database once per course.
    /// This executes 1 query for courses + N queries for enrollments.
    /// </summary>
    public async Task<List<CourseWithCountDto>> GetCoursesWithEnrollmentCountBadAsync(CancellationToken ct = default)
    {
        var courses = await _context.Courses
            .AsNoTracking()
            .ToListAsync(ct);

        var result = new List<CourseWithCountDto>();
        foreach (var course in courses)
        {
            // ⚠️ This queries the database for EACH course - N+1 problem!
            var count = await _context.Enrollments
                .CountAsync(e => e.CourseId == course.Id, ct);

            result.Add(new CourseWithCountDto(course.Id, course.Title, count));
        }
        return result;
    }

    /// <summary>
    /// GOOD: Single query with projection - executes one query.
    /// </summary>
    public async Task<List<CourseWithCountDto>> GetCoursesWithEnrollmentCountGoodAsync(CancellationToken ct = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Select(c => new CourseWithCountDto(c.Id, c.Title, c.Enrollments.Count))
            .ToListAsync(ct);
    }

    /// <summary>
    /// ALTERNATIVE: Using Include to load related data in one query.
    /// Use when you need the full entities, not just counts.
    /// </summary>
    public async Task<List<Course>> GetCoursesWithEnrollmentsAsync(CancellationToken ct = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Include(c => c.Enrollments)
                .ThenInclude(e => e.Student)
            .AsSplitQuery() // Prevents cartesian explosion
            .Where(c => c.IsActive)
            .ToListAsync(ct);
    }

    // ========================================================================
    // PROJECTION FOR PERFORMANCE
    // ========================================================================

    /// <summary>
    /// Projection with Select retrieves only needed fields.
    /// More efficient than loading full entities.
    /// </summary>
    public async Task<List<CourseDetailDto>> GetCourseDetailsAsync(CancellationToken ct = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Code)
            .Select(c => new CourseDetailDto(
                c.Id,
                c.Code,
                c.Title,
                c.Credits,
                c.Enrollments.Count,
                c.Enrollments
                    .Select(e => e.Student.FirstName + " " + e.Student.LastName)
                    .ToList()))
            .ToListAsync(ct);
    }

    // ========================================================================
    // DATABASE FUNCTIONS
    // ========================================================================

    /// <summary>
    /// Using EF.Functions to call database-specific functions.
    /// </summary>
    public async Task<List<Course>> SearchCoursesAsync(string searchTerm, CancellationToken ct = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => EF.Functions.Like(c.Title, $"%{searchTerm}%") ||
                       EF.Functions.Like(c.Code, $"%{searchTerm}%"))
            .OrderBy(c => c.Title)
            .ToListAsync(ct);
    }

    /// <summary>
    /// Using date functions for filtering.
    /// </summary>
    public async Task<List<Enrollment>> GetRecentEnrollmentsAsync(int daysAgo, CancellationToken ct = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysAgo);

        return await _context.Enrollments
            .AsNoTracking()
            .Where(e => e.EnrolledDate >= cutoffDate)
            .Include(e => e.Student)
            .Include(e => e.Course)
            .OrderByDescending(e => e.EnrolledDate)
            .ToListAsync(ct);
    }

    // ========================================================================
    // RAW SQL QUERIES (with proper parameterization)
    // ========================================================================

    /// <summary>
    /// Using FromSqlInterpolated for safe parameterization.
    /// NEVER concatenate strings in SQL - always use parameters.
    /// </summary>
    public async Task<List<Course>> GetCoursesByTitleAsync(string title, CancellationToken ct = default)
    {
        // FromSqlInterpolated automatically parameterizes values
        return await _context.Courses
            .FromSqlInterpolated($"SELECT * FROM Courses WHERE Title = {title}")
            .AsNoTracking()
            .ToListAsync(ct);
    }

    /// <summary>
    /// Using FromSqlRaw with explicit parameters.
    /// </summary>
    public async Task<List<Course>> GetCoursesByCreditsAsync(int minCredits, CancellationToken ct = default)
    {
        return await _context.Courses
            .FromSqlRaw("SELECT * FROM Courses WHERE Credits >= {0} AND IsActive = 1", minCredits)
            .AsNoTracking()
            .ToListAsync(ct);
    }

    // ========================================================================
    // IQUERYABLE COMPOSITION - Specification Pattern
    // ========================================================================

    /// <summary>
    /// Building reusable query filters with composition.
    /// IQueryable allows composing queries before execution.
    /// </summary>
    public async Task<List<Course>> SearchCoursesWithFiltersAsync(
        CourseFilter filter,
        CancellationToken ct = default)
    {
        var query = _context.Courses.AsNoTracking();

        if (filter.IsActive.HasValue)
            query = query.Where(c => c.IsActive == filter.IsActive.Value);

        if (filter.MinCredits.HasValue)
            query = query.Where(c => c.Credits >= filter.MinCredits.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            query = query.Where(c => c.Title.Contains(filter.SearchTerm) ||
                                    c.Code.Contains(filter.SearchTerm));

        if (filter.HasEnrollments)
            query = query.Where(c => c.Enrollments.Any());

        return await query.OrderBy(c => c.Code).ToListAsync(ct);
    }

    // ========================================================================
    // GROUPING AND AGGREGATION
    // ========================================================================

    /// <summary>
    /// GroupBy with aggregation functions.
    /// </summary>
    public async Task<List<CourseStatisticsDto>> GetCourseStatisticsAsync(CancellationToken ct = default)
    {
        return await _context.Courses
            .AsNoTracking()
            .Where(c => c.IsActive)
            .GroupBy(c => c.Credits)
            .Select(g => new CourseStatisticsDto(
                g.Key,
                g.Count(),
                g.Sum(c => c.Enrollments.Count),
                g.Average(c => c.Enrollments.Count)))
            .OrderBy(s => s.Credits)
            .ToListAsync(ct);
    }
}

// ============================================================================
// DTOs and Filters
// ============================================================================

public record CourseWithCountDto(Guid Id, string Title, int EnrollmentCount);

public record CourseDetailDto(
    Guid Id,
    string Code,
    string Title,
    int Credits,
    int EnrollmentCount,
    List<string> EnrolledStudentNames);

public record CourseStatisticsDto(
    int Credits,
    int CourseCount,
    int TotalEnrollments,
    double AverageEnrollments);

public class CourseFilter
{
    public bool? IsActive { get; set; }
    public int? MinCredits { get; set; }
    public string? SearchTerm { get; set; }
    public bool HasEnrollments { get; set; }
}
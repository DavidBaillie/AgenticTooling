using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Education.Data.Concurrency;

/// <summary>
/// Demonstrates optimistic concurrency control in EF Core:
/// - Using RowVersion/Timestamp for automatic concurrency tokens
/// - Using ConcurrencyCheck attribute for property-level concurrency
/// - Handling DbUpdateConcurrencyException
/// - Resolving concurrency conflicts
/// - Using IsConcurrencyToken in fluent API
/// </summary>

// ============================================================================
// ENTITIES WITH CONCURRENCY TOKENS
// ============================================================================

/// <summary>
/// Student entity with RowVersion for optimistic concurrency.
/// RowVersion is a byte array that SQL Server automatically updates on every change.
/// </summary>
public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }

    // RowVersion - SQL Server timestamp, auto-updated on each change
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
}

/// <summary>
/// Course entity with property-based concurrency check.
/// Useful when you want to check specific properties for conflicts.
/// </summary>
public class Course
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Credits { get; set; }

    // Version field manually incremented for concurrency checking
    public int Version { get; set; }

    // Last modified timestamp for tracking changes
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Enrollment entity combining both approaches.
/// </summary>
public class Enrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrolledDate { get; set; }
    public string? Grade { get; set; }

    // RowVersion for optimistic concurrency
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    // Navigation properties
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

// ============================================================================
// ENTITY CONFIGURATIONS
// ============================================================================

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(255);

        // Configure RowVersion as concurrency token
        // SQL Server: ROWVERSION, PostgreSQL: xmin, SQLite: custom trigger needed
        builder.Property(s => s.RowVersion)
            .IsRowVersion(); // Automatically managed by database
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);

        // Configure Version as concurrency token (manually managed)
        builder.Property(c => c.Version)
            .IsConcurrencyToken();

        // Configure LastModified as concurrency token
        builder.Property(c => c.LastModified)
            .IsConcurrencyToken();
    }
}

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RowVersion)
            .IsRowVersion();

        builder.HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId);

        builder.HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId);
    }
}

// ============================================================================
// CONCURRENCY HANDLING SERVICE
// ============================================================================

public interface IStudentService
{
    Task<bool> UpdateStudentEmailAsync(Guid studentId, string newEmail, CancellationToken ct = default);
    Task<bool> UpdateCourseAsync(Guid courseId, string title, int credits, CancellationToken ct = default);
}

public class StudentService(StudentDbContext context) : IStudentService
{
    private readonly StudentDbContext _context = context;

    /// <summary>
    /// Update with automatic retry on concurrency conflict.
    /// Uses RowVersion which is automatically managed by database.
    /// </summary>
    public async Task<bool> UpdateStudentEmailAsync(
        Guid studentId,
        string newEmail,
        CancellationToken ct = default)
    {
        const int maxRetries = 3;

        for (int retry = 0; retry < maxRetries; retry++)
        {
            try
            {
                // Load student with current RowVersion
                var student = await _context.Students.FindAsync(new object[] { studentId }, ct);

                if (student == null)
                    return false;

                // Modify entity
                student.Email = newEmail;

                // RowVersion is automatically included in WHERE clause
                // UPDATE Students SET Email = @p0, RowVersion = <new_version>
                // WHERE Id = @p1 AND RowVersion = @p2
                await _context.SaveChangesAsync(ct);

                return true; // Success
            }
            catch (DbUpdateConcurrencyException)
            {
                if (retry == maxRetries - 1)
                {
                    // Max retries reached, give up
                    throw;
                }

                // Wait a bit before retrying
                await Task.Delay(100 * (retry + 1), ct);
            }
        }

        return false;
    }

    /// <summary>
    /// Update with manual version increment for concurrency checking.
    /// </summary>
    public async Task<bool> UpdateCourseAsync(
        Guid courseId,
        string title,
        int credits,
        CancellationToken ct = default)
    {
        try
        {
            var course = await _context.Courses.FindAsync(new object[] { courseId }, ct);

            if (course == null)
                return false;

            // Update properties
            course.Title = title;
            course.Credits = credits;

            // Manually increment version (concurrency token)
            course.Version++;
            course.LastModified = DateTime.UtcNow;

            // EF includes Version in WHERE clause:
            // UPDATE Courses SET Title = @p0, Credits = @p1, Version = @p2, LastModified = @p3
            // WHERE Id = @p4 AND Version = @p5 AND LastModified = @p6
            await _context.SaveChangesAsync(ct);

            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            // Conflict detected - another user modified the record
            throw new InvalidOperationException(
                "The course was modified by another user. Please reload and try again.", ex);
        }
    }

    /// <summary>
    /// Advanced conflict resolution - choose which values to keep.
    /// </summary>
    public async Task<bool> UpdateStudentWithConflictResolutionAsync(
        Guid studentId,
        string newEmail,
        ConflictResolutionStrategy strategy,
        CancellationToken ct = default)
    {
        try
        {
            var student = await _context.Students.FindAsync(new object[] { studentId }, ct);

            if (student == null)
                return false;

            student.Email = newEmail;

            await _context.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.Single();
            var databaseValues = await entry.GetDatabaseValuesAsync(ct);

            if (databaseValues == null)
                throw new InvalidOperationException("The student was deleted by another user.");

            switch (strategy)
            {
                case ConflictResolutionStrategy.ClientWins:
                    // Keep our changes, overwrite database
                    entry.OriginalValues.SetValues(databaseValues);
                    await _context.SaveChangesAsync(ct);
                    break;

                case ConflictResolutionStrategy.DatabaseWins:
                    // Discard our changes, accept database values
                    entry.CurrentValues.SetValues(databaseValues);
                    break;

                case ConflictResolutionStrategy.MergeValues:
                    // Example: keep client's email, merge other fields from database
                    var clientEmail = entry.CurrentValues["Email"];
                    entry.CurrentValues.SetValues(databaseValues);
                    entry.CurrentValues["Email"] = clientEmail;
                    entry.OriginalValues.SetValues(databaseValues);
                    await _context.SaveChangesAsync(ct);
                    break;

                case ConflictResolutionStrategy.ThrowError:
                default:
                    throw;
            }

            return true;
        }
    }

    /// <summary>
    /// Batch update with concurrency handling.
    /// </summary>
    public async Task<(int Updated, int Conflicts)> BatchUpdateStudentsAsync(
        List<(Guid Id, string NewEmail)> updates,
        CancellationToken ct = default)
    {
        int updated = 0;
        int conflicts = 0;

        foreach (var (id, newEmail) in updates)
        {
            try
            {
                var student = await _context.Students.FindAsync(new object[] { id }, ct);
                if (student != null)
                {
                    student.Email = newEmail;
                    await _context.SaveChangesAsync(ct);
                    updated++;
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                conflicts++;
                _context.ChangeTracker.Clear();
            }
        }

        return (updated, conflicts);
    }

    /// <summary>
    /// Disconnected entity update pattern (common in web APIs).
    /// </summary>(common in web APIs).
    /// Client sends entity with original RowVersion from GET request.
    /// </summary>
    public async Task<bool> UpdateStudentDisconnectedAsync(
        Student studentDto,
        byte[] originalRowVersion,
        CancellationToken ct = default)
    {
        studentDto.RowVersion = originalRowVersion;
        _context.Attach(studentDto);
        _context.Entry(studentDto).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync(ct); return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "The student was modified by another user. Please reload and try again.");
        }
    }
}

// ============================================================================
// SUPPORTING TYPES
// ============================================================================

public enum ConflictResolutionStrategy
{
    ClientWins,      // Overwrite database with client values
    DatabaseWins,    // Discard client changes, keep database values
    MergeValues,     // Merge client and database values
    ThrowError       // Throw exception and let caller handle
}

public class StudentDbContext(DbContextOptions<StudentDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
        modelBuilder.ApplyConfiguration(new EnrollmentConfiguration());
    }
}

// ============================================================================
// WEB API DTOs - Include RowVersion for round-trip concurrency checking
// ============================================================================

public record StudentUpdateRequest(Guid Id, string Email, byte[] RowVersion);
public record StudentResponse(Guid Id, string Email, byte[] RowVersion);
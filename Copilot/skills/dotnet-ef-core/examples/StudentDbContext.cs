using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Education.Data;

/// <summary>
/// Example DbContext demonstrating EF Core best practices:
/// - Primary constructor with options injection
/// - Separate entity configurations using IEntityTypeConfiguration
/// - Clean OnModelCreating with ApplyConfigurationsFromAssembly
/// </summary>
public class StudentDbContext(DbContextOptions<StudentDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StudentDbContext).Assembly);
    }
}

// ============================================================================
// ENTITY CLASSES
// ============================================================================

public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Course
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public int Credits { get; set; }
    public bool IsActive { get; set; }
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}

public class Enrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid CourseId { get; set; }
    public DateTime EnrolledDate { get; set; }
    public string? Grade { get; set; }
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}

// ============================================================================
// ENTITY CONFIGURATIONS - IEntityTypeConfiguration Pattern
// ============================================================================

/// <summary>
/// Student entity configuration using IEntityTypeConfiguration pattern.
/// </summary>
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(255);
        builder.Property(e => e.EnrollmentDate).IsRequired();
        builder.Property(e => e.RowVersion).IsRowVersion();

        builder.HasIndex(e => e.Email).IsUnique();
        builder.HasIndex(e => new { e.LastName, e.FirstName });

        builder.HasMany(e => e.Enrollments)
            .WithOne(e => e.Student)
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Credits).IsRequired();
        builder.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);

        builder.HasIndex(e => e.Code).IsUnique();

        builder.HasMany(e => e.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EnrolledDate).IsRequired();
        builder.Property(e => e.Grade).HasMaxLength(5);

        builder.HasIndex(e => new { e.StudentId, e.CourseId }).IsUnique();
        builder.HasIndex(e => e.CourseId);
    }
}

// ============================================================================
// DbContextFactory Pattern - For console apps, tests, or pooling
// ============================================================================

/// <summary>
/// Example service using IDbContextFactory for creating DbContext instances.
/// Useful for console apps, background services, or when you need multiple contexts.
/// </summary>
public class StudentDbContextFactory(IDbContextFactory<StudentDbContext> factory)
{
    public async Task<List<Student>> GetActiveStudentsAsync(CancellationToken ct = default)
    {
        await using var context = await factory.CreateDbContextAsync(ct);
        return await context.Students
            .AsNoTracking()
            .Where(s => s.Enrollments.Any(e => e.Course.IsActive))
            .ToListAsync(ct);
    }
}

// ============================================================================
// Registration Examples
// ============================================================================

public static class DbContextRegistrationExamples
{
    // Standard registration for web applications
    public static void RegisterDbContext(IServiceCollection services, string connectionString) =>
        services.AddDbContext<StudentDbContext>(options => options.UseSqlServer(connectionString));

    // DbContextFactory registration for console apps or when you need multiple contexts
    public static void RegisterDbContextFactory(IServiceCollection services, string connectionString) =>
        services.AddDbContextFactory<StudentDbContext>(options => options.UseSqlServer(connectionString));

    // Pooled DbContext for high-performance scenarios (stateless contexts only)
    public static void RegisterPooledDbContext(IServiceCollection services, string connectionString) =>
        services.AddDbContextPool<StudentDbContext>(options => options.UseSqlServer(connectionString));
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Education.Data.Repositories;

/// <summary>
/// Demonstrates async best practices: proper naming, CancellationToken, Task.WhenAll, IAsyncEnumerable, and ValueTask.
/// </summary>
public class ClassroomRepositoryAsync(EducationDbContext context, ILogger<ClassroomRepositoryAsync> logger)
{
    // Async method with CancellationToken support
    public async Task<Classroom?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        try
        {
            return await context.Classrooms.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id, ct);
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Operation cancelled for classroom {Id}", id);
            throw;
        }
    }

    // Elide async/await when just passing through
    public Task<Guid> CreateAsync(Classroom classroom, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(classroom);
        return CreateInternalAsync(classroom, ct);
    }

    private async Task<Guid> CreateInternalAsync(Classroom classroom, CancellationToken ct)
    {
        classroom.Id = Guid.NewGuid();
        context.Classrooms.Add(classroom);
        await context.SaveChangesAsync(ct);
        return classroom.Id;
    }

    // Task.WhenAll for parallel operations
    public async Task<Dictionary<Guid, Classroom>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
    {
        var idList = ids.ToList();
        if (!idList.Any()) return new();

        var tasks = idList.Select(id => GetByIdAsync(id, ct));
        var results = await Task.WhenAll(tasks);
        return results.Where(c => c != null).ToDictionary(c => c!.Id, c => c!);
    }

    // IAsyncEnumerable for streaming results
    public async IAsyncEnumerable<Classroom> GetAllAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        var query = context.Classrooms.AsNoTracking().OrderBy(c => c.Building).AsAsyncEnumerable();
        await foreach (var classroom in query.WithCancellation(ct))
            yield return classroom;
    }

    // ValueTask for high-performance scenarios
    public async ValueTask<bool> ExistsAsync(Guid id, CancellationToken ct = default)
    {
        return await context.Classrooms.AsNoTracking().AnyAsync(c => c.Id == id, ct);
    }
}

public class Classroom
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public string Building { get; set; } = string.Empty;
}

public class EducationDbContext : DbContext
{
    public EducationDbContext(DbContextOptions<EducationDbContext> options) : base(options)
    {
    }

    public DbSet<Classroom> Classrooms { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Building).IsRequired().HasMaxLength(100);
        });
    }
}

// Example of common anti-patterns (DO NOT USE)
#region Anti-Patterns - DO NOT USE

public class ClassroomRepositoryBadExamples
{
    private readonly EducationDbContext _context;

    public ClassroomRepositoryBadExamples(EducationDbContext context)
    {
        _context = context;
    }

    // ❌ BAD: Using .Result blocks the calling thread and can cause deadlocks
    public Classroom GetClassroomBlocking(Guid id)
    {
        return GetClassroomByIdAsync(id).Result; // NEVER DO THIS
    }

    // ❌ BAD: Using .Wait() blocks the calling thread
    public void UpdateClassroomBlocking(Classroom classroom)
    {
        UpdateClassroomAsync(classroom).Wait(); // NEVER DO THIS
    }

    // ❌ BAD: async void should only be used for event handlers
    public async void DeleteClassroomAsync(Guid id) // AVOID - should return Task
    {
        var classroom = await _context.Classrooms.FindAsync(id);
        if (classroom != null)
        {
            _context.Classrooms.Remove(classroom);
            await _context.SaveChangesAsync();
        }
        // Exceptions thrown here can crash the application
    }

    // ❌ BAD: Swallowing exceptions
    public async Task<Classroom?> GetClassroomSilentFailAsync(Guid id)
    {
        try
        {
            return await GetClassroomByIdAsync(id);
        }
        catch
        {
            return null; // Lost valuable error information
        }
    }

    // ❌ BAD: Not using cancellation tokens for long-running operations
    public async Task<List<Classroom>> GetAllClassroomsNoCancellationAsync()
    {
        // No way to cancel this operation
        return await _context.Classrooms.ToListAsync();
    }

    // Placeholder methods for anti-pattern examples
    private Task<Classroom> GetClassroomByIdAsync(Guid id) => Task.FromResult(new Classroom());
    private Task UpdateClassroomAsync(Classroom classroom) => Task.CompletedTask;
}

#endregion

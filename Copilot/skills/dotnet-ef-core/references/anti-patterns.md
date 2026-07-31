# Entity Framework Core Anti-Patterns

Common mistakes to avoid when working with EF Core, with examples of what NOT to do and the correct approach.

---

## Anti-Pattern 1: Not Using AsNoTracking for Read-Only Queries

**Wrong:**
```csharp
// Tracking overhead for read-only data
public async Task<List<Student>> GetStudentsForDisplayAsync()
{
    return await _context.Students.ToListAsync();
}
```

**Why it's wrong:**
- Change tracking adds memory and CPU overhead
- Unnecessary for read-only operations
- Impacts performance at scale

**Correct:**
```csharp
// Use AsNoTracking for read-only queries
public async Task<List<Student>> GetStudentsForDisplayAsync()
{
    return await _context.Students
        .AsNoTracking()
        .ToListAsync();
}
```

---

## Anti-Pattern 2: N+1 Query Problem

**Wrong:**
```csharp
// Executes 1 query for students + N queries for enrollments
public async Task<List<StudentWithEnrollmentCount>> GetStudentsWithCountAsync()
{
    var students = await _context.Students.ToListAsync();
    
    var result = new List<StudentWithEnrollmentCount>();
    foreach (var student in students)
    {
        // ⚠️ Additional database query for EACH student!
        var count = await _context.Enrollments
            .CountAsync(e => e.StudentId == student.Id);
        
        result.Add(new StudentWithEnrollmentCount 
        { 
            Student = student, 
            EnrollmentCount = count 
        });
    }
    return result;
}
```

**Why it's wrong:**
- Executes N+1 database queries (1 for students, N for enrollments)
- Massive performance hit with large datasets
- Network latency multiplied by number of records

**Correct:**
```csharp
// Single query with projection
public async Task<List<StudentWithEnrollmentCount>> GetStudentsWithCountAsync()
{
    return await _context.Students
        .AsNoTracking()
        .Select(s => new StudentWithEnrollmentCount
        {
            StudentId = s.Id,
            StudentName = s.FirstName + " " + s.LastName,
            EnrollmentCount = s.Enrollments.Count // EF translates to SQL COUNT
        })
        .ToListAsync();
}
```

---

## Anti-Pattern 3: Calling SaveChanges in a Loop

**Wrong:**
```csharp
// SaveChanges called multiple times in loop
public async Task ImportStudentsAsync(List<Student> students)
{
    foreach (var student in students)
    {
        _context.Students.Add(student);
        await _context.SaveChangesAsync(); // ⚠️ Database round-trip per student!
    }
}
```

**Why it's wrong:**
- Database round-trip for every record
- Not transactional - partial failures leave inconsistent data
- Very slow for bulk operations

**Correct:**
```csharp
// Batch SaveChanges after all adds
public async Task ImportStudentsAsync(List<Student> students)
{
    _context.Students.AddRange(students);
    await _context.SaveChangesAsync(); // Single database round-trip
}
```

---

## Anti-Pattern 4: Not Including Related Data (Lazy Loading Issues)

**Wrong:**
```csharp
// Without Include, accessing navigation properties causes additional queries
public async Task<string> GetStudentWithCoursesAsync(Guid studentId)
{
    var student = await _context.Students
        .FirstOrDefaultAsync(s => s.Id == studentId);
    
    if (student == null) return string.Empty;
    
    // ⚠️ This triggers a separate query for enrollments (if lazy loading enabled)
    // or throws exception if lazy loading disabled
    var courseCount = student.Enrollments.Count;
    
    return $"{student.FirstName} has {courseCount} enrollments";
}
```

**Why it's wrong:**
- Causes additional database queries (N+1 problem)
- Can throw exceptions if lazy loading is disabled
- Unpredictable performance

**Correct:**
```csharp
// Explicitly include related data with Include
public async Task<string> GetStudentWithCoursesAsync(Guid studentId)
{
    var student = await _context.Students
        .Include(s => s.Enrollments)
        .AsNoTracking()
        .FirstOrDefaultAsync(s => s.Id == studentId);
    
    if (student == null) return string.Empty;
    
    // Enrollments are already loaded - no additional query
    var courseCount = student.Enrollments.Count;
    
    return $"{student.FirstName} has {courseCount} enrollments";
}
```

---

## Anti-Pattern 5: Over-Including Related Data (Cartesian Explosion)

**Wrong:**
```csharp
// Including multiple collections without AsSplitQuery
public async Task<Student?> GetStudentWithAllDataAsync(Guid id)
{
    return await _context.Students
        .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
        .Include(s => s.Payments)
            .ThenInclude(p => p.PaymentMethod)
        .Include(s => s.Documents)
        .FirstOrDefaultAsync(s => s.Id == id);
    // ⚠️ Cartesian explosion: Student data duplicated for every combination
}
```

**Why it's wrong:**
- Creates cartesian product (student × enrollments × payments × documents)
- Massive data transfer over network
- Student data duplicated many times

**Correct:**
```csharp
// Use AsSplitQuery to avoid cartesian explosion
public async Task<Student?> GetStudentWithAllDataAsync(Guid id)
{
    return await _context.Students
        .Include(s => s.Enrollments)
            .ThenInclude(e => e.Course)
        .Include(s => s.Payments)
            .ThenInclude(p => p.PaymentMethod)
        .Include(s => s.Documents)
        .AsSplitQuery() // Generates separate queries for each Include
        .FirstOrDefaultAsync(s => s.Id == id);
}
```

---

## Anti-Pattern 6: Not Using Pagination for Large Result Sets

**Wrong:**
```csharp
// Returns all records - memory and performance disaster
public async Task<List<Student>> SearchStudentsAsync(string searchTerm)
{
    return await _context.Students
        .Where(s => s.LastName.Contains(searchTerm))
        .ToListAsync(); // ⚠️ Could return thousands of records!
}
```

**Why it's wrong:**
- Loads all matching records into memory
- Can cause out-of-memory exceptions
- Slow response times

**Correct:**
```csharp
// Implement pagination
public async Task<PagedResult<Student>> SearchStudentsAsync(
    string searchTerm, 
    int page, 
    int pageSize)
{
    var query = _context.Students
        .AsNoTracking()
        .Where(s => s.LastName.Contains(searchTerm));
    
    var total = await query.CountAsync();
    
    var students = await query
        .OrderBy(s => s.LastName)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<Student>
    {
        Items = students,
        TotalCount = total,
        PageNumber = page,
        PageSize = pageSize
    };
}
```

---

## Anti-Pattern 7: Loading Full Entities When Only Few Properties Needed

**Wrong:**
```csharp
// Loads all properties when only need name
public async Task<List<string>> GetStudentNamesAsync()
{
    var students = await _context.Students.ToListAsync();
    return students.Select(s => s.FirstName + " " + s.LastName).ToList();
    // ⚠️ Transferred all student data over network, only used names
}
```

**Why it's wrong:**
- Unnecessary data transfer
- Wasted memory
- Slower than projection

**Correct:**
```csharp
// Use projection to select only needed properties
public async Task<List<string>> GetStudentNamesAsync()
{
    return await _context.Students
        .AsNoTracking()
        .Select(s => s.FirstName + " " + s.LastName)
        .ToListAsync();
    // Only transfers name data from database
}
```

---

## Anti-Pattern 8: SQL Injection with String Concatenation

**Wrong:**
```csharp
// DANGEROUS: SQL injection vulnerability!
public async Task<List<Student>> SearchByNameUnsafeAsync(string name)
{
    return await _context.Students
        .FromSqlRaw($"SELECT * FROM Students WHERE LastName = '{name}'")
        .ToListAsync();
    // ⚠️ If name = "'; DROP TABLE Students; --" your table is gone!
}
```

**Why it's wrong:**
- SQL injection attack vulnerability
- Can lead to data loss, theft, or corruption
- Security disaster

**Correct:**
```csharp
// Use parameterized queries
public async Task<List<Student>> SearchByNameSafeAsync(string name)
{
    // Option 1: FormattableString (preferred)
    return await _context.Students
        .FromSqlInterpolated($"SELECT * FROM Students WHERE LastName = {name}")
        .ToListAsync();
    
    // Option 2: Explicit parameters
    return await _context.Students
        .FromSqlRaw("SELECT * FROM Students WHERE LastName = {0}", name)
        .ToListAsync();
}
```

---

## Anti-Pattern 9: Not Using Transactions for Multi-Operation Changes

**Wrong:**
```csharp
// Multiple SaveChanges without transaction - inconsistent on failure
public async Task TransferStudentAsync(Guid studentId, Guid fromCourseId, Guid toCourseId)
{
    // Remove from old course
    var oldEnrollment = await _context.Enrollments
        .FirstAsync(e => e.StudentId == studentId && e.CourseId == fromCourseId);
    _context.Enrollments.Remove(oldEnrollment);
    await _context.SaveChangesAsync();
    
    // ⚠️ If this fails, student removed from old course but not added to new one!
    var newEnrollment = new Enrollment 
    { 
        StudentId = studentId, 
        CourseId = toCourseId 
    };
    _context.Enrollments.Add(newEnrollment);
    await _context.SaveChangesAsync();
}
```

**Why it's wrong:**
- Not atomic - partial failures leave inconsistent data
- Student could be removed from old course but not added to new one

**Correct:**
```csharp
// Use transaction to make operations atomic
public async Task TransferStudentAsync(Guid studentId, Guid fromCourseId, Guid toCourseId)
{
    using var transaction = await _context.Database.BeginTransactionAsync();
    
    try
    {
        // Remove from old course
        var oldEnrollment = await _context.Enrollments
            .FirstAsync(e => e.StudentId == studentId && e.CourseId == fromCourseId);
        _context.Enrollments.Remove(oldEnrollment);
        
        // Add to new course
        var newEnrollment = new Enrollment 
        { 
            StudentId = studentId, 
            CourseId = toCourseId 
        };
        _context.Enrollments.Add(newEnrollment);
        
        // Both operations succeed or both fail
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

---

## Anti-Pattern 10: Improper DbContext Lifetime Management

**Wrong:**
```csharp
// Storing DbContext as singleton or static field
public class StudentRepository
{
    private static readonly StudentDbContext _context = new(); // ⚠️ NEVER DO THIS!
    
    public async Task<Student?> GetStudentAsync(Guid id)
    {
        return await _context.Students.FindAsync(id);
    }
}
```

**Why it's wrong:**
- DbContext is not thread-safe
- Causes concurrency issues
- Connection leaks
- Memory leaks from change tracker

**Correct:**
```csharp
// Inject DbContext per request (scoped lifetime)
public class StudentRepository
{
    private readonly StudentDbContext _context;
    
    // DbContext injected via constructor (scoped in web apps)
    public StudentRepository(StudentDbContext context)
    {
        _context = context;
    }
    
    public async Task<Student?> GetStudentAsync(Guid id)
    {
        return await _context.Students.AsNoTracking().FirstOrDefaultAsync(s => s.Id == id);
    }
}

// Registration in Program.cs
services.AddDbContext<StudentDbContext>(options => 
    options.UseSqlServer(connectionString)); // Scoped by default
```

---

## Summary

When reviewing EF Core code, watch for:

1. **Performance**: AsNoTracking, pagination, projection, N+1 queries
2. **Data Loading**: Proper use of Include, ThenInclude, AsSplitQuery
3. **Batching**: Single SaveChanges for bulk operations
4. **Security**: Parameterized queries, never string concatenation
5. **Transactions**: Use for multi-operation changes
6. **Lifetime**: Proper DbContext scoping
7. **Cartesian Explosion**: AsSplitQuery with multiple collections
8. **Concurrency**: RowVersion for optimistic locking

These anti-patterns cause most EF Core performance and correctness issues in production.

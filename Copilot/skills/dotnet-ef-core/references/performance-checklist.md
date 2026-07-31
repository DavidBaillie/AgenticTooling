# Entity Framework Core Performance Checklist

A systematic checklist for reviewing and optimizing EF Core performance.

---

## Query Performance

### Read-Only Queries
- [ ] Use `AsNoTracking()` for all read-only queries
- [ ] Use `AsNoTrackingWithIdentityResolution()` when tracking same entities multiple times
- [ ] Verify tracking is enabled only when modifying entities

**Example:**
```csharp
// Read-only - use AsNoTracking
var students = await context.Students.AsNoTracking().ToListAsync();

// Will modify - tracking enabled by default
var student = await context.Students.FindAsync(id);
student.Email = newEmail;
await context.SaveChangesAsync();
```

### Projection and Filtering
- [ ] Use `Select()` to retrieve only needed columns
- [ ] Apply `Where()` filters before `Include()` or projections
- [ ] Project to DTOs instead of loading full entities when possible
- [ ] Move filtering logic to SQL rather than client-side with `AsEnumerable()`

**Example:**
```csharp
// Good - projection retrieves only needed data
var studentDtos = await context.Students
    .AsNoTracking()
    .Where(s => s.IsActive)
    .Select(s => new StudentDto
    {
        Id = s.Id,
        FullName = s.FirstName + " " + s.LastName,
        EnrollmentCount = s.Enrollments.Count
    })
    .ToListAsync();
```

### Pagination
- [ ] Implement `Skip()` and `Take()` for large result sets
- [ ] Always include `OrderBy()` before pagination for consistent results
- [ ] Consider using keyset/cursor pagination for very large datasets
- [ ] Return total count separately when needed

**Example:**
```csharp
var pageSize = 20;
var pageNumber = 1;

var query = context.Students.AsNoTracking().OrderBy(s => s.LastName);
var totalCount = await query.CountAsync();
var students = await query
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

### Eager Loading
- [ ] Use `Include()` to load related data in single query
- [ ] Use `ThenInclude()` for nested relationships
- [ ] Apply `AsSplitQuery()` when loading multiple collections to avoid cartesian explosion
- [ ] Avoid over-including - only load what you need

**Example:**
```csharp
// Multiple collections - use AsSplitQuery
var student = await context.Students
    .Include(s => s.Enrollments)
        .ThenInclude(e => e.Course)
    .Include(s => s.Payments)
    .AsSplitQuery() // Prevents cartesian explosion
    .FirstOrDefaultAsync(s => s.Id == id);
```

### Avoiding N+1 Problems
- [ ] Check for navigation property access in loops
- [ ] Use `Include()` or projection instead of lazy loading
- [ ] Disable lazy loading by default: `UseLazyLoadingProxies(false)`
- [ ] Use SQL profiler or logging to detect N+1 queries

**Example:**
```csharp
// Bad - N+1 problem
var students = await context.Students.ToListAsync();
foreach (var student in students)
{
    var count = student.Enrollments.Count; // Additional query per student!
}

// Good - single query with projection
var studentCounts = await context.Students
    .Select(s => new { s.Id, Count = s.Enrollments.Count })
    .ToListAsync();
```

---

##  Database Operations

### Bulk Operations
- [ ] Use `AddRange()` instead of multiple `Add()` calls
- [ ] Call `SaveChangesAsync()` once after all changes, not in loops
- [ ] Consider `ExecuteDelete()` / `ExecuteUpdate()` for bulk modifications (EF Core 7+)
- [ ] Use batching for very large datasets (process in chunks)

**Example:**
```csharp
// Good - single SaveChanges
context.Students.AddRange(students);
await context.SaveChangesAsync();

// EF Core 7+ bulk delete
await context.Students
    .Where(s => !s.IsActive)
    .ExecuteDeleteAsync();
```

### Change Tracking
- [ ] Minimize tracked entities by using `AsNoTracking()` where possible
- [ ] Clear change tracker when done: `context.ChangeTracker.Clear()`
- [ ] Use `DetachAllEntities()` in long-running contexts
- [ ] Consider setting `QueryTrackingBehavior.NoTracking` as default

**Example:**
```csharp
// Set no-tracking as default
services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

### Compiled Queries
- [ ] Use compiled queries for frequently executed queries
- [ ] Store compiled queries as static fields
- [ ] Use for performance-critical paths
- [ ] Profile to verify performance improvement

**Example:**
```csharp
private static readonly Func<AppDbContext, Guid, Task<Student?>> GetByIdQuery =
    EF.CompileAsyncQuery((AppDbContext context, Guid id) =>
        context.Students.AsNoTracking().FirstOrDefault(s => s.Id == id));

var student = await GetByIdQuery(context, studentId);
```

---

## Database Design

### Indexes
- [ ] Create indexes on foreign keys
- [ ] Index frequently queried columns
- [ ] Create composite indexes for multi-column queries
- [ ] Use unique indexes for unique constraints
- [ ] Review index usage with database profiling tools

**Example:**
```csharp
builder.HasIndex(s => s.Email).IsUnique();
builder.HasIndex(s => new { s.LastName, s.FirstName });
```

### Concurrency
- [ ] Implement `RowVersion` or concurrency tokens for entities
- [ ] Handle `DbUpdateConcurrencyException` appropriately
- [ ] Use optimistic concurrency for multi-user scenarios
- [ ] Test concurrent update scenarios

**Example:**
```csharp
builder.Property(s => s.RowVersion).IsRowVersion();
```

### Relationships
- [ ] Configure appropriate cascade delete behavior
- [ ] Use `DeleteBehavior.Restrict` to prevent unintended cascades
- [ ] Verify foreign key indexes exist
- [ ] Consider using junction tables for many-to-many

---

## 🔌 Connection and Context Management

### DbContext Lifetime
- [ ] Use scoped lifetime in web applications (default)
- [ ] Use `DbContextFactory` for console apps or background services
- [ ] Never store DbContext as static or singleton
- [ ] Dispose contexts properly with `using` or DI

**Example:**
```csharp
// Web app - scoped (default)
services.AddDbContext<AppDbContext>(options => ...);

// Console app or pooling
services.AddDbContextFactory<AppDbContext>(options => ...);
```

### Connection Pooling
- [ ] Use connection pooling (enabled by default)
- [ ] Configure `MaxPoolSize` for high-concurrency scenarios
- [ ] Monitor connection pool exhaustion
- [ ] Avoid long-running contexts that hold connections

### DbContext Pooling
- [ ] Consider `AddDbContextPool()` for high-traffic web apps
- [ ] Ensure no state is stored in DbContext when pooling
- [ ] Test with pooling enabled before deploying

**Example:**
```csharp
services.AddDbContextPool<AppDbContext>(options => 
    options.UseSqlServer(connectionString),
    poolSize: 128);
```

---

## Monitoring and Profiling

### Query Logging
- [ ] Enable sensitive data logging in development only
- [ ] Log slow queries with `LogTo()` or logging provider
- [ ] Use `TagWith()` to identify queries in logs
- [ ] Monitor generated SQL in production

**Example:**
```csharp
// Development
optionsBuilder.EnableSensitiveDataLogging()
    .LogTo(Console.WriteLine, LogLevel.Information);

// Query tagging
var students = await context.Students
    .TagWith("GetActiveStudents query")
    .Where(s => s.IsActive)
    .ToListAsync();
```

### Performance Metrics
- [ ] Measure query execution time
- [ ] Monitor database round trips (aim for fewer)
- [ ] Track memory usage of change tracker
- [ ] Use Application Insights or similar for telemetry

### Database Profiling
- [ ] Use SQL Server Profiler, pg_stat_statements, or equivalent
- [ ] Identify slow queries and missing indexes
- [ ] Check execution plans for table scans
- [ ] Monitor lock contention and deadlocks

---

## 🏗️ Best Practices

### General Guidelines
- [ ] Keep queries simple and readable
- [ ] Avoid complex business logic in queries
- [ ] Use repository pattern to encapsulate queries
- [ ] Write unit tests for complex queries
- [ ] Document any raw SQL queries

### Code Review Checklist
- [ ] No N+1 query patterns
- [ ] AsNoTracking used for read-only operations
- [ ] Pagination implemented for list endpoints
- [ ] Proper Include/ThenInclude usage
- [ ] No SaveChanges in loops
- [ ] Concurrency tokens where needed
- [ ] Transactions for multi-step operations
- [ ] No SQL injection vulnerabilities

### Testing
- [ ] Load test with realistic data volumes
- [ ] Test with production-like dataset sizes
- [ ] Verify pagination works with large datasets
- [ ] Test concurrent update scenarios
- [ ] Measure and profile performance regularly

---

## Quick Wins

Focus on these high-impact optimizations first:

1. **Add AsNoTracking to all read-only queries** - Easy win, immediate benefit
2. **Fix N+1 queries** - Often 10-100x performance improvement
3. **Implement pagination** - Prevents memory issues
4. **Use projection (Select)** - Reduces data transfer
5. **Add missing indexes** - Can dramatically improve query speed
6. **Batch SaveChanges** - Reduce database round trips

---

## Performance Goals

Target these metrics for good EF Core performance:

- **Query execution time**: < 100ms for most queries
- **Database round trips**: Minimize to essential operations only
- **Memory usage**: Change tracker < 10MB for typical web requests
- **Connection time**: Reuse pooled connections
- **Batch size**: Process 1000+ records efficiently

Use this checklist during code reviews and performance audits to systematically improve EF Core performance.

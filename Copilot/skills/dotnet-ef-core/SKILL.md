---
name: dotnet-ef-core
description: 'Get best practices for Entity Framework'
---

# Entity Framework Core Best Practices

Your goal is to help me follow best practices when working with Entity Framework Core.

## Quick Reference

### Examples
- **DbContext Design**: See [StudentDbContext.cs](examples/StudentDbContext.cs) for IEntityTypeConfiguration pattern and DbContextFactory
- **Repository Implementation**: See [StudentRepository.cs](examples/StudentRepository.cs) for AsNoTracking, Include, pagination, and compiled queries
- **Query Optimization**: See [CourseQueries.cs](examples/CourseQueries.cs) for avoiding N+1, projection, and database functions
- **Value Objects**: See [ValueObjectConfiguration.cs](examples/ValueObjectConfiguration.cs) for owned entities and complex types
- **Concurrency Control**: See [ConcurrencyHandling.cs](examples/ConcurrencyHandling.cs) for RowVersion and conflict resolution

### References
- **Common Mistakes**: See [anti-patterns.md](references/anti-patterns.md) for what NOT to do with before/after examples
- **Performance Review**: See [performance-checklist.md](references/performance-checklist.md) for systematic optimization checklist
- **Testing Guide**: See [testing-strategies.md](references/testing-strategies.md) for unit and integration testing patterns
- **Migration Workflow**: See [migration-guide.md](references/migration-guide.md) for creating and deploying migrations

## Data Context Design

- Keep DbContext classes focused and cohesive
- Use constructor injection for configuration options
- Override OnModelCreating for fluent API configuration
- Separate entity configurations using IEntityTypeConfiguration
- Consider using DbContextFactory pattern for console apps or tests

## Entity Design

- Use the `WithGeneratedSurrogateKey<Guid>` as the default for all entities.
- Implement proper relationships (one-to-one, one-to-many, many-to-many)
- Use fluent API for constraints and validations
- Implement appropriate navigational properties
- Consider using owned entity or complex property types for value objects

## Performance

- Use `AsNoTracking()` for read-only queries
- Implement pagination for large result sets with Skip() and Take()
- Use `Include()` to eager load related entities when needed
- Consider projection (`Select()`) to retrieve only required fields
- Use compiled queries for frequently executed queries
- Avoid N+1 query problems by properly including related data
- When using the `ThenInclude()` method, consider also specifying the `AsSplitQuery()` method

## Migrations

- Always use the `dotnet ef migrations add {MIGRATION_NAME} -c {DB_CONTEXT_NAME} -o {MIGRATIONS_DIRECTORY}` to generate migrations
  - Never generate a migration yourself, always use the command
- Name migrations descriptively using the `{MIGRATION_NAME}` command argument 
- Verify migration SQL scripts before applying to production
- Consider using migration bundles for deployment
- Add data seeding through migrations when appropriate

## Querying

- Use IQueryable judiciously and understand when queries execute
- Prefer strongly-typed LINQ queries over raw SQL
- Use appropriate query operators (Where, OrderBy, GroupBy)
- Consider database functions for complex operations
- Implement specifications pattern for reusable queries

## Change Tracking & Saving

- Use appropriate change tracking strategies
- Batch your SaveChangesAsync() calls
- Implement concurrency control for multi-user scenarios
- Consider using transactions for multiple operations
- Always use the pooled dbContext available to you using the factory pattern

## Security

- Avoid SQL injection by using parameterized queries
- Implement appropriate data access permissions
- Be careful with raw SQL queries
- Consider data encryption for sensitive information
- Use migrations to manage database user permissions

## Testing

- Use in-memory database provider for unit tests
- Test migrations in isolated environments
- Consider snapshot testing for model changes

When reviewing my EF Core code, identify issues and suggest improvements that follow these best practices.

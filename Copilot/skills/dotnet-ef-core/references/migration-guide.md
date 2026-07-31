# Entity Framework Core Migration Guide

Comprehensive guide for creating, managing, and deploying EF Core migrations.

---

## Migration Workflow Overview

1. **Modify your entities or DbContext configuration**
2. **Create a migration** using `dotnet ef migrations add`
3. **Review the generated migration** code
4. **Apply migration** to development database
5. **Test the migration** thoroughly
6. **Deploy to production** using migration bundle or scripts

---

## Creating Migrations

### Basic Migration Command

```bash
dotnet ef migrations add InitialCreate
```

### Complete Migration Command (Recommended)

```bash
dotnet ef migrations add AddStudentEmailIndex -c StudentDbContext -o Data/Migrations
```

**Parameters:**
- `AddStudentEmailIndex` - Migration name (descriptive, PascalCase)
- `-c StudentDbContext` - Specify DbContext when multiple exist
- `-o Data/Migrations` - Output directory for migrations

### Migration Naming Conventions

Use descriptive names that explain what changed:

**Good Names:**
- `AddStudentEmailIndex`
- `CreateCoursesTable`
- `AddStudentEnrollmentDateColumn`
- `RemoveObsoleteCourseDescriptionField`
- `UpdateStudentEmailMaxLength`
- `AddStudentCourseRelationship`

**Bad Names:**
- `Migration1` (non-descriptive)
- `Update` (too vague)
- `Fix` (what was fixed?)
- `Changes` (what changes?)

### Creating Migrations for Multiple DbContexts

```bash
# Specify context explicitly
dotnet ef migrations add AddAuditLog -c AuditDbContext
dotnet ef migrations add AddStudent -c StudentDbContext
```

---

## 🔍 Reviewing Generated Migrations

### Migration File Structure

```csharp
public partial class AddStudentEmailIndex : Migration
{
    // Applied when migrating up (forward)
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateIndex(
            name: "IX_Students_Email",
            table: "Students",
            column: "Email",
            unique: true);
    }

    // Applied when rolling back (down)
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropIndex(
            name: "IX_Students_Email",
            table: "Students");
    }
}
```

### Always Review These:

- [ ] **Index names** - Ensure they're meaningful
- [ ] **Data loss warnings** - Dropping columns, changing types
- [ ] **Default values** - Verify they're correct
- [ ] **Nullable changes** - Ensure existing data compatibility
- [ ] **Foreign keys** - Check cascade behavior
- [ ] **Down migration** - Verify rollback will work

### Common Issues to Fix

#### Issue 1: Missing Down Migration Logic
```csharp
// Bad - Down migration is incomplete
protected override void Down(MigrationBuilder migrationBuilder)
{
    // Empty - can't roll back!
}

// Good - Complete Down migration
protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropIndex(
        name: "IX_Students_Email",
        table: "Students");
}
```

#### Issue 2: Non-Nullable Column Without Default
```csharp
// Bad - Will fail if Students table has existing rows
migrationBuilder.AddColumn<string>(
    name: "Email",
    table: "Students",
    nullable: false);

// Good - Provide default value or make nullable initially
migrationBuilder.AddColumn<string>(
    name: "Email",
    table: "Students",
    nullable: false,
    defaultValue: "noemail@example.com");
```

---

## 🚀 Applying Migrations

### Development Environment

```bash
# Apply all pending migrations
dotnet ef database update

# Apply to specific migration
dotnet ef database update AddStudentEmailIndex

# Rollback all migrations
dotnet ef database update 0

# Rollback to specific migration
dotnet ef database update PreviousMigrationName
```

### Check Migration Status

```bash
# List all migrations and their status
dotnet ef migrations list
```

Output example:
```
20240715120000_InitialCreate (Applied)
20240716130000_AddStudentEmailIndex (Applied)
20240717140000_AddCoursesTable (Pending)
```

### Apply Migrations at Runtime (Development Only)

```csharp
// Program.cs - Development only
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<StudentDbContext>();
    await context.Database.MigrateAsync(); // Apply pending migrations
}
```

**⚠️ Warning:** Do NOT use `context.Database.MigrateAsync()` in production! Use migration bundles instead.

---

## 🏭 Production Deployment

### Option 1: Migration Bundles (Recommended)

Generate a self-contained executable that applies migrations:

```bash
# Generate migration bundle
dotnet ef migrations bundle -o migrations.exe

# On production server, run the bundle
./migrations.exe --connection "Server=prod;Database=StudentDb;..."
```

**Advantages:**
- Self-contained executable
- No EF tools needed on production server
- Can be part of CI/CD pipeline
- Supports rollback

### Option 2: SQL Scripts

Generate SQL scripts for review and manual execution:

```bash
# Generate SQL for all migrations
dotnet ef migrations script -o migrations.sql

# Generate SQL for specific range
dotnet ef migrations script FromMigration ToMigration -o partial.sql

# Idempotent script (safe to run multiple times)
dotnet ef migrations script --idempotent -o migrations.sql
```

**Advantages:**
- DBA can review SQL before execution
- Can be integrated into existing database deployment process
- Clear audit trail

**Example generated SQL:**
```sql
IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20240715120000_InitialCreate')
BEGIN
    CREATE TABLE [Students] (
        [Id] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(100) NOT NULL,
        [LastName] nvarchar(100) NOT NULL,
        CONSTRAINT [PK_Students] PRIMARY KEY ([Id])
    );
    
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240715120000_InitialCreate', N'8.0.0');
END;
GO
```

### Option 3: CI/CD Pipeline Integration

```yaml
# Example GitHub Actions workflow
- name: Generate Migration Bundle
  run: dotnet ef migrations bundle -o ${{ github.workspace }}/migrations

- name: Deploy Migration Bundle
  run: |
    ./migrations --connection "${{ secrets.PROD_CONNECTION_STRING }}"
```

---

## 🌱 Data Seeding in Migrations

### Option 1: HasData (Model Seeding)

```csharp
public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);
        
        // Seed data
        builder.HasData(
            new Student
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                EnrollmentDate = new DateTime(2024, 1, 1)
            },
            new Student
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                EnrollmentDate = new DateTime(2024, 1, 1)
            }
        );
    }
}
```

When you create a migration, EF Core generates INSERT statements:

```csharp
migrationBuilder.InsertData(
    table: "Students",
    columns: new[] { "Id", "FirstName", "LastName", "Email" },
    values: new object[] { Guid.Parse("11111111..."), "John", "Doe", "john.doe@example.com" });
```

### Option 2: Custom Migration Data

Add data seeding directly in migration:

```csharp
public partial class SeedInitialData : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.InsertData(
            table: "Courses",
            columns: new[] { "Id", "Code", "Title", "Credits" },
            values: new object[,]
            {
                { Guid.NewGuid(), "CS101", "Introduction to Computer Science", 3 },
                { Guid.NewGuid(), "CS102", "Data Structures", 4 },
                { Guid.NewGuid(), "CS201", "Algorithms", 4 }
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DeleteData(
            table: "Courses",
            keyColumn: "Code",
            keyValues: new object[] { "CS101", "CS102", "CS201" });
    }
}
```

### Option 3: SQL-Based Seeding

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.Sql(@"
        INSERT INTO Courses (Id, Code, Title, Credits)
        VALUES 
            (NEWID(), 'CS101', 'Introduction to Computer Science', 3),
            (NEWID(), 'CS102', 'Data Structures', 4),
            (NEWID(), 'CS201', 'Algorithms', 4)
    ");
}
```

---

## 🛠️ Migration Management

### Remove Last Migration (Not Applied)

```bash
# Remove the last migration (if not yet applied to database)
dotnet ef migrations remove
```

⚠️ **Only works if migration hasn't been applied to any database!**

### Remove Last Migration (Already Applied)

```bash
# 1. Rollback database to previous migration
dotnet ef database update PreviousMigrationName

# 2. Remove the migration
dotnet ef migrations remove
```

### Squashing Migrations

For very old projects with many migrations:

```bash
# 1. Generate SQL for all migrations
dotnet ef migrations script -o all-migrations.sql

# 2. Remove all migration files

# 3. Delete database

# 4. Create fresh initial migration
dotnet ef migrations add InitialCreate

# 5. Modify Up() to include manual SQL if needed
```

⚠️ **Only do this if all environments are in sync!**

---

## 🔒 Best Practices

### Do's

1. **Always generate migrations, never write them manually**
   ```bash
   dotnet ef migrations add DescriptiveName
   ```

2. **Use descriptive migration names**
   - `AddStudentEmailIndex`    - `Migration1` 
3. **Review generated migrations before applying**
   - Check for data loss warnings
   - Verify indexes are named correctly
   - Ensure Down migration can rollback

4. **Test migrations on copy of production data**
   - Clone production database
   - Apply migration
   - Verify data integrity

5. **Use migration bundles for production**
   ```bash
   dotnet ef migrations bundle
   ```

6. **Keep migrations in source control**
   - Commit migration files with code changes
   - Tag releases with migration names

7. **Separate migrations by DbContext**
   ```bash
   dotnet ef migrations add Name -c SpecificDbContext
   ```

8. **Backup database before production migration**
   - Always have rollback plan

### Don'ts

1. **Don't manually edit the database schema** - Always use migrations

2. **Don't modify migrations after they've been applied** - Create a new migration

3. **Don't use `EnsureCreated()` with migrations** - Choose one approach

4. **Don't apply migrations automatically in production** - Use bundles/scripts

5. **Don't skip migrations** - Apply in order

6. **Don't commit generated SQL scripts** - Generate fresh for each deployment

7. **Don't delete old migrations** - Unless you're squashing (advanced)

---

## 🐛 Troubleshooting

### Problem: "Migration has already been applied"

```bash
# Check migration history
dotnet ef migrations list

# If needed, manually remove from __EFMigrationsHistory table
DELETE FROM __EFMigrationsHistory WHERE MigrationId = 'ProblematicMigration';
```

### Problem: "Column already exists"

The migration tried to add a column that already exists.

```bash
# Option 1: Remove the migration and create a new one
dotnet ef database update PreviousMigration
dotnet ef migrations remove

# Option 2: Manually edit migration to check if column exists
migrationBuilder.Sql(@"
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE Name = 'Email' AND Object_ID = Object_ID('Students'))
    BEGIN
        ALTER TABLE Students ADD Email NVARCHAR(255) NOT NULL;
    END
");
```

### Problem: "Build failed" when running migrations

```bash
# Build project first
dotnet build

# Then run migration
dotnet ef migrations add MigrationName
```

---

## 📋 Quick Reference

### Essential Commands

```bash
# Create migration
dotnet ef migrations add <Name> -c <DbContext> -o <OutputDir>

# Apply migrations
dotnet ef database update

# List migrations
dotnet ef migrations list

# Remove last migration
dotnet ef migrations remove

# Generate SQL script
dotnet ef migrations script -o migrations.sql

# Create migration bundle
dotnet ef migrations bundle -o migrations.exe

# Rollback to specific migration
dotnet ef database update <MigrationName>

# Rollback all migrations
dotnet ef database update 0
```

### Migration Lifecycle

1. **Develop** → Modify entities/DbContext
2. **Generate** → `dotnet ef migrations add`
3. **Review** → Check generated code
4. **Test** → Apply to dev database
5. **Commit** → Add to source control
6. **Deploy** → Use bundle or script
7. **Verify** → Confirm production success
8. **Backup** → Keep database backup

Use this guide as your reference for all migration-related tasks in EF Core.

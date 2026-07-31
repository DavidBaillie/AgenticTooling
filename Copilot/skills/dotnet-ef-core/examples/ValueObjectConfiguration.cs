using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Education.Data.ValueObjects;

/// <summary>
/// Demonstrates owned entities and value objects in EF Core.
/// Owned entities are stored in the same table as the owner (by default).
/// Use for value objects that have no identity and belong to an entity.
/// Examples: Address, Money, DateRange, ContactInfo
/// </summary>

// ============================================================================
// ENTITY WITH VALUE OBJECTS
// ============================================================================

public class Student
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Owned entity - no separate table, columns added to Students table
    public Address HomeAddress { get; set; } = new();

    // Owned entity - stored separately when configured
    public ContactInfo ContactInfo { get; set; } = new();

    // Collection of owned entities
    public List<Address> PreviousAddresses { get; set; } = new();
}

public class Course
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;

    // Owned entity representing price
    public Money Price { get; set; } = new();

    // Owned entity representing schedule
    public Schedule Schedule { get; set; } = new();
}

// ============================================================================
// VALUE OBJECTS (using records for automatic value equality)
// ============================================================================

/// <summary>
/// Address value object - no identity, equality based on all properties.
/// Records provide automatic value-based equality and are ideal for value objects.
/// </summary>
public record Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

/// <summary>
/// Money value object - represents monetary amount with currency.
/// </summary>
public record Money
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}

/// <summary>
/// ContactInfo value object - multiple contact methods.
/// </summary>
public record ContactInfo
{
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
}

/// <summary>
/// Schedule value object - represents course schedule.
/// </summary>
public record Schedule
{
    public DayOfWeek StartDay { get; set; }
    public DayOfWeek EndDay { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}

// ============================================================================
// OWNED ENTITY CONFIGURATION
// ============================================================================

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(s => s.LastName).IsRequired().HasMaxLength(100);

        // Configure HomeAddress as owned entity (inline in same table)
        builder.OwnsOne(s => s.HomeAddress, address =>
        {
            // Properties are stored as columns in Students table
            address.Property(a => a.Street)
                .HasColumnName("HomeStreet")
                .HasMaxLength(200);

            address.Property(a => a.City)
                .HasColumnName("HomeCity")
                .HasMaxLength(100);

            address.Property(a => a.State)
                .HasColumnName("HomeState")
                .HasMaxLength(50);

            address.Property(a => a.ZipCode)
                .HasColumnName("HomeZipCode")
                .HasMaxLength(20);

            address.Property(a => a.Country)
                .HasColumnName("HomeCountry")
                .HasMaxLength(100);
        });

        // Configure ContactInfo as owned entity in separate table
        builder.OwnsOne(s => s.ContactInfo, contact =>
        {
            // Store in separate table
            contact.ToTable("StudentContactInfo");

            contact.Property(c => c.Email)
                .IsRequired()
                .HasMaxLength(255);

            contact.Property(c => c.Phone)
                .IsRequired()
                .HasMaxLength(20);

            contact.Property(c => c.AlternatePhone)
                .HasMaxLength(20);
        });

        // Configure collection of owned entities (PreviousAddresses)
        builder.OwnsMany(s => s.PreviousAddresses, address =>
        {
            // Stored in separate table with foreign key back to Student
            address.ToTable("StudentPreviousAddresses");

            address.WithOwner().HasForeignKey("StudentId");

            // Owned entities in collections need a key
            address.HasKey("Id", "StudentId");

            address.Property<int>("Id").ValueGeneratedOnAdd();

            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.State).HasMaxLength(50);
            address.Property(a => a.ZipCode).HasMaxLength(20);
            address.Property(a => a.Country).HasMaxLength(100);
        });
    }
}

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Code).IsRequired().HasMaxLength(20);
        builder.Property(c => c.Title).IsRequired().HasMaxLength(200);

        // Configure Money value object
        builder.OwnsOne(c => c.Price, money =>
        {
            money.Property(m => m.Amount)
                .HasColumnName("PriceAmount")
                .HasPrecision(18, 2); // For decimal precision

            money.Property(m => m.Currency)
                .HasColumnName("PriceCurrency")
                .HasMaxLength(3)
                .HasDefaultValue("USD");
        });

        // Configure Schedule value object
        builder.OwnsOne(c => c.Schedule, schedule =>
        {
            schedule.Property(s => s.StartDay)
                .HasColumnName("ScheduleStartDay");

            schedule.Property(s => s.EndDay)
                .HasColumnName("ScheduleEndDay");

            schedule.Property(s => s.StartTime)
                .HasColumnName("ScheduleStartTime");

            schedule.Property(s => s.EndTime)
                .HasColumnName("ScheduleEndTime");
        });
    }
}

// ============================================================================
// COMPLEX TYPE (EF Core 8+)
// ============================================================================

/// <summary>
/// EF Core 8+ introduces ComplexProperty as an alternative to owned entities.
/// Complex types are value objects that are always stored inline (no separate table).
/// They cannot be null and have no identity.
/// </summary>
public class StudentWithComplexType
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    // Complex property (EF Core 8+)
    public AddressComplexType Address { get; set; } = new();
}

public record AddressComplexType
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
}

public class StudentWithComplexTypeConfiguration : IEntityTypeConfiguration<StudentWithComplexType>
{
    public void Configure(EntityTypeBuilder<StudentWithComplexType> builder)
    {
        builder.HasKey(s => s.Id);

        // EF Core 8+ ComplexProperty configuration
        builder.ComplexProperty(s => s.Address, address =>
        {
            address.Property(a => a.Street).HasMaxLength(200);
            address.Property(a => a.City).HasMaxLength(100);
            address.Property(a => a.ZipCode).HasMaxLength(20);
        });
    }
}

// ============================================================================
// USAGE EXAMPLES
// ============================================================================

public class ValueObjectExamples(StudentDbContext context)
{
    private readonly StudentDbContext _context = context;

    /// <summary>
    /// Creating entity with value objects.
    /// </summary>
    public async Task CreateStudentWithAddressAsync()
    {
        var student = new Student
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            HomeAddress = new Address
            {
                Street = "123 Main St",
                City = "Springfield",
                State = "IL",
                ZipCode = "62701",
                Country = "USA"
            },
            ContactInfo = new ContactInfo
            {
                Email = "john.doe@example.com",
                Phone = "555-0100"
            }
        };

        student.PreviousAddresses.Add(new Address
        {
            Street = "456 Oak Ave",
            City = "Chicago",
            State = "IL",
            ZipCode = "60601",
            Country = "USA"
        });

        _context.Students.Add(student);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Querying by value object properties.
    /// </summary>
    public async Task<List<Student>> FindStudentsByCity(string city)
    {
        return await _context.Students
            .Where(s => s.HomeAddress.City == city)
            .ToListAsync();
    }

    /// <summary>
    /// Updating value objects.
    /// </summary>
    public async Task UpdateStudentAddressAsync(Guid studentId, Address newAddress)
    {
        var student = await _context.Students.FindAsync(studentId);
        if (student != null)
        {
            // Replace entire value object
            student.HomeAddress = newAddress;
            await _context.SaveChangesAsync();
        }
    }
}

public class StudentDbContext(DbContextOptions<StudentDbContext> options) : DbContext(options)
{
    public DbSet<Student> Students => Set<Student>();
    public DbSet<Course> Courses => Set<Course>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new StudentConfiguration());
        modelBuilder.ApplyConfiguration(new CourseConfiguration());
    }
}

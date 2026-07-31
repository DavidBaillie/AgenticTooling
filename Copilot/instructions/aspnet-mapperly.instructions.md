---
applyTo: '**/Mapperly/*.cs, **/*Mapper.cs'
---

# Mappings Conventions

All model-to-model mapping in this repository **must** use the [Mapperly](https://mapperly.riok.app/) source-generation package. Do not hand-roll mapping logic inline or use other mapping libraries (e.g. AutoMapper).

- Mapperly Documentation: https://mapperly.riok.app/
- Mapperly Repository: https://github.com/riok/mapperly

## Rules

- **Always use Mapperly.** Any code that copies values from one model to another must go through a Mapperly mapper class.
- **Mappers must always be `static` classes** decorated with `[Mapper]`.
- **Auto-generated mapping methods must be `static partial` methods.** Mapperly fills in the implementation at compile time.
- **Manual mapping methods must be marked `[UserMapping(Default = false)]`.** This tells Mapperly the method is user-provided and should not be overwritten by source generation.
- **Use `[UseStaticMapper]` at the class level** to compose multiple mapper classes when a model contains nested types that have their own mapper.
- **Use `[MapperIgnoreSource]` / `[MapperIgnoreTarget]`** to explicitly suppress properties that must not be mapped (e.g. navigation properties, computed columns, audit fields).
- **Use `[MapProperty]`** to specify a source-to-target property name override when the names differ.
- **Use `[MapValue]`** to hard-code a constant value onto a target property.
- **Use `[MapPropertyFromSource]`** together with `Use = nameof(...)` to delegate a single target property to a named `[UserMapping]` method.
- **Always** append the "Mapper" postfix to all static mapping classes

## When to Use Partial (Auto-Generated) vs Manual Mappings

| Scenario | Approach |
|---|---|
| All (or most) properties have identical names and types | Declare a `static partial` method and let Mapperly generate it |
| A single property needs custom logic (e.g. formatting, conditional value) | Keep the method `partial` for the rest, add a `[UserMapping(Default = false)]` helper and reference it with `[MapPropertyFromSource(..., Use = nameof(...))]` |
| The entire mapping requires complex or bespoke logic | Write the method body manually and mark it `[UserMapping(Default = false)]` |

## Examples

### Simple auto-generated mapping

When source and target share property names, declare the method as `partial` and Mapperly generates the body:

```csharp
[Mapper]
internal static partial class AdvancedNotificationInformationMapper
{
    public static partial AdvancedNotificationInformationEntity ToEntity(this AdvancedNotificationInformationDto dto);

    public static partial AdvancedNotificationInformationDto ToDto(this AdvancedNotificationInformationEntity entity);
}
```

### Auto-generated mapping with property overrides and ignored members

Use attributes to control which properties are mapped and how:

```csharp
[Mapper]
[UseStaticMapper(typeof(AddressMapper))]
[UseStaticMapper(typeof(GuardianMapper))]
[UseStaticMapper(typeof(EmergencyContactMapper))]
[UseStaticMapper(typeof(EnrollmentMapper))]
internal static partial class StudentMapper
{
    [MapProperty(nameof(dto.Grade), nameof(StudentEntity.GradeLevel))]
    [MapProperty(nameof(dto.ParentStudentId), nameof(StudentEntity.ParentStudentId))]
    [MapperIgnoreTarget(nameof(StudentEntity.ParentStudent))]
    [MapperIgnoreTarget(nameof(StudentEntity.EnrollmentRecord))]
    [MapperIgnoreTarget(nameof(StudentEntity.IsDeleted))]
    [MapValue(nameof(StudentEntity.IsDeleted), false)]
    public static partial StudentEntity ToEntity(this StudentDto dto);

    [MapProperty(nameof(entity.GradeLevel), nameof(StudentDto.Grade))]
    [MapperIgnoreSource(nameof(entity.ParentStudent))]
    [MapperIgnoreSource(nameof(entity.IsDeleted))]
    public static partial StudentDto ToDto(this StudentEntity entity);
}
```

### Manual helper for a single property

When one property requires custom logic, delegate it via `[MapPropertyFromSource]` and a `[UserMapping(Default = false)]` method. The rest of the mapping remains auto-generated:

```csharp
[Mapper]
internal static partial class UndeliverableAddressMapper
{
    [MapperIgnoreTarget(nameof(EWSv2.Address.Department))]
    [MapPropertyFromSource(nameof(EWSv2.Address.StreetAddress2), Use = nameof(MapAddress2))]
    [MapPropertyFromSource(nameof(EWSv2.Address.PhoneNumber), Use = nameof(MapPhoneNumber))]
    public static partial EWSv2.Address ToEwsAddress(this UndeliverableAddressDto dto);

    [UserMapping(Default = false)]
    private static string MapAddress2(this UndeliverableAddressDto dto)
        => string.IsNullOrWhiteSpace(dto.BuzzerCode)
            ? null!
            : $"BUZZ: {dto.BuzzerCode}";

    [UserMapping(Default = false)]
    private static EWSv2.PhoneNumber MapPhoneNumber(this UndeliverableAddressDto dto) => new()
    {
        AreaCode = dto.PhoneAreaCode,
        CountryCode = dto.PhoneCountryCode,
        Extension = dto.PhoneExtension,
        Phone = dto.PhoneNumber
    };
}
```

## Checklist

When adding a new mapper, verify:

- [ ] The class is `static` and decorated with `[Mapper]`.
- [ ] Auto-generated methods are `static partial`.
- [ ] Any manually implemented method is marked `[UserMapping(Default = false)]`.
- [ ] Navigation properties, audit fields, and any members that must not be copied are explicitly ignored with `[MapperIgnoreSource]` or `[MapperIgnoreTarget]`.
- [ ] Nested model types that have their own mapper are wired in with `[UseStaticMapper]` at the class level rather than duplicating mapping logic.
- [ ] No hand-rolled property assignments exist outside of `[UserMapping]` helpers.

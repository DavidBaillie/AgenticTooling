---
applyTo: '**/*.cs'
---

# C# Documentation Best Practices

Document all public members with XML comments. Document complex internal members as well.

## Common Tags

- `<summary>` - Brief description starting with present-tense, third-person verb
- `<remarks>` - Additional details, usage notes, implementation context. Use `<para>` for paragraphs
- `<see langword>` - Keywords like `null`, `true`, `false`, `int`, `bool`
- `<c>` - Inline code in sentences
- `<see cref>` - Reference types/members inline
- `<seealso>` - Standalone references for "See also" section
- `<example>` with `<code language="csharp">` - Usage examples
- `<list type="bullet|number|table">` - Lists in documentation
- `<inheritdoc/>` - Inherit from base/interface (or `<inheritdoc cref="Member"/>` for specific member)

## Types

**Classes/Structs:** Start with "Represents..." (data/entities) or "Provides..." (services/utilities). Document thread safety, alternatives, limitations, performance in `<remarks>`.

**Interfaces:** Start with "Defines..." or "Provides...". Document implementer requirements and typical scenarios.

**Enums:** Start with "Specifies...". Document each member. For `[Flags]`, document valid combinations.

**Example:**
```csharp
/// <summary>Represents a student enrolled in a classroom.</summary>
/// <remarks>
/// <para>This class is immutable and thread-safe.</para>
/// <para>Use <see cref="StudentBuilder"/> for optional properties.</para>
/// </remarks>
public class Student { }

/// <summary>Specifies the enrollment status of a student.</summary>
public enum EnrollmentStatus
{
    /// <summary>The student is not currently enrolled.</summary>
    NotEnrolled = 0,
    /// <summary>The student is actively enrolled and attending classes.</summary>
    Active = 1
}
```

## Events, Fields, Delegates

**Events:** Describe when raised, event data, threading context, sync vs async invocation.

**Constants/Fields:** What the value represents, when to use, thread safety.

**Delegates:** Start with "Represents a method that...". Document params/returns.

## Parameters (`<param>`)

Description is a noun phrase with introductory article, not repeating the type:
- **Enum (non-flags):** "One of the enumeration values that specifies..."
- **Enum (flags):** "A bitwise combination of the enumeration values that specifies..."
- **Boolean:** "`<see langword="true"/>` to ...; otherwise, `<see langword="false"/>`."
- **out:** "When this method returns, contains .... This parameter is treated as uninitialized."
- **ref:** "A reference to .... When this method returns, contains ..."
- **in:** Emphasize read-only, passed by reference for performance
- **Nullable:** Document if `<see langword="null"/>` is valid and its meaning
- **Collections:** What elements expected, if empty is valid
- **Optional with defaults:** Document default value and meaning

Use `<paramref>` to reference parameters in text. Use `<typeparam>` for generics, documenting constraints and variance.

## Returns (`<returns>`)

Noun phrase with introductory article, not repeating type:
- **Boolean:** "`<see langword="true"/>` if ...; otherwise, `<see langword="false"/>`."
- **Nullable:** Document when `<see langword="null"/>` is returned
- **Collections:** Specify if empty or `<see langword="null"/>` possible
- **Task/ValueTask:** Describe result value, not the Task wrapper
- **Task<T>/ValueTask<T>:** Describe T, not the wrapper

## Async Methods

Standard `CancellationToken` param: "A <see cref="CancellationToken"/> to observe while waiting for the task to complete."

Standard cancellation exception: `<exception cref="OperationCanceledException">The operation was canceled.</exception>`

## Constructors, Indexers, Operators

**Constructors:** "Initializes a new instance of the `ClassName` class." (or struct). Document params, validation, exceptions.

**Indexers:** Describe access provided. Use `<value>` for stored/retrieved value. Document index exceptions.

**Operators:** Describe operation. Document params and result.

## Properties (`<value>`)

Noun phrase not repeating type. Add default if applicable. Boolean: "`<see langword="true"/>` if ...; otherwise, `<see langword="false"/>`. The default is ..."

Document: nullable meaning, collection empty/null possibility, exceptions, read-only source (construction vs computed), validation rules.

**Example:**
```csharp
/// <summary>Gets or sets the student's email address.</summary>
/// <value>The email address, or <see langword="null"/> if not provided.</value>
/// <exception cref="ArgumentException">The value is not a valid email format.</exception>
public string? Email { get; set; }

/// <summary>Gets the courses the student is enrolled in.</summary>
/// <value>A read-only collection. Never <see langword="null"/>, may be empty.</value>
public IReadOnlyList<Course> Courses { get; }
```

## Exceptions (`<exception cref>`)

Document exceptions thrown directly (and key nested exceptions users will encounter). Describe the condition directly without "Thrown if/when" or "If".

**Common patterns:**
- `ArgumentNullException`: "`<paramref name=\"paramName\"/>` is `<see langword=\"null\"/>`."
- `ArgumentException`: "`<paramref name=\"paramName\"/>` is empty or contains only white space."
- `ArgumentOutOfRangeException`: "`<paramref name=\"paramName\"/>` is less than zero."
- `InvalidOperationException`: "The operation is not valid due to the current state of the object."
- `ObjectDisposedException`: "The object has been disposed."

## Key Principles

1. **Be consistent** in patterns throughout codebase
2. **Be concise** - brief summaries
3. **Be complete** - all public + important internal members
4. **Be accurate** - match actual behavior
5. **Update regularly** - sync with code changes
6. **Use examples** for complex APIs
7. **Think audience** - write for API consumers
8. **Reference related members** with `<see cref>` and `<seealso>`
9. **Document thread safety** explicitly
10. **Avoid redundancy** - don't repeat obvious information

**Poor:** `/// <summary>This method gets the student.</summary>`

**Good:** `/// <summary>Retrieves the currently authenticated student from the session.</summary>`

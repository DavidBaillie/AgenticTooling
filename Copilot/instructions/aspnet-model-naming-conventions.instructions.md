---
applyTo: '**/Models/*.cs, **/Requests/*.cs, **/Responses/*.cs, **/*Dto.cs, **/*Request.cs, **/*Entity.cs, **/*Command.cs, **/*Consumer.cs'
---

## Quick Reference

| Pattern | Example | Purpose | Layer |
|---------|---------|---------|-------|
| `*Entity` | `StudentEntity` | Entity Framework database model | Infrastructure |
| `Create*Request` | `CreateStudentRequest` | HTTP request body for resource creation | Domain |
| `Update*Request` | `UpdateStudentRequest` | HTTP request body for resource update | Domain |
| `*Dto` | `StudentDto` | Transformed domain data returned from controllers | Domain |
| `*QueryEnvelope` | `StudentQueryEnvelope` | Paginated GET request parameters | Domain |
| `*EnvelopeDto` | `StudentEnvelopeDto` | Paginated GET response body | Domain |
| `*Command` | `CreateStudentCommand` | Message published to a queue | Infrastructure |
| `*Consumer` | `CreateStudentConsumer` | Processes a queue `*Command` message | Infrastructure |
| `*Base` | `StudentBase` | Abstract base class (exempt from all other rules) | Any |

---

## Entity Models (`*Entity`)

Models used directly by Entity Framework to represent database tables or owned types must be postfixed with `Entity`.

**Examples:**
```csharp
// Infrastructure layer
public class StudentEntity { ... }
public class ClassroomEntity { ... }
```

---

## Request Models (`Create*Request` / `Update*Request`)

Models used to receive data from the body of an HTTP request must be postfixed with `Request`.

- **Prefix rules:**
  - Models for **creating** a new resource are prefixed with `Create`
  - Models for **updating** an existing resource are prefixed with `Update`

**Examples:**
```csharp
// Domain layer
public class CreateStudentRequest { ... }
public class UpdateStudentRequest { ... }

public class CreateClassroomRequest { ... }
public class UpdateClassroomRequest { ... }
```

---

## Data Transfer Object Models (`*Dto`)

Models used to transform Entity Framework entities (or domain models) into a form suitable for the end user are postfixed with `Dto`.

- **Usage:** These are the **default return type** for all controller actions. Controllers must not return raw entities or infrastructure models.
- **Nested children:** Any child object nested inside a `*Dto` must also use the `Dto` postfix. Do not mix `*Dto`, `*Request`, or `*Response` types within the same object graph.

**Examples:**
```csharp
public class StudentDto
{
    public AddressDto HomeAddress { get; set; }       // ✓ child uses Dto postfix
    public ICollection<ClassroomDto> Classrooms { get; set; }  // ✓ collection children use Dto postfix
}

public class ClassroomDto { ... }
```

---

## Paginated Request Models (`*QueryEnvelope`)

When a GET endpoint supports pagination, the model representing the incoming query parameters (page number, page size, filters, etc.) must be postfixed with `QueryEnvelope`.

**Examples:**
```csharp
public class StudentQueryEnvelope { ... }
public class ClassroomQueryEnvelope { ... }
```

---

## Paginated Response Models (`*EnvelopeDto`)

When a GET endpoint returns a paginated result, the model wrapping the paged data (items, total count, page metadata, etc.) must be postfixed with `EnvelopeDto`.

**Examples:**
```csharp
public class StudentEnvelopeDto { ... }
public class ClassroomEnvelopeDto { ... }
```

---

## Queue Models (`*Command` / `*Consumer`)

Models and classes used for queue-based messaging follow two conventions:

- **`*Command`** — The message published into the queue. Represents the intent to perform an action.
- **`*Consumer`** — The class that reads and processes a `*Command` from the queue.

**Examples:**
```csharp
public class CreateStudentCommand { ... }      // message sent to queue
public class CreateStudentConsumer { ... }     // processes CreateStudentCommand

public class UpdateClassroomCommand { ... }
public class UpdateClassroomConsumer { ... }
```

---

## Configuration Options (`*Options` / `IOptions<T>`)

Classes in `**/Options/**` folders are used exclusively to bind configuration data from `IConfiguration` at runtime startup. These classes are not subject to the Dto, Request, or Entity naming rules. They should be named after the configuration section they represent.

**Examples:**
```csharp
public class SchoolApiOptions { ... }   // binds to "SchoolApi" config section
public class EnrollmentClientOptions { ... }
```

---

## Abstract Base Classes (`*Base`)

Abstract base classes are exempt from all other naming rules. They must be postfixed with `Base` regardless of the layer they reside in.

**Examples:**
```csharp
public abstract class StudentBase { ... }
public abstract class ClassroomBase { ... }
public abstract class ConsumerBase { ... }
```

---

## Common Pitfalls

- **Do not** return `*Entity` models from controllers — map them to a `*Dto` first.
- **Do not** use generic names like `StudentModel`, `StudentResponse`, or `StudentData` — always use the correct postfix.
- **Do not** use `*Request` as a return type — requests are inbound only.
- **Do not** place `*Entity` models in Domain or Application layers — they belong in Infrastructure.
- **Do not** place `*Dto`, `*Request`, `*QueryEnvelope`, or `*EnvelopeDto` models in Infrastructure — they belong in Domain.
- **Do not** nest a `*Request` or `*Response` type inside a `*Dto` — all nested children must also use the `Dto` postfix.
- **Do not** apply `Entity`, `Dto`, `Request`, `Command`, or `Consumer` postfixes to abstract base classes — use `Base` instead.

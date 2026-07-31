---
applyTo: '**/*.cs'
---

# Logging Conventions

## Logger Injection

Always use `ILogger<T>` where `T` is the **current class** — never a base class, interface, or unrelated type.

```csharp
public class MyService(ILogger<MyService> logger)
{
}
```

## Log Levels

| Scenario | Level |
|---|---|
| Normal flow state transitions (method entered, operation started/completed, branch taken) | `LogInformation` |
| Non-happy path (unexpected state, validation failure, missing data, retries, degraded behaviour) | `LogWarning` |
| Caught exception | `LogError` |

## Flow State Logging

Log significant flow states with `LogInformation` so execution paths are traceable in production:

```csharp
logger.LogInformation("Starting enrollment process for StudentId {StudentId}", student.Id);
// ... work ...
logger.LogInformation("Student {StudentId} enrolled successfully", student.Id);
```

## Exception Logging

Whenever an exception is caught, **always** pass the exception as the first argument to `LogError`:

```csharp
catch (Exception ex)
{
    logger.LogError(ex, "Failed to enroll student for StudentId {StudentId}", student.Id);
}
```

Never swallow exceptions silently. Never log the exception only as a formatted string — always pass it as the typed `exception` parameter so the full stack trace is captured.

## Non-Happy Path Logging

Whenever a non-happy path occurs (validation failure, unexpected result, missing entity, business rule violation, external service degradation, etc.), log **as many relevant details as possible** to aid diagnosis:

```csharp
logger.LogWarning(
    "Student lookup returned no result. StudentId {StudentId}, RequestedBy {UserId}, Timestamp {Timestamp}",
    studentId, userId, DateTimeOffset.UtcNow);
```

Include all relevant identifiers, inputs, and contextual state. Use structured logging parameters (named placeholders `{Name}`) — never string interpolation.

## Security — Never Log Secrets

The following must **never** appear in log output:

- Passwords, passphrases
- API keys, tokens, secrets, connection strings
- Private keys or certificates
- Full card numbers, CVVs, or bank account numbers
- Any field whose name or value suggests it is a credential or secret

If a non-happy path involves an authentication or authorisation failure, log the user/request identifier and outcome only — not the credential that was presented.

```csharp
// CORRECT
logger.LogWarning("Authentication failed for UserId {UserId}", userId);

// WRONG — never do this
logger.LogWarning("Authentication failed. Password attempted: {Password}", password);
```

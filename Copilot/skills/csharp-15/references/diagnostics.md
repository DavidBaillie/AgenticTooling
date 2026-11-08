# Diagnostic quick reference

| Diagnostic | Meaning in C# 15 context |
| --- | --- |
| CS8121 | A type pattern isn't compatible with any union case type (for example `pet is Pet`). |
| CS8509 | Switch not exhaustive: a missing union case, or an inaccessible closed-hierarchy subtype. |
| CS8655 | Union contents are maybe-null and there's no `null` arm. |
| CS8510 | Unreachable arm (a base arm after its subtypes, or `Car` before `Sedan`). |
| CS0121 / CS0411 | `with(...)` arguments don't pick overloads or infer type arguments. |
| CS9370 / CS9380 / CS1519 | `union`/`closed` used as an identifier. Escape it with `@`. |
| CS9363 | A member with pointer parameters (for example an indexer) called outside `unsafe` (preview). |
| CS9376 | A requires-unsafe parameterless constructor used where `new()`/`struct` is required (preview). |
| CS9392 | An explicit-layout field needs `safe`/`unsafe` (preview). |
| CS9401 / CS0227 | `safe`/`unsafe` modifiers or blocks used without `AllowUnsafeBlocks`. |
| `SwitchExpressionException` | Run time: a `default` union (null contents) reached a switch with no `null` arm. |


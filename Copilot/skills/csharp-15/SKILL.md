---
name: csharp-15
description: Write, review and modernize C# 15 (.NET 11) code correctly. Covers every C# 15 feature or when the build reports CS8121, CS8509, CS8655, CS8510, CS0121/CS0411 on a `with(...)` element, CS9370, CS9380 or CS9363.
---

# C# 15

C# 15 ships with the .NET 11 SDK and Visual Studio 2026. It is the default language version for
projects that target `net11.0`.

Read the prerequisites below first, then load only the reference for the feature you are using.
The references contain the detailed rules, examples and limitations; do not load them all upfront.
For union-versus-hierarchy decisions, read both the union and closed-hierarchy references.

## Feature references

| Feature / reference | Keyword / syntax | Needs preview? |
| --- | --- | --- |
| [Collection expression arguments](./references/collection-expression-arguments.md) | `[with(args), e1, e2]` | No |
| [Union types](./references/union-types.md) | `union U(A, B);`, `[Union]` | No |
| [Closed hierarchies](./references/closed-hierarchies.md) | `closed class` / `closed record class` | No |
| [Extension indexers](./references/extension-indexers.md) | `this[...]` inside `extension(T x) { }` | No |
| [Labeled break / continue](./references/labeled-jumps.md) | `outer: for (...) { ... break outer; }` | No |
| [Non-virtual static interface members on older runtimes](./references/static-interface-members.md) | `static` helpers in interfaces on .NET Framework / netstandard | No |
| [Memory safety model](./references/memory-safety.md) | `unsafe(expr)`, `safe`, requires-unsafe `unsafe` members | **Yes** (`LangVersion=preview` + feature flag) |

## Before using any C# 15 feature

1. **Check the language version.** Look at the project's `TargetFramework` and `LangVersion`.
   `net11.0` without a `LangVersion` gives C# 15. To see the active version, temporarily add
   `#error version`: the compiler reports CS8304 with the compiler and language versions.
2. **Don't set `<LangVersion>latest</LangVersion>`.** Microsoft warns against it because it varies
   by machine. Using a language version newer than the target framework's default is unsupported.
   Prefer moving the TFM to `net11.0`.
3. **Union types need runtime types.** `System.Runtime.CompilerServices.UnionAttribute` and
   `System.Runtime.CompilerServices.IUnion` ship in the .NET 11 runtime. The compiler does **not**
   synthesize them. When you target an older TFM, the project must reference or define them.
4. **Know the new contextual keywords.** These can break existing identifiers, and `@` escapes them
   (details in the [compiler breaking changes](./references/compiler-breaking-changes.md)):
   - `with(` at the start of a collection-expression element: use `@with(...)` to call a method
     named `with`.
   - `union` before a type name in a declaration: use `@union`.
   - `closed` in type declarations and modifier positions: use `@closed`.
   - `safe`, in preview, as a member modifier: use `@safe`.
5. **The SDK is still prerelease** (as of the October 2026 docs). The feature specs may differ from
   the shipped compiler. The Microsoft Learn language reference describes shipped behavior, so it
   wins when they disagree.

## Agent checklist

- [ ] Confirm the project is on C# 15 (`net11.0`, or an explicit `LangVersion` of 15). Don't add
      `latest`.
- [ ] Use `[with(...), ...]` instead of construct-then-add boilerplate when a capacity, comparer or
      constructor argument matters. Never use it with arrays or spans.
- [ ] Model "one of these types" with `union`. Model "one of my subclasses" with `closed`. Switch
      on the union or the closed base, not on `.Value` or `object`.
- [ ] In exhaustive switches, prefer explicit `null` arms over `_`, and always handle `null` when
      default union values or nullable inputs can arrive.
- [ ] Seal the leaves of closed hierarchies, and treat their subtype lists as public API.
- [ ] Hand-write a union with `TryGetValue`/`HasValue` for value-type cases on hot paths.
- [ ] Give each labeled loop a unique label in the method. `continue` never targets a `switch`.
- [ ] Don't use extension indexers on arrays or strings, or inside expression trees and
      `IQueryable` queries.
- [ ] Touch the memory-safety preview only when the user has opted in. Keep `unsafe` blocks
      minimal, and document every requires-unsafe member with `/// <safety>`.

## Supporting references

- Read [compiler breaking changes](./references/compiler-breaking-changes.md) when upgrading to
  the .NET 11 SDK or diagnosing contextual-keyword and compiler compatibility issues.
- Read the [diagnostic quick reference](./references/diagnostics.md) when investigating a compiler
  error or a union-related `SwitchExpressionException`, then load the relevant feature reference.
- Read [sources](./references/sources.md) when verifying language-reference, proposal or compiler
  implementation details.

## Sources 

- What's new in C# 15: https://learn.microsoft.com/dotnet/csharp/whats-new/csharp-15
- Collection expressions (arguments): https://learn.microsoft.com/dotnet/csharp/language-reference/operators/collection-expressions#collection-expression-arguments
- Spec, collection expression arguments: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-15.0/collection-expression-arguments
- Union types: https://learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/union
- Spec, unions: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-15.0/unions
- `closed` modifier: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/closed
- Patterns (closed hierarchy and union patterns): https://learn.microsoft.com/dotnet/csharp/language-reference/operators/patterns
- Spec, closed hierarchies: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-15.0/closed-hierarchies
- Extension members (indexers): https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/extension#extension-indexers
- Spec, extension indexers: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-15.0/extension-indexers
- Jump statements: https://learn.microsoft.com/dotnet/csharp/language-reference/statements/jump-statements
- Spec, labeled break/continue: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/csharp-15.0/labeled-break-continue
- IDE0410: https://learn.microsoft.com/dotnet/fundamentals/code-analysis/style-rules/ide0410
- Interface static members and runtime support: https://learn.microsoft.com/dotnet/csharp/language-reference/keywords/interface#static-interface-members-and-runtime-support
- Unsafe code (updated memory safety model): https://learn.microsoft.com/dotnet/csharp/language-reference/unsafe-code
- Spec, memory safety: https://learn.microsoft.com/dotnet/csharp/language-reference/proposals/unsafe-evolution
- Compiler options (enabling the memory safety rules): https://learn.microsoft.com/dotnet/csharp/language-reference/compiler-options/language#enable-the-updated-memory-safety-rules
- Compiler breaking changes in .NET 11: https://learn.microsoft.com/dotnet/csharp/whats-new/breaking-changes/compiler%20breaking%20changes%20-%20dotnet%2011
- Roslyn language feature status: https://github.com/dotnet/roslyn/blob/main/docs/Language%20Feature%20Status.md
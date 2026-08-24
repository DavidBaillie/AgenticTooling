# Agentic Tooling

A curated collection of GitHub Copilot instructions and skills for enhanced AI-assisted development workflows.

## Overview

This repository contains customization files that extend GitHub Copilot's capabilities through:

- **Instructions**: Context-specific rules and conventions that apply to particular file patterns
- **Skills**: Reusable domain-specific workflows and knowledge packages
- **Prompts**: Reusable task prompts for repeatable, multi-phase work

## Structure

```
Copilot/
├── instructions/     # Instruction files (.instructions.md)
│   ├── agent-*       # Agent customization and markdown formatting
│   ├── aspnet-*      # ASP.NET Core patterns and conventions
│   ├── dotnet-*      # .NET development guidelines
│   ├── shell.*       # Shell scripting best practices
│   └── structure-*   # Code style and architecture preferences
│
├── prompts/          # Reusable prompt files (.prompt.md)
│
└── skills/           # Skill packages (SKILL.md + examples + references)
    ├── aspnet-*      # ASP.NET Core workflows
    ├── dotnet-*      # .NET patterns and practices
    ├── github-*      # GitHub Actions and hooks
    ├── postgresql-*  # PostgreSQL optimization and review
    ├── refactor-*    # Refactoring workflows
    ├── microsoft-*   # Microsoft documentation tools
    └── web-*         # Web design, layout, and static-site workflows
```

## Instructions

Instructions are applied automatically based on file patterns. They guide Copilot on:

- Code style and conventions
- Documentation standards
- Architecture patterns
- Technology-specific best practices

### Key Instruction Sets

- **ASP.NET Core**: Controller patterns, clean architecture, service workflows, logging
- **.NET**: Async patterns, enum definitions, EF Core, documentation
- **Agent Customization**: Creating instructions, skills, and agents
- **Code Structure**: Lazy Dev efficiency patterns
- **Static Websites**: Semantic HTML, responsive CSS, accessible interactions, and page metadata

## Skills

Skills provide specialized workflows and domain knowledge for specific tasks:

### Core Skills

- **aspnet-controller-service-workflow**: Controller-Service-Repository pattern
- **dotnet-discriminated-union**: OneOf result type patterns
- **dotnet-ef-core**: Entity Framework best practices
- **dotnet-nunit**: Unit and integration testing patterns
- **github-actions-ci-cd**: CI/CD pipeline design
- **github-actions-hardening**: Security hardening for workflows
- **postgresql-optimization**: PostgreSQL-specific development patterns
- **refactor-plan**: Multi-file refactoring workflows
- **microsoft-docs**: Query Microsoft Learn documentation
- **nuget-manager**: NuGet package management

### Web Skills

- **web-general-design**: Visual direction and production-quality frontend design
- **web-css-layouts**: Intrinsic responsive CSS layouts and layout validation
- **web-wcag-compliance**: WCAG 2.2 Level AA accessibility review, fixes, and validation
- **web-static-site-workflow**: Requirements, design-document approval, implementation, and handoff
- **web-static-site-audit**: Visual, content, accessibility, metadata, and deployment-readiness review

Each skill includes:
- `SKILL.md` - Main skill documentation
- `examples/` - Working code samples
- `references/` - Supporting documentation and guidelines

## Usage

### Distribution Model

`Copilot/` is the catalog and install source for this collection. It is not a live VS Code configuration directory by default.

Install the files into the location your client discovers:

| Customization | Source | VS Code workspace target | User-wide target |
| --- | --- | --- | --- |
| Prompts | `Copilot/prompts/*.prompt.md` | `.github/prompts/` | VS Code user prompt location |
| Instructions | `Copilot/instructions/*.instructions.md` | `.github/instructions/` | VS Code user instruction location |
| Skills | `Copilot/skills/<name>/SKILL.md` | `.github/skills/<name>/` | `~/.copilot/skills/<name>/` or `~/.agents/skills/<name>/` |

Copy is the supported workflow. Sync tools are acceptable if they preserve the same files exactly. Symbolic links are not the default because support varies by operating system, repository policy, and client sandboxing.

PowerShell install example from a target repository:

```powershell
$catalog = 'C:\Users\leema\source\repos\AgenticTooling\Copilot'
New-Item -ItemType Directory -Force -Path '.github\prompts', '.github\instructions', '.github\skills' | Out-Null
Copy-Item -Path "$catalog\prompts\*.prompt.md" -Destination '.github\prompts' -Force
Copy-Item -Path "$catalog\instructions\*.instructions.md" -Destination '.github\instructions' -Force
Copy-Item -Path "$catalog\skills\*" -Destination '.github\skills' -Recurse -Force
```

Verify discovery:

- VS Code: run **Chat: Open Customizations**, then check prompts, instructions, and skills.
- VS Code prompts: type `/` in chat and look for the prompt name or copied file name.
- Skills: type `/skills` or ask for a task matching the skill description.
- Instructions: open a matching file and check Copilot Chat **References** for applied instructions.

### Client Compatibility

| Client | Prompts | Instructions | Skills | Notes |
| --- | --- | --- | --- | --- |
| VS Code local agents | Supported from `.github/prompts/` | Supported from `.github/instructions/` | Supported from `.github/skills/`, `.agents/skills/`, or configured locations | Prompt frontmatter may set `description`, `argument-hint`, `agent`, `model`, and `tools`; unavailable tools are ignored. |
| Copilot CLI / Agent Host | Prompt files are not loaded directly | Follow the active runtime's instruction support | Supported skill format | Convert prompt workflows to skills when prompt-file discovery is unavailable. |
| Other skills-compatible clients | Client-specific | Client-specific | Supported when the client implements Agent Skills | Use `SKILL.md` frontmatter and relative links for portability. |

If a client ignores prompt frontmatter, use the prompt body as the source of truth. The static website prompt includes the same gates and validation requirements in the body.

The static-site security backlog remains separate: review `web-html-injection-testing` independently before changing its examples.

### With GitHub Copilot

Place this repository in your Copilot configuration directory, or reference these files in your VS Code workspace settings.

Instructions are automatically applied when editing matching files. Skills can be invoked by asking Copilot to perform related tasks.

Prompt files are kept under `Copilot/prompts/` to match this collection's layout. For VS Code's `#prompt:` picker, copy the prompt into the target repository's `.github/prompts/` folder.

### Prompts

- **static-website-generator**: Gated discovery, design-document approval, static-site implementation, owner content feedback, and final review

### Standalone

These files serve as reference documentation for development patterns and best practices, even without GitHub Copilot.

## License

See [LICENSE.txt](LICENSE.txt) for details.

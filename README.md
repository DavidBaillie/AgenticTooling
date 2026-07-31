# Agentic Tooling

A curated collection of GitHub Copilot instructions and skills for enhanced AI-assisted development workflows.

## Overview

This repository contains customization files that extend GitHub Copilot's capabilities through:

- **Instructions**: Context-specific rules and conventions that apply to particular file patterns
- **Skills**: Reusable domain-specific workflows and knowledge packages

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
└── skills/           # Skill packages (SKILL.md + examples + references)
    ├── aspnet-*      # ASP.NET Core workflows
    ├── dotnet-*      # .NET patterns and practices
    ├── github-*      # GitHub Actions and hooks
    ├── postgresql-*  # PostgreSQL optimization and review
    ├── refactor-*    # Refactoring workflows
    └── microsoft-*   # Microsoft documentation tools
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

Each skill includes:
- `SKILL.md` - Main skill documentation
- `examples/` - Working code samples
- `references/` - Supporting documentation and guidelines

## Usage

### With GitHub Copilot

Place this repository in your Copilot configuration directory, or reference these files in your VS Code workspace settings.

Instructions are automatically applied when editing matching files. Skills can be invoked by asking Copilot to perform related tasks.

### Standalone

These files serve as reference documentation for development patterns and best practices, even without GitHub Copilot.

## License

See [LICENSE.txt](LICENSE.txt) for details.

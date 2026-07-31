---
applyTo: '**/Controllers/*.cs, **/Services/*.cs'
---

# Controller Service Workflow

This codebase follows a layered architecture pattern for database-backed controller operations:

**Controller → Service Interface → Service → Repository Interface → Repository → Database**

## Quick Reference

- Controllers depend on service interfaces, not repositories
- Services perform all business validation and orchestration
- Repositories handle only database persistence and queries
- Both services and repositories default to transient lifetime registration

## When Creating or Refactoring

For detailed guidance on implementing this pattern, including:
- Complete controller, service, and repository rules
- Refactor checklist for existing code
- Validation placement guide
- Working examples

Invoke the **controller-service-workflow** skill.

---
name: github-actions-ci-cd
description: 'Comprehensive guide for building robust, secure, and efficient CI/CD pipelines using GitHub Actions. Use when creating new workflows, optimizing existing pipelines, implementing security best practices, setting up deployment strategies, or troubleshooting workflow issues. Covers workflow structure, security, caching, testing, and deployment.'
---

# GitHub Actions CI/CD Best Practices

Use this skill when designing, implementing, or optimizing GitHub Actions workflows for building, testing, and deploying applications.

## Quick Reference

- **Complete Examples**: See [examples/](examples/) directory for full workflow implementations
- **Security Guide**: See [references/security-best-practices.md](references/security-best-practices.md)
- **Performance**: See [references/optimization-performance.md](references/optimization-performance.md)
- **Testing**: See [references/testing-strategies.md](references/testing-strategies.md)
- **Deployment**: See [references/deployment-strategies.md](references/deployment-strategies.md)
- **Troubleshooting**: See [references/troubleshooting.md](references/troubleshooting.md)

## When to Use This Skill

- Creating new CI/CD workflows from scratch
- Optimizing existing pipeline performance (slow builds, inefficient caching)
- Implementing security best practices (secret management, OIDC, least privilege)
- Setting up comprehensive testing strategies (unit, integration, E2E, performance)
- Designing deployment workflows (staging, production, rollback strategies)
- Troubleshooting workflow failures or unexpected behavior
- Reviewing workflows for security, performance, or reliability issues

## Core Principles

### 1. Security First
- **Always pin actions to commit SHAs**: Use `actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v6.0.2` instead of mutable tags like `@v4` or `@main`
- **Least privilege for GITHUB_TOKEN**: Default to `contents: read`, grant write permissions only when necessary
- **Secret management**: Use GitHub Secrets exclusively, never hardcode credentials
- **OIDC over static credentials**: Prefer OpenID Connect for cloud authentication

### 2. Performance Optimization
- **Effective caching**: Use `actions/cache` with `hashFiles()` for dependencies and build outputs
- **Parallelization**: Leverage `strategy.matrix` for concurrent testing across environments
- **Shallow clones**: Use `fetch-depth: 1` unless full Git history is required
- **Artifacts for data transfer**: Pass build outputs between jobs efficiently

### 3. Comprehensive Testing
- **Unit tests**: Fast feedback on every push
- **Integration tests**: Validate component interactions with real dependencies
- **E2E tests**: Full user flow validation against staging environments
- **Performance tests**: Prevent regressions and ensure scalability

### 4. Reliable Deployments
- **Environment protection**: Use GitHub Environments with manual approvals for production
- **Rollback strategies**: Always have a tested path to revert deployments
- **Progressive delivery**: Consider blue/green, canary, or feature flags for critical applications
- **Post-deployment validation**: Automated smoke tests and health checks

## Workflow Structure

### Basic Template

```yaml
name: Build and Test

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]
  workflow_dispatch:

permissions:
  contents: read  # Least privilege by default

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA> # Always pin to SHA
        with:
          fetch-depth: 1  # Shallow clone for performance
      
      - name: Setup environment
        uses: actions/setup-node@<SHA>
        with:
          node-version: 18
      
      - name: Cache dependencies
        uses: actions/cache@<SHA>
        with:
          path: ~/.npm
          key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
      
      - name: Install and build
        run: |
          npm ci
          npm run build
```

### Key Components

**Triggers (`on`)**: 
- Use specific branch filters for `push` and `pull_request`
- Add `workflow_dispatch` for manual runs
- Consider `schedule` for nightly builds or security scans

**Concurrency**:
```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true  # Cancel outdated runs
```

**Job Dependencies**:
```yaml
jobs:
  build:
    # ...
  
  test:
    needs: build  # Run after build completes
    # ...
  
  deploy:
    needs: [build, test]  # Wait for both
    if: github.ref == 'refs/heads/main'  # Conditional execution
    # ...
```

## Security Checklist

- [ ] All actions pinned to full commit SHA (not tags or branches)
- [ ] `permissions` explicitly defined with least privilege
- [ ] Secrets accessed via `${{ secrets.SECRET_NAME }}` only
- [ ] OIDC configured for cloud authentication (no long-lived credentials)
- [ ] Dependency scanning enabled (dependency-review-action, Snyk)
- [ ] SAST tools integrated (CodeQL, SonarQube)
- [ ] Secret scanning enabled for repository
- [ ] Self-hosted runners properly secured (if used)

## Performance Checklist

- [ ] Caching configured for dependencies with `hashFiles()` keys
- [ ] Matrix strategies used for parallel testing
- [ ] `fetch-depth: 1` for shallow clones where appropriate
- [ ] Artifacts used for inter-job data transfer
- [ ] Jobs parallelized where possible (no unnecessary `needs` dependencies)
- [ ] Large files managed with Git LFS optimization

## Testing Checklist

- [ ] Unit tests run on every push/PR
- [ ] Integration tests with real dependencies (using `services`)
- [ ] E2E tests against staging environment
- [ ] Performance/load tests for critical applications
- [ ] Test reports published as artifacts and GitHub Checks
- [ ] Code coverage tracked and enforced

## Deployment Checklist

- [ ] Staging environment configured with protection rules
- [ ] Production environment with manual approval gates
- [ ] Rollback strategy documented and tested
- [ ] Post-deployment health checks automated
- [ ] Deployment type appropriate for application criticality
- [ ] Monitoring and alerting configured

## Common Patterns

### Multi-Environment Deployment

```yaml
jobs:
  deploy-staging:
    environment: staging
    if: github.ref == 'refs/heads/develop'
    # ... deploy to staging
  
  deploy-production:
    needs: deploy-staging
    environment: production  # Requires manual approval
    if: github.ref == 'refs/heads/main'
    # ... deploy to production
```

### Conditional Job Execution

```yaml
jobs:
  security-scan:
    if: github.event_name == 'pull_request'
    # ... run security scans on PRs
  
  deploy:
    if: github.ref == 'refs/heads/main' && github.event_name == 'push'
    # ... deploy only on main branch pushes
```

### Passing Data Between Jobs

```yaml
jobs:
  build:
    outputs:
      version: ${{ steps.package.outputs.version }}
    steps:
      - id: package
        run: echo "version=1.2.3" >> "$GITHUB_OUTPUT"
  
  deploy:
    needs: build
    steps:
      - run: echo "Deploying version ${{ needs.build.outputs.version }}"
```

## Review Process

When reviewing or creating workflows, follow this order:

1. **Security**: Verify all security checklist items above
2. **Structure**: Ensure clear job organization and naming
3. **Performance**: Check for optimization opportunities
4. **Testing**: Validate comprehensive test coverage
5. **Deployment**: Review deployment strategy and rollback plan
6. **Troubleshooting**: Add logging and error handling

## Next Steps

- Review [security-best-practices.md](references/security-best-practices.md) for detailed security guidance
- Check [examples/](examples/) for complete workflow templates
- Consult [troubleshooting.md](references/troubleshooting.md) when issues arise
- Read [deployment-strategies.md](references/deployment-strategies.md) for production deployment patterns

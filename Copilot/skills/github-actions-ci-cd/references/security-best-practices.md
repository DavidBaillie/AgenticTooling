# Security Best Practices for GitHub Actions

## Critical Security Rules

### 1. Always Pin Actions to Commit SHA

**NEVER use mutable references** like tags (`@v4`) or branches (`@main`, `@latest`).

**VULNERABLE** (Mutable - can be redirected to malicious code):
```yaml
- uses: actions/checkout@v4
- uses: third-party/action@main
- uses: some-action@latest
```

**SECURE** (Immutable commit SHA with version comment):
```yaml
- uses: actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v6.0.2
- uses: actions/setup-node@3235b876344d2a9aa001b8d1453c930bba69e610 # v3.9.1
- uses: actions/cache@668228422ae6a00e4ad889ee87cd7109ec5666a7 # v5.0.4
```

**Why this matters**: Tags and branches are mutable references. A malicious actor who gains write access to an action's repository can silently move a tag (e.g., `@v4`) to a compromised commit, executing arbitrary code in your workflow. This is a **supply chain attack**. A commit SHA is immutable and cannot be redirected.

**How to find the SHA**:
1. Go to the action's GitHub repository
2. Find the tag/release you want (e.g., `v4.3.1`)
3. Click on the tag to see its commit
4. Copy the full SHA from the commit page
5. Add it with a version comment: `@<SHA> # v4.3.1`

### 2. Least Privilege for GITHUB_TOKEN

**Default to read-only permissions**, grant write access only when absolutely necessary.

**TOO PERMISSIVE** (Default broad permissions):
```yaml
# No permissions defined - uses broad defaults
jobs:
  build:
    steps:
      - uses: actions/checkout@<SHA>
```

**LEAST PRIVILEGE**:
```yaml
permissions:
  contents: read  # Read-only by default
  pull-requests: write  # Only if needed to update PRs
  checks: write  # Only if needed for status checks

jobs:
  build:
    permissions:
      contents: read  # This job only needs read
    steps:
      - uses: actions/checkout@<SHA>
  
  deploy:
    permissions:
      contents: write  # Only deploy job can write
      packages: write  # Only deploy job publishes packages
    steps:
      # deployment steps
```

**Common permission mappings**:
- Reading code: `contents: read`
- Pushing commits/tags: `contents: write`
- Updating PR status: `pull-requests: write`
- Publishing packages: `packages: write`
- Managing issues: `issues: write`
- Updating check runs: `checks: write`

### 3. Secret Management

**Never hardcode secrets**. Always use GitHub Secrets.

**NEVER DO THIS**:
```yaml
env:
  API_KEY: "sk-1234567890abcdef"  # VULNERABLE!
  DATABASE_PASSWORD: "mypassword"  # NEVER!

steps:
  - run: echo "MY_SECRET=abc123" >> $GITHUB_ENV  # NO!
  - run: curl -H "Authorization: Bearer hardcoded-token"  # BAD!
```

**SECURE SECRET MANAGEMENT**:
```yaml
jobs:
  deploy:
    environment: production  # Environment-specific secrets
    steps:
      - name: Deploy with secrets
        env:
          API_KEY: ${{ secrets.PROD_API_KEY }}
          DB_PASSWORD: ${{ secrets.DB_PASSWORD }}
        run: |
          ./deploy.sh
          # Secrets are automatically masked in logs
```

**Secret scope hierarchy** (most to least restrictive):
1. **Environment secrets**: Tied to specific environments (staging, production), can require manual approval
2. **Repository secrets**: Available to all workflows in the repository
3. **Organization secrets**: Shared across repositories (use cautiously)

**Best practices**:
- Use environment secrets for deployment credentials
- Rotate secrets regularly
- Never log or print secrets (even if masked)
- Limit secret access to specific workflows/jobs when possible
- Use different secrets for staging and production

### 4. OpenID Connect (OIDC) for Cloud Authentication

**Eliminate long-lived credentials** by using OIDC to exchange short-lived tokens with cloud providers.

**AVOID** (Long-lived credentials in secrets):
```yaml
- name: Configure AWS
  env:
    AWS_ACCESS_KEY_ID: ${{ secrets.AWS_ACCESS_KEY_ID }}
    AWS_SECRET_ACCESS_KEY: ${{ secrets.AWS_SECRET_ACCESS_KEY }}
  run: aws configure
```

**USE OIDC** (Temporary credentials):
```yaml
permissions:
  id-token: write  # Required for OIDC
  contents: read

jobs:
  deploy-to-aws:
    steps:
      - uses: aws-actions/configure-aws-credentials@<SHA> # v4.x.x
        with:
          role-to-assume: arn:aws:iam::123456789012:role/GitHubActionsRole
          aws-region: us-east-1
      
      - run: aws s3 ls  # Authenticated with temporary credentials
```

**OIDC Setup Requirements**:
1. Configure identity provider in your cloud platform (AWS IAM, Azure AD, GCP)
2. Create a trust policy that trusts GitHub's OIDC issuer
3. Assign appropriate permissions to the role/identity
4. Use the OIDC action for authentication in workflows

**Benefits**:
- No long-lived credentials stored in secrets
- Temporary credentials expire automatically
- More granular permission control
- Audit trail of authentication events

### 5. Dependency Scanning and Software Composition Analysis (SCA)

**Continuously scan dependencies** for known vulnerabilities.

```yaml
name: Dependency Review

on: [pull_request]

permissions:
  contents: read
  pull-requests: write

jobs:
  dependency-review:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      
      - name: Dependency Review
        uses: actions/dependency-review-action@<SHA>
        with:
          fail-on-severity: moderate
          deny-licenses: GPL-2.0, GPL-3.0
```

**Recommended tools**:
- `dependency-review-action` (GitHub native)
- Snyk
- Trivy
- Mend (formerly WhiteSource)
- OWASP Dependency-Check

### 6. Static Application Security Testing (SAST)

**Scan source code** for security vulnerabilities before runtime.

```yaml
name: CodeQL Analysis

on:
  push:
    branches: [main]
  pull_request:
    branches: [main]

permissions:
  security-events: write
  contents: read

jobs:
  analyze:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      
      - name: Initialize CodeQL
        uses: github/codeql-action/init@<SHA>
        with:
          languages: javascript, python
      
      - name: Perform CodeQL Analysis
        uses: github/codeql-action/analyze@<SHA>
```

**Recommended tools**:
- CodeQL (GitHub Advanced Security)
- SonarQube/SonarCloud
- Bandit (Python)
- ESLint with security plugins (JavaScript/TypeScript)
- Semgrep

### 7. Secret Scanning

**Prevent secrets from being committed** to the repository.

**Enable GitHub secret scanning**:
1. Go to repository Settings → Security & analysis
2. Enable "Secret scanning"
3. Enable "Push protection" to prevent commits with secrets

**Use pre-commit hooks locally**:
```bash
# Install git-secrets
brew install git-secrets

# Setup in repository
git secrets --install
git secrets --register-aws
git secrets --scan
```

**Pre-commit hook example** (`.git/hooks/pre-commit`):
```bash
#!/bin/bash
# Scan for secrets before commit
if git secrets --scan; then
  exit 0
else
  echo "ERROR: Found secrets in commit. Aborting."
  exit 1
fi
```

### 8. Container Image Security

**Sign and verify container images** to ensure integrity.

```yaml
jobs:
  build-and-sign:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      packages: write
      id-token: write  # For signing
    
    steps:
      - uses: actions/checkout@<SHA>
      
      - name: Build container image
        run: docker build -t myapp:${{ github.sha }} .
      
      - name: Install Cosign
        uses: sigstore/cosign-installer@<SHA>
      
      - name: Sign container image
        run: |
          cosign sign --yes myapp:${{ github.sha }}
      
      - name: Push signed image
        run: docker push myapp:${{ github.sha }}
```

**Verify signed images during deployment**:
```yaml
- name: Verify image signature
  run: cosign verify myapp:${{ github.sha }}
```

### 9. Self-Hosted Runner Security

If using self-hosted runners, follow these guidelines:

**Security hardening**:
- Run runners on ephemeral VMs that are destroyed after each job
- Use dedicated, isolated networks
- Implement strict firewall rules
- Keep runner software updated
- Use runner groups to control access
- Never use self-hosted runners for public repositories (security risk)

**Best practices**:
- Prefer GitHub-hosted runners when possible
- Use self-hosted runners only for private repositories
- Monitor runner activity and logs
- Implement network-level access controls
- Regular security audits and patching

### 10. Audit Marketplace Actions

**Review third-party actions** before using them.

**Checklist**:
- [ ] Action from trusted source (e.g., `actions/` organization)?
- [ ] Repository has many stars and active maintenance?
- [ ] Code is open source and reviewable?
- [ ] Recent commits and active issue triage?
- [ ] Positive community feedback?
- [ ] Security advisories or known vulnerabilities?

**Use Dependabot for action updates**:
```yaml
# .github/dependabot.yml
version: 2
updates:
  - package-ecosystem: "github-actions"
    directory: "/"
    schedule:
      interval: "weekly"
```

## Security Review Checklist

When reviewing workflows for security:

- [ ] All actions pinned to full commit SHA (not tags/branches)
- [ ] `permissions` explicitly defined with least privilege
- [ ] No hardcoded secrets or credentials
- [ ] OIDC used for cloud authentication (no long-lived credentials)
- [ ] Dependency scanning enabled
- [ ] SAST tools integrated
- [ ] Secret scanning enabled
- [ ] Self-hosted runners properly secured (if used)
- [ ] Third-party actions audited and from trusted sources
- [ ] Container images signed and verified
- [ ] Sensitive operations require manual approval
- [ ] Logs don't expose sensitive information

## Additional Resources

- [GitHub Actions Security Hardening Guide](https://docs.github.com/en/actions/security-guides/security-hardening-for-github-actions)
- [OIDC Documentation](https://docs.github.com/en/actions/deployment/security-hardening-your-deployments/about-security-hardening-with-openid-connect)
- [Secret Scanning](https://docs.github.com/en/code-security/secret-scanning/about-secret-scanning)
- [Dependency Review](https://docs.github.com/en/code-security/supply-chain-security/understanding-your-software-supply-chain/about-dependency-review)

# Troubleshooting GitHub Actions Workflows

## Common Issues and Solutions

### 1. Workflow Not Triggering

#### Symptoms
- Workflow doesn't appear in Actions tab
- No workflow runs after push/PR
- Expected trigger event doesn't start workflow

#### Root Causes
- Incorrect `on` trigger configuration
- Branch/path filters don't match
- Workflow file syntax errors
- Workflow file not in default branch (for `workflow_dispatch`)

#### Solutions

**Check trigger configuration**:
```yaml
# ❌ Wrong: Trigger won't match your branch
on:
  push:
    branches: [master]  # But your default branch is 'main'

# ✅ Correct: Matches your branch
on:
  push:
    branches: [main, develop]
```

**Check path filters**:
```yaml
# ❌ Wrong: Excludes your changes
on:
  push:
    paths:
      - 'src/**'  # But you changed 'tests/**'

# ✅ Correct: Includes relevant paths
on:
  push:
    paths:
      - 'src/**'
      - 'tests/**'
      - 'package.json'
```

**Verify workflow syntax**:
```bash
# Use GitHub CLI to validate workflow
gh workflow view <workflow-name>

# Or validate YAML syntax
yamllint .github/workflows/*.yml
```

**Check concurrency blocking**:
```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: false  # Might be blocking new runs

# Check in Actions tab → Workflow → "Concurrency" section
```

**Debug with workflow_dispatch**:
```yaml
on:
  push:
    branches: [main]
  workflow_dispatch:  # Add manual trigger for debugging
```

### 2. Jobs or Steps Skipping

#### Symptoms
- Jobs show as "Skipped" in workflow run
- Steps don't execute
- Conditional steps not running

#### Root Causes
- `if` conditions evaluating to false
- Previous job failure (with `needs` dependency)
- Branch/event filters

#### Solutions

**Debug `if` conditions**:
```yaml
- name: Debug context
  run: |
    echo "Event: ${{ github.event_name }}"
    echo "Ref: ${{ github.ref }}"
    echo "Actor: ${{ github.actor }}"
    echo "SHA: ${{ github.sha }}"

- name: Debug job context
  run: echo '${{ toJson(github) }}'

- name: Debug needs context
  run: echo '${{ toJson(needs) }}'
```

**Common `if` condition mistakes**:
```yaml
# ❌ Wrong: String comparison without quotes
if: github.ref == refs/heads/main

# ✅ Correct: Proper string comparison
if: github.ref == 'refs/heads/main'

# ❌ Wrong: Checking job success incorrectly
if: needs.build == 'success'

# ✅ Correct: Check job outcome
if: needs.build.result == 'success'
```

**Always run steps for debugging**:
```yaml
- name: Debug step
  if: always()  # Run even if previous steps failed
  run: echo "This always runs"
```

### 3. Permission Errors

#### Symptoms
- `Resource not accessible by integration`
- `Permission denied` errors
- `403 Forbidden` responses
- Can't push tags, update PRs, or write packages

#### Root Causes
- `GITHUB_TOKEN` lacks necessary permissions
- Environment secrets not accessible
- OIDC trust policy misconfigured

#### Solutions

**Grant proper permissions**:
```yaml
# ❌ Wrong: Too restrictive
permissions:
  contents: read  # Can't push tags or write to repo

# ✅ Correct: Grant specific needed permissions
permissions:
  contents: write      # For pushing tags/commits
  pull-requests: write # For updating PRs
  packages: write      # For publishing packages
  id-token: write      # For OIDC
```

**Job-level permission override**:
```yaml
permissions:
  contents: read  # Default for workflow

jobs:
  deploy:
    permissions:
      contents: write  # Override for this job only
      packages: write
    steps:
      # Can now push and publish
```

**Check environment access**:
```yaml
jobs:
  deploy:
    environment: production  # Requires environment access
    steps:
      - env:
          SECRET: ${{ secrets.PROD_SECRET }}  # Must exist in 'production' environment
        run: echo "Deploying..."
```

**Debug OIDC issues**:
```yaml
- name: Debug OIDC token
  run: |
    curl -H "Authorization: bearer $ACTIONS_ID_TOKEN_REQUEST_TOKEN" \
      "$ACTIONS_ID_TOKEN_REQUEST_URL&audience=api://AzureADTokenExchange" \
      | jq
```

**Verify cloud trust policy** (AWS example):
```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Effect": "Allow",
      "Principal": {
        "Federated": "arn:aws:iam::123456789012:oidc-provider/token.actions.githubusercontent.com"
      },
      "Action": "sts:AssumeRoleWithWebIdentity",
      "Condition": {
        "StringEquals": {
          "token.actions.githubusercontent.com:aud": "sts.amazonaws.com",
          "token.actions.githubusercontent.com:sub": "repo:owner/repo:ref:refs/heads/main"
        }
      }
    }
  ]
}
```

### 4. Caching Issues

#### Symptoms
- `Cache not found` every run
- `Cache miss` for unchanged dependencies
- Cache size too large
- Slow cache restore

#### Root Causes
- Incorrect cache key or path
- `hashFiles()` pattern doesn't match files
- Cache eviction (7-day limit or 10GB limit)
- Path doesn't exist at cache time

#### Solutions

**Verify cache key pattern**:
```yaml
# ❌ Wrong: Pattern doesn't match any files
key: ${{ runner.os }}-node-${{ hashFiles('package-lock.json') }}
# Missing **/ prefix

# ✅ Correct: Matches files in subdirectories
key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
```

**Debug cache behavior**:
```yaml
- name: Check lock file
  run: |
    ls -la package-lock.json
    sha256sum package-lock.json

- name: Cache dependencies
  id: cache
  uses: actions/cache@<SHA>
  with:
    path: ~/.npm
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}

- name: Check cache result
  run: |
    echo "Cache hit: ${{ steps.cache.outputs.cache-hit }}"
    echo "Cache key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}"
```

**Verify cache path exists**:
```yaml
- name: Show cache directory
  run: ls -la ~/.npm || echo "Cache directory doesn't exist"

- name: Cache dependencies
  uses: actions/cache@<SHA>
  with:
    path: ~/.npm
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
```

**Use restore-keys for fallback**:
```yaml
- uses: actions/cache@<SHA>
  with:
    path: ~/.npm
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
    restore-keys: |
      ${{ runner.os }}-node-  # Fallback to any node cache
      ${{ runner.os }}-       # Fallback to any OS cache
```

**Inspect cache usage**:
```bash
# Using GitHub CLI
gh cache list

# Delete specific cache
gh cache delete <cache-id>
```

### 5. Long Running Workflows / Timeouts

#### Symptoms
- Workflows take too long
- Job timeout after 6 hours (default)
- Slow dependency installation
- Long build times

#### Root Causes
- No caching configured
- Sequential jobs that could be parallel
- Large dependency trees
- Full Git history fetch
- Inefficient build processes

#### Solutions

**Add aggressive caching**:
```yaml
- uses: actions/cache@<SHA>
  with:
    path: |
      ~/.npm
      ~/.cache
      node_modules
    key: ${{ runner.os }}-deps-${{ hashFiles('**/package-lock.json') }}
```

**Parallelize jobs**:
```yaml
# ❌ Slow: Sequential execution
jobs:
  lint:
    # ...
  test:
    needs: lint  # Waits unnecessarily
  build:
    needs: test  # Waits unnecessarily

# ✅ Fast: Parallel execution
jobs:
  lint:
    # Runs in parallel
  test:
    # Runs in parallel
  build:
    # Runs in parallel
  
  deploy:
    needs: [lint, test, build]  # Wait only for deployment
```

**Use shallow clone**:
```yaml
- uses: actions/checkout@<SHA>
  with:
    fetch-depth: 1  # Don't fetch full history
```

**Optimize Docker builds**:
```yaml
# Use BuildKit and layer caching
- name: Build Docker image
  run: |
    DOCKER_BUILDKIT=1 docker build \
      --cache-from myapp:latest \
      --build-arg BUILDKIT_INLINE_CACHE=1 \
      -t myapp:${{ github.sha }} .
```

**Profile execution time**:
```yaml
- name: Install dependencies
  run: |
    start=$(date +%s)
    npm ci
    end=$(date +%s)
    echo "⏱️ Installation took $((end-start)) seconds"
```

**Increase timeout for specific jobs**:
```yaml
jobs:
  long-running-job:
    timeout-minutes: 120  # Default is 360 (6 hours)
    steps:
      # ...
```

### 6. Flaky Tests

#### Symptoms
- Tests pass locally, fail in CI
- Intermittent test failures
- Random timeouts
- Tests fail/pass on retry

#### Root Causes
- Race conditions
- Hardcoded timeouts
- Environmental differences
- Non-deterministic tests
- Reliance on external services

#### Solutions

**Use explicit waits (not sleeps)**:
```javascript
// ❌ Bad: Arbitrary wait
await page.waitForTimeout(3000);

// ✅ Good: Wait for specific condition
await page.waitForSelector('[data-testid="submit-button"]');
await page.waitForLoadState('networkidle');
```

**Implement retries**:
```yaml
- name: Run tests with retry
  uses: nick-fields/retry@<SHA>
  with:
    timeout_minutes: 10
    max_attempts: 3
    command: npm test
```

**Ensure test isolation**:
```javascript
// ❌ Bad: Tests depend on each other
test('create user', async () => {
  user = await createUser();  // Global state
});

test('update user', async () => {
  await updateUser(user);  // Depends on previous test
});

// ✅ Good: Each test is independent
test('update user', async () => {
  const user = await createUser();
  await updateUser(user);
  await deleteUser(user);
});
```

**Capture diagnostics on failure**:
```yaml
- name: Run E2E tests
  id: e2e
  run: npm run test:e2e

- name: Upload screenshots on failure
  if: failure() && steps.e2e.conclusion == 'failure'
  uses: actions/upload-artifact@<SHA>
  with:
    name: test-screenshots
    path: screenshots/

- name: Upload video on failure
  if: failure()
  uses: actions/upload-artifact@<SHA>
  with:
    name: test-videos
    path: videos/
```

### 7. Secrets Not Working

#### Symptoms
- Secret value is empty
- `secrets.SECRET_NAME` returns nothing
- Authentication fails with correct secret
- Secret masking not working

#### Root Causes
- Secret not defined in correct scope
- Environment secret not accessible
- Typo in secret name
- Secret contains special characters causing issues

#### Solutions

**Verify secret exists**:
```yaml
- name: Check if secret is set
  run: |
    if [ -z "${{ secrets.API_KEY }}" ]; then
      echo "❌ SECRET is not set!"
      exit 1
    else
      echo "✅ Secret is set (length: ${#API_KEY} chars)"
    fi
  env:
    API_KEY: ${{ secrets.API_KEY }}
```

**Check secret scope**:
```yaml
# Repository secret
secrets.REPO_SECRET

# Environment secret (only available with environment)
jobs:
  deploy:
    environment: production  # Required for environment secrets
    steps:
      - env:
          SECRET: ${{ secrets.ENV_SECRET }}
```

**Debug secret access** (without exposing value):
```yaml
- name: Debug secret
  run: |
    echo "Secret length: ${#SECRET}"
    echo "First char: ${SECRET:0:1}"
    echo "Last char: ${SECRET: -1}"
  env:
    SECRET: ${{ secrets.MY_SECRET }}
```

**Handle multiline secrets**:
```yaml
# Multiline secret (e.g., SSH key, certificate)
- name: Use multiline secret
  run: |
    echo "${{ secrets.SSH_PRIVATE_KEY }}" > key.pem
    chmod 600 key.pem
```

### 8. Artifact Issues

#### Symptoms
- Artifact upload fails
- Artifact not found for download
- Empty artifact
- Large artifact causing issues

#### Root Causes
- Path doesn't exist at upload time
- Incorrect artifact name reference
- Artifact expired (retention period)
- Artifact too large

#### Solutions

**Verify path exists**:
```yaml
- name: Check artifact path
  run: |
    ls -la dist/ || echo "⚠️ dist/ directory doesn't exist"

- uses: actions/upload-artifact@<SHA>
  with:
    name: build-output
    path: dist/
```

**Debug artifact contents**:
```yaml
- name: Show artifact contents
  run: |
    echo "Files to upload:"
    find dist/ -type f

- uses: actions/upload-artifact@<SHA>
  with:
    name: build-output
    path: dist/
```

**Download artifact correctly**:
```yaml
jobs:
  build:
    steps:
      - uses: actions/upload-artifact@<SHA>
        with:
          name: my-artifact  # Remember this name
          path: dist/

  deploy:
    needs: build
    steps:
      - uses: actions/download-artifact@<SHA>
        with:
          name: my-artifact  # Must match upload name
          path: ./downloaded  # Downloads to ./downloaded/
```

**Handle large artifacts**:
```yaml
- name: Compress artifact
  run: tar -czf artifact.tar.gz dist/

- uses: actions/upload-artifact@<SHA>
  with:
    name: compressed-artifact
    path: artifact.tar.gz
    retention-days: 7  # Reduce retention for large files
```

## Debugging Techniques

### Enable Debug Logging

**Repository secrets**:
- Set `ACTIONS_STEP_DEBUG` to `true`
- Set `ACTIONS_RUNNER_DEBUG` to `true`

**In workflow**:
```yaml
env:
  ACTIONS_STEP_DEBUG: true
  ACTIONS_RUNNER_DEBUG: true
```

### Add Debug Steps

```yaml
- name: 🐛 Debug - Print all env vars
  run: env | sort

- name: 🐛 Debug - Print GitHub context
  run: echo '${{ toJson(github) }}'

- name: 🐛 Debug - Print job context
  run: echo '${{ toJson(job) }}'

- name: 🐛 Debug - Print runner context
  run: echo '${{ toJson(runner) }}'

- name: 🐛 Debug - Print secrets (names only)
  run: |
    echo "Available secrets:"
    echo "${{ toJson(secrets) }}" | jq 'keys'
```

### Use tmate for Interactive Debugging

```yaml
- name: Setup tmate session
  if: failure()
  uses: mxschmitt/action-tmate@<SHA>
  timeout-minutes: 15
  with:
    limit-access-to-actor: true
```

### Check Runner Logs

Access runner diagnostic logs:
1. Go to workflow run
2. Click on job
3. Click gear icon (⚙️) → "Download log archive"

## Workflow Health Checklist

When troubleshooting, verify:

- [ ] Workflow syntax is valid YAML
- [ ] Trigger conditions match your events
- [ ] All referenced actions are pinned to SHA
- [ ] Permissions are explicitly defined
- [ ] Secrets exist in correct scope
- [ ] Cache keys use `hashFiles()` correctly
- [ ] Artifact names match between upload/download
- [ ] `if` conditions use proper syntax
- [ ] Jobs are parallelized where possible
- [ ] Debug logging enabled for investigation

## Getting Help

**GitHub Actions Documentation**:
- [Workflow Syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
- [Troubleshooting](https://docs.github.com/en/actions/monitoring-and-troubleshooting-workflows)
- [Context and Expressions](https://docs.github.com/en/actions/learn-github-actions/contexts)

**Community Resources**:
- [GitHub Community Forum](https://github.community/c/code-to-cloud/github-actions/)
- [GitHub Actions Marketplace](https://github.com/marketplace?type=actions)

**GitHub Support**:
- For paid accounts: Open support ticket
- Check [GitHub Status](https://www.githubstatus.com/) for service issues

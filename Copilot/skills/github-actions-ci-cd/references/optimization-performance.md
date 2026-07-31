# Optimization and Performance Best Practices

## 1. Effective Caching Strategies

Caching is crucial for fast workflow execution. A well-designed cache strategy can reduce build times by 50-90%.

### Cache Key Design

**Use `hashFiles()` for dependency lock files** to automatically invalidate cache when dependencies change:

```yaml
- name: Cache Node.js modules
  uses: actions/cache@668228422ae6a00e4ad889ee87cd7109ec5666a7 # v5.0.4
  with:
    path: ~/.npm
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
    restore-keys: |
      ${{ runner.os }}-node-
```

**Key components**:
- `${{ runner.os }}`: OS-specific cache (ubuntu, windows, macos)
- `hashFiles('**/package-lock.json')`: Hash of dependency lock file
- `restore-keys`: Fallback for partial cache hits

### Common Cache Patterns

**Node.js / npm**:
```yaml
- uses: actions/cache@<SHA>
  with:
    path: |
      ~/.npm
      node_modules
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
    restore-keys: ${{ runner.os }}-node-
```

**Python / pip**:
```yaml
- uses: actions/cache@<SHA>
  with:
    path: ~/.cache/pip
    key: ${{ runner.os }}-pip-${{ hashFiles('**/requirements.txt') }}
    restore-keys: ${{ runner.os }}-pip-
```

**Java / Maven**:
```yaml
- uses: actions/cache@<SHA>
  with:
    path: ~/.m2/repository
    key: ${{ runner.os }}-maven-${{ hashFiles('**/pom.xml') }}
    restore-keys: ${{ runner.os }}-maven-
```

**Java / Gradle**:
```yaml
- uses: actions/cache@<SHA>
  with:
    path: |
      ~/.gradle/caches
      ~/.gradle/wrapper
    key: ${{ runner.os }}-gradle-${{ hashFiles('**/*.gradle*', '**/gradle-wrapper.properties') }}
    restore-keys: ${{ runner.os }}-gradle-
```

**Go modules**:
```yaml
- uses: actions/cache@<SHA>
  with:
    path: ~/go/pkg/mod
    key: ${{ runner.os }}-go-${{ hashFiles('**/go.sum') }}
    restore-keys: ${{ runner.os }}-go-
```

**Rust / Cargo**:
```yaml
- uses: actions/cache@<SHA>
  with:
    path: |
      ~/.cargo/bin/
      ~/.cargo/registry/index/
      ~/.cargo/registry/cache/
      ~/.cargo/git/db/
      target/
    key: ${{ runner.os }}-cargo-${{ hashFiles('**/Cargo.lock') }}
```

**Docker layer caching**:
```yaml
- uses: docker/setup-buildx-action@<SHA>

- uses: actions/cache@<SHA>
  with:
    path: /tmp/.buildx-cache
    key: ${{ runner.os }}-buildx-${{ github.sha }}
    restore-keys: ${{ runner.os }}-buildx-

- uses: docker/build-push-action@<SHA>
  with:
    cache-from: type=local,src=/tmp/.buildx-cache
    cache-to: type=local,dest=/tmp/.buildx-cache-new,mode=max
```

### Advanced Cache Strategies

**Monorepo caching** (cache per package):
```yaml
- uses: actions/cache@<SHA>
  with:
    path: |
      ~/.npm
      packages/*/node_modules
    key: ${{ runner.os }}-monorepo-${{ hashFiles('**/package-lock.json') }}-${{ github.run_id }}
    restore-keys: |
      ${{ runner.os }}-monorepo-${{ hashFiles('**/package-lock.json') }}-
      ${{ runner.os }}-monorepo-
```

**Build output caching** (reuse compiled artifacts):
```yaml
- uses: actions/cache@<SHA>
  with:
    path: |
      dist
      build
      .next/cache
    key: ${{ runner.os }}-build-${{ hashFiles('src/**') }}-${{ github.sha }}
    restore-keys: |
      ${{ runner.os }}-build-${{ hashFiles('src/**') }}-
      ${{ runner.os }}-build-
```

### Cache Debugging

**Check cache hit rate**:
```yaml
- name: Cache dependencies
  id: cache
  uses: actions/cache@<SHA>
  with:
    path: ~/.npm
    key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}

- name: Check cache status
  run: |
    if [ "${{ steps.cache.outputs.cache-hit }}" == "true" ]; then
      echo "✅ Cache hit! Skipping install."
    else
      echo "❌ Cache miss. Installing dependencies."
    fi
```

## 2. Matrix Strategies for Parallelization

Run jobs in parallel across multiple configurations to dramatically reduce total workflow time.

### Basic Matrix

```yaml
jobs:
  test:
    runs-on: ${{ matrix.os }}
    strategy:
      matrix:
        os: [ubuntu-latest, windows-latest, macos-latest]
        node-version: [16, 18, 20]
    steps:
      - uses: actions/checkout@<SHA>
      - uses: actions/setup-node@<SHA>
        with:
          node-version: ${{ matrix.node-version }}
      - run: npm test
```

This creates **9 parallel jobs** (3 OSs × 3 Node versions).

### Advanced Matrix with Include/Exclude

```yaml
strategy:
  matrix:
    os: [ubuntu-latest, windows-latest]
    node-version: [16, 18, 20]
    include:
      # Add extra configuration for ubuntu-latest + node 20
      - os: ubuntu-latest
        node-version: 20
        experimental: true
    exclude:
      # Skip windows + node 16 (not supported)
      - os: windows-latest
        node-version: 16
```

### Fail-Fast Control

```yaml
strategy:
  fail-fast: false  # Continue all tests even if one fails
  matrix:
    os: [ubuntu-latest, windows-latest, macos-latest]
```

**When to use `fail-fast: false`**:
- Comprehensive test reporting (want to see all failures)
- Debugging platform-specific issues
- CI pipelines where you need full test coverage results

**When to use `fail-fast: true`** (default):
- Quick feedback on critical failures
- Cost optimization (stop early on failure)
- PR checks where first failure is enough to block merge

## 3. Fast Checkout and Shallow Clones

Optimize repository checkout time, especially for large repositories.

### Shallow Clone (Recommended for Most Builds)

```yaml
- uses: actions/checkout@de0fac2e4500dabe0009e67214ff5f5447ce83dd # v6.0.2
  with:
    fetch-depth: 1  # Only fetch the latest commit
```

**Benefits**:
- Faster checkout (seconds vs. minutes for large repos)
- Reduced bandwidth usage
- Smaller disk usage on runner

**Use `fetch-depth: 1` when**:
- Building and testing the latest commit
- Most CI/CD workflows
- Docker image builds

### Full History (When Needed)

```yaml
- uses: actions/checkout@<SHA>
  with:
    fetch-depth: 0  # Fetch full Git history
```

**Use `fetch-depth: 0` when**:
- Creating releases with changelogs (need git history)
- Running `git blame` or `git log` analysis
- Semantic versioning based on commit history
- Tools that require full Git history

### Submodule Control

```yaml
- uses: actions/checkout@<SHA>
  with:
    submodules: false  # Don't fetch submodules (faster)
```

Only fetch submodules if your build actually needs them:
```yaml
- uses: actions/checkout@<SHA>
  with:
    submodules: true  # Fetch all submodules
    # OR
    submodules: recursive  # Fetch nested submodules
```

## 4. Artifacts for Inter-Job Communication

Use artifacts to pass data efficiently between jobs and workflows.

### Upload Artifacts

```yaml
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      - run: npm run build
      
      - name: Upload build artifacts
        uses: actions/upload-artifact@bbbca2ddaa5d8feaa63e36b76fdaad77386f024f # v7.0.0
        with:
          name: build-output
          path: |
            dist/
            build/
          retention-days: 7  # Keep for 7 days (cost optimization)
```

### Download Artifacts

```yaml
jobs:
  deploy:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Download build artifacts
        uses: actions/download-artifact@3e5f45b2cfb9172054b4087a40e8e0b5a5461e7c # v8.0.1
        with:
          name: build-output
          path: ./dist
      
      - run: ./deploy.sh
```

### Multiple Artifacts

```yaml
- name: Upload test results
  uses: actions/upload-artifact@<SHA>
  with:
    name: test-results-${{ matrix.os }}-${{ matrix.node-version }}
    path: test-results/

- name: Upload coverage reports
  uses: actions/upload-artifact@<SHA>
  with:
    name: coverage
    path: coverage/
    retention-days: 30
```

### Artifact Best Practices

**Set appropriate retention periods**:
- Test results: 7-14 days
- Build artifacts for releases: 90 days
- Coverage reports: 30 days
- Logs and debugging info: 7 days

**Compress large artifacts**:
```yaml
- run: tar -czf build.tar.gz dist/
- uses: actions/upload-artifact@<SHA>
  with:
    name: build
    path: build.tar.gz
```

## 5. Job Parallelization

Minimize unnecessary job dependencies to maximize parallelization.

### ❌ Sequential (Slow)

```yaml
jobs:
  lint:
    runs-on: ubuntu-latest
    steps:
      - run: npm run lint
  
  test:
    needs: lint  # Unnecessary dependency
    runs-on: ubuntu-latest
    steps:
      - run: npm test
  
  build:
    needs: test  # Unnecessary dependency
    runs-on: ubuntu-latest
    steps:
      - run: npm run build
```

### ✅ Parallel (Fast)

```yaml
jobs:
  lint:
    runs-on: ubuntu-latest
    steps:
      - run: npm run lint
  
  test:
    runs-on: ubuntu-latest
    steps:
      - run: npm test
  
  build:
    runs-on: ubuntu-latest
    steps:
      - run: npm run build
  
  # Only wait for all checks before deploying
  deploy:
    needs: [lint, test, build]
    if: github.ref == 'refs/heads/main'
    runs-on: ubuntu-latest
    steps:
      - run: ./deploy.sh
```

## 6. Self-Hosted Runners

Use self-hosted runners for specialized needs or cost optimization.

### When to Use Self-Hosted Runners

**Good use cases**:
- Access to private networks or on-premise resources
- Specialized hardware (GPUs, specific CPU architectures)
- Very high build volumes (cost optimization)
- Large persistent caches
- Regulated environments with specific compliance requirements

**Avoid self-hosted runners when**:
- GitHub-hosted runners meet your needs
- Public repositories (security risk)
- You can't maintain runner infrastructure

### Self-Hosted Runner Configuration

```yaml
jobs:
  build:
    runs-on: [self-hosted, linux, x64, gpu]  # Custom labels
    steps:
      - uses: actions/checkout@<SHA>
      - run: ./build-with-gpu.sh
```

### Auto-Scaling Self-Hosted Runners

Use tools like:
- **GitHub-hosted larger runners** (paid, managed)
- **Actions Runner Controller (ARC)** for Kubernetes
- **Terraform** for cloud VM auto-scaling
- **AWS EC2 Auto Scaling** or **Azure VMSS**

## 7. Workflow-Level Optimization

### Reduce Duplicate Workflow Runs

```yaml
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true  # Cancel outdated runs
```

### Conditional Workflow Execution (Path Filters)

```yaml
on:
  push:
    paths:
      - 'src/**'
      - 'tests/**'
      - 'package.json'
    paths-ignore:
      - 'docs/**'
      - '**.md'
```

### Reusable Workflows

Create reusable workflows to avoid duplication:

**`.github/workflows/reusable-test.yml`**:
```yaml
on:
  workflow_call:
    inputs:
      node-version:
        required: true
        type: string

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      - uses: actions/setup-node@<SHA>
        with:
          node-version: ${{ inputs.node-version }}
      - run: npm test
```

**Usage**:
```yaml
jobs:
  test-node-18:
    uses: ./.github/workflows/reusable-test.yml
    with:
      node-version: '18'
  
  test-node-20:
    uses: ./.github/workflows/reusable-test.yml
    with:
      node-version: '20'
```

## Performance Checklist

- [ ] Caching configured for all dependencies with `hashFiles()` keys
- [ ] Matrix strategies used for parallel testing
- [ ] `fetch-depth: 1` for checkouts that don't need history
- [ ] Artifacts used for inter-job data transfer
- [ ] Jobs parallelized (minimal `needs` dependencies)
- [ ] Concurrency control to cancel outdated runs
- [ ] Path filters to skip unnecessary workflow runs
- [ ] Reusable workflows for common patterns
- [ ] Build outputs cached when possible
- [ ] Large files compressed before artifact upload

## Monitoring Performance

**Track workflow execution time**:
1. Go to repository Actions tab
2. View workflow run summaries
3. Identify slowest jobs/steps
4. Optimize those specific areas

**Profile with timing information**:
```yaml
- name: Build application
  run: |
    time npm run build
```

**Add benchmark steps**:
```yaml
- name: Benchmark build time
  run: |
    echo "Build started at $(date)"
    npm run build
    echo "Build completed at $(date)"
```

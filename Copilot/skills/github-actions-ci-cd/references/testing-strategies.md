# Testing Strategies in CI/CD

## Testing Pyramid

```
         /\
        /E2E\      Fewer, slower, expensive
       /------\
      /  Integ \   Moderate number, moderate speed
     /----------\
    /    Unit    \ Many, fast, cheap
   /--------------\
```

## 1. Unit Tests

**Purpose**: Test individual functions, classes, or modules in isolation.

**Characteristics**:
- Very fast execution (milliseconds per test)
- No external dependencies (databases, APIs, file system)
- High volume (hundreds to thousands of tests)
- Run on every push and PR

### Basic Unit Test Workflow

```yaml
name: Unit Tests

on: [push, pull_request]

permissions:
  contents: read

jobs:
  unit-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      
      - uses: actions/setup-node@<SHA>
        with:
          node-version: 18
      
      - name: Cache dependencies
        uses: actions/cache@<SHA>
        with:
          path: ~/.npm
          key: ${{ runner.os }}-node-${{ hashFiles('**/package-lock.json') }}
      
      - run: npm ci
      
      - name: Run unit tests
        run: npm run test:unit
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@<SHA>
        with:
          name: unit-test-results
          path: test-results/
```

### Unit Tests with Coverage

```yaml
- name: Run unit tests with coverage
  run: npm run test:coverage

- name: Upload coverage to Codecov
  uses: codecov/codecov-action@<SHA>
  with:
    files: ./coverage/coverage-final.json
    fail_ci_if_error: true
    flags: unittests

- name: Check coverage threshold
  run: |
    COVERAGE=$(node -e "console.log(require('./coverage/coverage-summary.json').total.lines.pct)")
    if (( $(echo "$COVERAGE < 80" | bc -l) )); then
      echo "Coverage $COVERAGE% is below 80% threshold"
      exit 1
    fi
```

### Parallel Unit Tests (Matrix)

```yaml
jobs:
  unit-test:
    runs-on: ${{ matrix.os }}
    strategy:
      fail-fast: false
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

## 2. Integration Tests

**Purpose**: Test interactions between components, modules, or services.

**Characteristics**:
- Moderate execution time (seconds to minutes)
- Use real dependencies (databases, message queues)
- Moderate volume (dozens to hundreds of tests)
- Run on PR and pre-deployment

### Integration Tests with Services

```yaml
jobs:
  integration-test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
          POSTGRES_DB: testdb
        options: >-
          --health-cmd pg_isready
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 5432:5432
      
      redis:
        image: redis:7
        options: >-
          --health-cmd "redis-cli ping"
          --health-interval 10s
          --health-timeout 5s
          --health-retries 5
        ports:
          - 6379:6379
    
    steps:
      - uses: actions/checkout@<SHA>
      
      - uses: actions/setup-node@<SHA>
        with:
          node-version: 18
      
      - run: npm ci
      
      - name: Run database migrations
        env:
          DATABASE_URL: postgresql://postgres:postgres@localhost:5432/testdb
        run: npm run migrate
      
      - name: Run integration tests
        env:
          DATABASE_URL: postgresql://postgres:postgres@localhost:5432/testdb
          REDIS_URL: redis://localhost:6379
        run: npm run test:integration
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@<SHA>
        with:
          name: integration-test-results
          path: test-results/
```

### Integration Tests with Docker Compose

```yaml
jobs:
  integration-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      
      - name: Start services
        run: docker-compose -f docker-compose.test.yml up -d
      
      - name: Wait for services to be ready
        run: |
          timeout 60 bash -c 'until docker-compose exec -T app curl -f http://localhost:3000/health; do sleep 2; done'
      
      - name: Run integration tests
        run: docker-compose exec -T app npm run test:integration
      
      - name: Collect logs
        if: always()
        run: docker-compose logs > docker-compose.log
      
      - name: Upload logs
        if: always()
        uses: actions/upload-artifact@<SHA>
        with:
          name: docker-logs
          path: docker-compose.log
      
      - name: Cleanup
        if: always()
        run: docker-compose down -v
```

## 3. End-to-End (E2E) Tests

**Purpose**: Test complete user flows from UI to backend.

**Characteristics**:
- Slow execution (minutes to hours)
- Test against deployed environment
- Small volume (tens of critical paths)
- Run before production deployment

### E2E Tests with Playwright

```yaml
jobs:
  e2e-test:
    runs-on: ubuntu-latest
    strategy:
      fail-fast: false
      matrix:
        browser: [chromium, firefox, webkit]
    
    steps:
      - uses: actions/checkout@<SHA>
      
      - uses: actions/setup-node@<SHA>
        with:
          node-version: 18
      
      - name: Cache dependencies
        uses: actions/cache@<SHA>
        with:
          path: |
            ~/.npm
            ~/.cache/ms-playwright
          key: ${{ runner.os }}-playwright-${{ hashFiles('**/package-lock.json') }}
      
      - run: npm ci
      
      - name: Install Playwright browsers
        run: npx playwright install ${{ matrix.browser }} --with-deps
      
      - name: Run E2E tests
        env:
          BASE_URL: https://staging.example.com
        run: npx playwright test --project=${{ matrix.browser }}
      
      - name: Upload test results
        if: always()
        uses: actions/upload-artifact@<SHA>
        with:
          name: playwright-report-${{ matrix.browser }}
          path: playwright-report/
          retention-days: 30
      
      - name: Upload screenshots on failure
        if: failure()
        uses: actions/upload-artifact@<SHA>
        with:
          name: playwright-screenshots-${{ matrix.browser }}
          path: test-results/
```

### E2E Tests with Cypress

```yaml
jobs:
  e2e-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      
      - name: Run Cypress tests
        uses: cypress-io/github-action@<SHA>
        with:
          browser: chrome
          start: npm start
          wait-on: 'http://localhost:3000'
          wait-on-timeout: 120
          record: true
        env:
          CYPRESS_RECORD_KEY: ${{ secrets.CYPRESS_RECORD_KEY }}
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      
      - name: Upload videos
        if: always()
        uses: actions/upload-artifact@<SHA>
        with:
          name: cypress-videos
          path: cypress/videos
```

### Flakiness Mitigation

**Retry failed tests**:
```yaml
- name: Run E2E tests with retry
  uses: nick-fields/retry@<SHA>
  with:
    timeout_minutes: 30
    max_attempts: 3
    retry_on: error
    command: npm run test:e2e
```

**Use explicit waits** (in test code):
```javascript
// ❌ BAD: Arbitrary sleep
await page.waitForTimeout(3000);

// ✅ GOOD: Wait for specific condition
await page.waitForSelector('[data-testid="submit-button"]');
await page.waitForLoadState('networkidle');
```

## 4. Performance and Load Tests

**Purpose**: Validate application performance under load.

**Characteristics**:
- Very slow execution (minutes to hours)
- Resource intensive
- Run less frequently (nightly, weekly, pre-release)
- Requires production-like environment

### Load Tests with k6

```yaml
jobs:
  load-test:
    runs-on: ubuntu-latest
    if: github.event_name == 'schedule' || github.event_name == 'workflow_dispatch'
    
    steps:
      - uses: actions/checkout@<SHA>
      
      - name: Run k6 load test
        uses: grafana/k6-action@<SHA>
        with:
          filename: tests/load-test.js
        env:
          K6_CLOUD_TOKEN: ${{ secrets.K6_CLOUD_TOKEN }}
      
      - name: Parse k6 results
        run: |
          RESPONSE_TIME=$(jq '.metrics.http_req_duration.avg' k6-results.json)
          if (( $(echo "$RESPONSE_TIME > 500" | bc -l) )); then
            echo "Average response time ${RESPONSE_TIME}ms exceeds 500ms threshold"
            exit 1
          fi
      
      - name: Upload results
        if: always()
        uses: actions/upload-artifact@<SHA>
        with:
          name: load-test-results
          path: k6-results.json
```

### Performance Baseline Comparison

```yaml
- name: Run performance test
  run: npm run test:performance > current-results.json

- name: Download baseline
  uses: actions/download-artifact@<SHA>
  with:
    name: performance-baseline
    path: baseline/

- name: Compare with baseline
  run: |
    python scripts/compare-performance.py \
      --baseline baseline/results.json \
      --current current-results.json \
      --threshold 10  # Allow 10% degradation

- name: Upload new baseline
  if: github.ref == 'refs/heads/main'
  uses: actions/upload-artifact@<SHA>
  with:
    name: performance-baseline
    path: current-results.json
    retention-days: 90
```

## 5. Test Reporting and Visibility

### Publish Test Results as Annotations

```yaml
- name: Publish test results
  uses: EnricoMi/publish-unit-test-results-action@<SHA>
  if: always()
  with:
    files: |
      test-results/**/*.xml
      test-results/**/*.json
```

### Generate HTML Reports

```yaml
- name: Generate test report
  if: always()
  run: |
    npm run test:report
    # Generates HTML report at test-results/report.html

- name: Upload HTML report
  if: always()
  uses: actions/upload-artifact@<SHA>
  with:
    name: test-report-html
    path: test-results/report.html

- name: Comment PR with test results
  if: github.event_name == 'pull_request'
  uses: actions/github-script@<SHA>
  with:
    script: |
      const fs = require('fs');
      const summary = fs.readFileSync('test-results/summary.txt', 'utf8');
      github.rest.issues.createComment({
        issue_number: context.issue.number,
        owner: context.repo.owner,
        repo: context.repo.repo,
        body: `## Test Results\n\n${summary}`
      });
```

### Status Badges

Add to README.md:
```markdown
![Tests](https://github.com/owner/repo/actions/workflows/test.yml/badge.svg)
![Coverage](https://codecov.io/gh/owner/repo/branch/main/graph/badge.svg)
```

## 6. Comprehensive Testing Workflow

Combining all test types:

```yaml
name: Comprehensive Tests

on:
  push:
    branches: [main, develop]
  pull_request:
  schedule:
    - cron: '0 2 * * *'  # Nightly at 2 AM
  workflow_dispatch:

permissions:
  contents: read
  checks: write
  pull-requests: write

jobs:
  unit-test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      - uses: actions/setup-node@<SHA>
        with:
          node-version: 18
      - run: npm ci
      - run: npm run test:unit
      - uses: actions/upload-artifact@<SHA>
        if: always()
        with:
          name: unit-test-results
          path: test-results/unit/

  integration-test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
        ports:
          - 5432:5432
    steps:
      - uses: actions/checkout@<SHA>
      - uses: actions/setup-node@<SHA>
        with:
          node-version: 18
      - run: npm ci
      - run: npm run test:integration
        env:
          DATABASE_URL: postgresql://postgres:postgres@localhost:5432/testdb
      - uses: actions/upload-artifact@<SHA>
        if: always()
        with:
          name: integration-test-results
          path: test-results/integration/

  e2e-test:
    needs: [unit-test, integration-test]
    runs-on: ubuntu-latest
    if: github.event_name != 'schedule'  # Skip E2E on nightly runs
    steps:
      - uses: actions/checkout@<SHA>
      - uses: actions/setup-node@<SHA>
        with:
          node-version: 18
      - run: npm ci
      - run: npx playwright install chromium --with-deps
      - run: npm run test:e2e
      - uses: actions/upload-artifact@<SHA>
        if: always()
        with:
          name: e2e-test-results
          path: playwright-report/

  performance-test:
    runs-on: ubuntu-latest
    if: github.event_name == 'schedule' || github.event_name == 'workflow_dispatch'
    steps:
      - uses: actions/checkout@<SHA>
      - run: npm run test:performance
      - uses: actions/upload-artifact@<SHA>
        with:
          name: performance-results
          path: performance-results.json

  publish-results:
    needs: [unit-test, integration-test, e2e-test]
    if: always()
    runs-on: ubuntu-latest
    steps:
      - uses: actions/download-artifact@<SHA>
        with:
          path: all-results/
      
      - name: Publish combined results
        uses: EnricoMi/publish-unit-test-results-action@<SHA>
        with:
          files: all-results/**/*.xml
```

## Testing Best Practices Checklist

- [ ] Unit tests run on every push/PR (fast feedback)
- [ ] Integration tests use real dependencies via `services`
- [ ] E2E tests run against staging environment
- [ ] Test reports published as artifacts
- [ ] Test results annotated in PRs
- [ ] Code coverage tracked and enforced
- [ ] Flaky tests use retries or explicit waits
- [ ] Performance tests run regularly with baseline comparison
- [ ] Test execution time monitored and optimized
- [ ] Screenshots/videos captured on E2E test failure

## Resources

- [Playwright Documentation](https://playwright.dev)
- [Cypress Documentation](https://docs.cypress.io)
- [k6 Documentation](https://k6.io/docs)
- [Jest Documentation](https://jestjs.io)
- [GitHub Actions Services](https://docs.github.com/en/actions/using-containerized-services)

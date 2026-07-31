# Deployment Strategies and Best Practices

## Deployment Environment Structure

### Environment Hierarchy

```
Development → Staging → Production
    ↓           ↓           ↓
  feature   pre-prod    live traffic
  testing   validation   customers
```

## 1. GitHub Environments

Configure environments with protection rules, secrets, and approvals.

### Basic Environment Configuration

```yaml
jobs:
  deploy-staging:
    runs-on: ubuntu-latest
    environment:
      name: staging
      url: https://staging.example.com
    steps:
      - uses: actions/checkout@<SHA>
      - name: Deploy to staging
        run: ./deploy.sh staging

  deploy-production:
    needs: deploy-staging
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://example.com
    steps:
      - uses: actions/checkout@<SHA>
      - name: Deploy to production
        run: ./deploy.sh production
```

### Environment Protection Rules

Configure in GitHub: **Settings → Environments → [Environment Name]**

**Protection rules**:
- **Required reviewers**: Specific people/teams must approve
- **Wait timer**: Delay deployment by specified minutes
- **Deployment branches**: Restrict which branches can deploy
- **Environment secrets**: Secrets specific to this environment

### Environment-Specific Configuration

```yaml
deploy:
  environment: ${{ github.ref == 'refs/heads/main' && 'production' || 'staging' }}
  steps:
    - name: Set environment variables
      run: |
        if [ "${{ github.ref }}" == "refs/heads/main" ]; then
          echo "API_URL=https://api.example.com" >> $GITHUB_ENV
          echo "CDN_URL=https://cdn.example.com" >> $GITHUB_ENV
        else
          echo "API_URL=https://api.staging.example.com" >> $GITHUB_ENV
          echo "CDN_URL=https://cdn.staging.example.com" >> $GITHUB_ENV
        fi
```

## 2. Deployment Strategies

### Rolling Update (Default/Standard)

Gradually replace instances with new version. Good for stateless applications.

```yaml
deploy-rolling:
  runs-on: ubuntu-latest
  environment: production
  steps:
    - uses: actions/checkout@<SHA>
    
    - name: Deploy with rolling update
      run: |
        kubectl set image deployment/myapp \
          myapp=myapp:${{ github.sha }} \
          --record
        
        kubectl rollout status deployment/myapp
```

**Kubernetes configuration**:
```yaml
# deployment.yaml
spec:
  replicas: 10
  strategy:
    type: RollingUpdate
    rollingUpdate:
      maxSurge: 2        # Max 2 new pods above desired count
      maxUnavailable: 1  # Max 1 pod can be unavailable
```

**Pros**:
- No downtime
- Gradual rollout
- Easy rollback

**Cons**:
- Mixed versions during rollout
- Can't easily A/B test

### Blue/Green Deployment

Deploy new version alongside old, switch traffic completely.

```yaml
deploy-blue-green:
  runs-on: ubuntu-latest
  environment: production
  steps:
    - uses: actions/checkout@<SHA>
    
    - name: Deploy green environment
      run: |
        # Deploy new version to "green" environment
        kubectl apply -f k8s/deployment-green.yaml
        
        # Wait for green to be ready
        kubectl wait --for=condition=available --timeout=300s \
          deployment/myapp-green
    
    - name: Run smoke tests on green
      run: |
        curl -f https://green.example.com/health || exit 1
    
    - name: Switch traffic to green
      run: |
        # Update service to point to green
        kubectl patch service myapp -p '{"spec":{"selector":{"version":"green"}}}'
        
        # Label green as "blue" for next deployment
        kubectl label deployment myapp-green version=blue --overwrite
    
    - name: Keep old blue for rollback
      run: |
        # Scale down old blue but keep it running
        kubectl scale deployment/myapp-blue --replicas=1
```

**Pros**:
- Instant rollback (switch back to blue)
- Zero downtime
- Full testing before switch

**Cons**:
- Requires 2x resources during deployment
- More complex infrastructure
- Database migrations need careful planning

### Canary Deployment

Gradually roll out to small percentage of users, monitor, then full rollout.

```yaml
deploy-canary:
  runs-on: ubuntu-latest
  environment: production
  steps:
    - uses: actions/checkout@<SHA>
    
    - name: Deploy canary (10% traffic)
      run: |
        kubectl apply -f k8s/canary-deployment.yaml
        
        # Update Istio VirtualService for 10% traffic split
        kubectl apply -f - <<EOF
        apiVersion: networking.istio.io/v1beta1
        kind: VirtualService
        metadata:
          name: myapp
        spec:
          hosts:
          - myapp.example.com
          http:
          - match:
            - headers:
                x-canary:
                  exact: "true"
            route:
            - destination:
                host: myapp-canary
                port:
                  number: 80
          - route:
            - destination:
                host: myapp-stable
                port:
                  number: 80
              weight: 90
            - destination:
                host: myapp-canary
                port:
                  number: 80
              weight: 10
        EOF
    
    - name: Monitor canary metrics
      run: |
        # Wait and monitor error rates
        sleep 300
        
        ERROR_RATE=$(curl -s "https://prometheus.example.com/api/v1/query?query=rate(http_requests_total{status=~'5..',deployment='canary'}[5m])" | jq '.data.result[0].value[1]')
        
        if (( $(echo "$ERROR_RATE > 0.01" | bc -l) )); then
          echo "Canary error rate too high: $ERROR_RATE"
          exit 1
        fi
    
    - name: Promote canary to stable
      if: success()
      run: |
        # Gradually increase canary traffic: 10% → 50% → 100%
        kubectl apply -f k8s/full-rollout.yaml
```

**Pros**:
- Minimal blast radius if issues
- Real user testing
- Gradual confidence building

**Cons**:
- Complex setup (requires service mesh or load balancer)
- Longer deployment time
- Requires sophisticated monitoring

### Feature Flags / Dark Launch

Deploy code but keep features hidden until toggled on.

```yaml
deploy-with-feature-flags:
  runs-on: ubuntu-latest
  environment: production
  steps:
    - uses: actions/checkout@<SHA>
    
    - name: Deploy new version
      run: |
        kubectl set image deployment/myapp \
          myapp=myapp:${{ github.sha }}
    
    - name: Enable feature for internal users
      run: |
        curl -X POST https://featureflags.example.com/api/features \
          -H "Authorization: Bearer ${{ secrets.FEATURE_FLAG_API_KEY }}" \
          -d '{
            "feature": "new-checkout-flow",
            "enabled": true,
            "rules": [
              {"attribute": "email", "operator": "endsWith", "value": "@example.com"}
            ]
          }'
    
    - name: Monitor metrics
      run: |
        # Monitor for 1 hour with internal users
        sleep 3600
    
    - name: Gradual rollout to users
      run: |
        # Enable for 10% of users
        curl -X PATCH https://featureflags.example.com/api/features/new-checkout-flow \
          -d '{"percentage": 10}'
```

**Pros**:
- Decouple deployment from release
- Granular control over feature exposure
- Easy rollback (just toggle off)
- A/B testing capability

**Cons**:
- Requires feature flag infrastructure
- Code complexity (feature flag checks)
- Technical debt if flags not cleaned up

## 3. Rollback Strategies

### Automatic Rollback on Health Check Failure

```yaml
deploy-with-rollback:
  runs-on: ubuntu-latest
  environment: production
  steps:
    - uses: actions/checkout@<SHA>
    
    - name: Deploy new version
      id: deploy
      run: |
        kubectl set image deployment/myapp \
          myapp=myapp:${{ github.sha }} \
          --record
        
        # Wait for rollout
        kubectl rollout status deployment/myapp --timeout=5m
    
    - name: Run post-deployment health checks
      id: health_check
      run: |
        # Wait for pods to stabilize
        sleep 30
        
        # Check health endpoint
        for i in {1..5}; do
          if curl -f https://example.com/health; then
            echo "Health check passed"
            exit 0
          fi
          sleep 10
        done
        echo "Health checks failed"
        exit 1
    
    - name: Automatic rollback on failure
      if: failure() && steps.deploy.conclusion == 'success'
      run: |
        echo "Health checks failed, rolling back..."
        kubectl rollout undo deployment/myapp
        kubectl rollout status deployment/myapp
        
        # Send alert
        curl -X POST ${{ secrets.SLACK_WEBHOOK }} \
          -d '{"text": "🚨 Deployment failed and was automatically rolled back"}'
```

### Manual Rollback Workflow

```yaml
name: Rollback Production

on:
  workflow_dispatch:
    inputs:
      revision:
        description: 'Revision number to rollback to (leave empty for previous)'
        required: false
        type: string
      reason:
        description: 'Reason for rollback'
        required: true
        type: string

permissions:
  contents: read

jobs:
  rollback:
    runs-on: ubuntu-latest
    environment: production  # Requires approval
    steps:
      - name: Rollback deployment
        run: |
          if [ -n "${{ inputs.revision }}" ]; then
            kubectl rollout undo deployment/myapp --to-revision=${{ inputs.revision }}
          else
            kubectl rollout undo deployment/myapp
          fi
          
          kubectl rollout status deployment/myapp
      
      - name: Verify rollback
        run: |
          sleep 30
          curl -f https://example.com/health
      
      - name: Post rollback notification
        run: |
          curl -X POST ${{ secrets.SLACK_WEBHOOK }} \
            -d '{
              "text": "⚠️ Production rolled back",
              "blocks": [
                {
                  "type": "section",
                  "text": {
                    "type": "mrkdwn",
                    "text": "*Rollback performed*\nReason: ${{ inputs.reason }}\nBy: ${{ github.actor }}"
                  }
                }
              ]
            }'
```

### Version Tagging for Easy Rollback

```yaml
deploy-with-versioning:
  runs-on: ubuntu-latest
  steps:
    - uses: actions/checkout@<SHA>
    
    - name: Generate version tag
      id: version
      run: |
        VERSION="v$(date +%Y%m%d)-${{ github.run_number }}"
        echo "tag=$VERSION" >> $GITHUB_OUTPUT
    
    - name: Build and tag image
      run: |
        docker build -t myapp:${{ steps.version.outputs.tag }} .
        docker tag myapp:${{ steps.version.outputs.tag }} myapp:latest
        docker push myapp:${{ steps.version.outputs.tag }}
        docker push myapp:latest
    
    - name: Deploy versioned image
      run: |
        kubectl set image deployment/myapp \
          myapp=myapp:${{ steps.version.outputs.tag }}
    
    - name: Create Git tag
      run: |
        git tag ${{ steps.version.outputs.tag }}
        git push origin ${{ steps.version.outputs.tag }}
```

## 4. Database Migrations in Deployments

### Safe Migration Strategy

```yaml
deploy-with-migrations:
  runs-on: ubuntu-latest
  environment: production
  steps:
    - uses: actions/checkout@<SHA>
    
    - name: Backup database
      run: |
        pg_dump ${{ secrets.DATABASE_URL }} > backup-$(date +%Y%m%d-%H%M%S).sql
    
    - name: Run migrations
      run: |
        npm run migrate
      env:
        DATABASE_URL: ${{ secrets.DATABASE_URL }}
    
    - name: Deploy application
      run: |
        kubectl set image deployment/myapp myapp=myapp:${{ github.sha }}
    
    - name: Verify deployment
      run: |
        kubectl rollout status deployment/myapp
        curl -f https://example.com/health
```

### Backwards-Compatible Migrations

**Phase 1: Add new column (backwards compatible)**
```sql
-- Migration: Add new column with default value
ALTER TABLE users ADD COLUMN email_verified BOOLEAN DEFAULT false;
```

**Phase 2: Deploy code that writes to both old and new columns**
```yaml
deploy: # Application now writes to email_verified column
```

**Phase 3: Backfill data**
```sql
UPDATE users SET email_verified = true WHERE email_confirmation_token IS NULL;
```

**Phase 4: Deploy code that only uses new column**

**Phase 5: Remove old column**
```sql
ALTER TABLE users DROP COLUMN email_confirmation_token;
```

## 5. Multi-Region Deployments

```yaml
deploy-multi-region:
  runs-on: ubuntu-latest
  environment: production
  strategy:
    matrix:
      region: [us-east-1, eu-west-1, ap-southeast-1]
    max-parallel: 1  # Deploy one region at a time
  steps:
    - uses: actions/checkout@<SHA>
    
    - name: Configure AWS credentials
      uses: aws-actions/configure-aws-credentials@<SHA>
      with:
        role-to-assume: ${{ secrets.AWS_ROLE_ARN }}
        aws-region: ${{ matrix.region }}
    
    - name: Deploy to ${{ matrix.region }}
      run: |
        kubectl config use-context ${{ matrix.region }}
        kubectl set image deployment/myapp myapp=myapp:${{ github.sha }}
        kubectl rollout status deployment/myapp
    
    - name: Health check ${{ matrix.region }}
      run: |
        curl -f https://${{ matrix.region }}.example.com/health
    
    - name: Wait before next region
      if: ${{ matrix.region != 'ap-southeast-1' }}
      run: sleep 300  # 5 min soak time between regions
```

## 6. Deployment Approval Workflow

```yaml
name: Production Deployment

on:
  push:
    branches: [main]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@<SHA>
      - run: npm run build
      - uses: actions/upload-artifact@<SHA>
        with:
          name: build
          path: dist/

  request-approval:
    needs: build
    runs-on: ubuntu-latest
    steps:
      - name: Create deployment issue
        uses: actions/github-script@<SHA>
        with:
          script: |
            const issue = await github.rest.issues.create({
              owner: context.repo.owner,
              repo: context.repo.repo,
              title: `Deploy ${context.sha.substring(0, 7)} to production`,
              body: `
              ## Deployment Request
              
              **Commit**: ${context.sha}
              **Author**: ${context.actor}
              **Workflow**: ${context.runNumber}
              
              Review changes and approve deployment by commenting "/approve"
              `,
              labels: ['deployment-request']
            });
            
            core.setOutput('issue_number', issue.data.number);

  deploy-production:
    needs: build
    runs-on: ubuntu-latest
    environment:
      name: production
      url: https://example.com
    steps:
      - uses: actions/download-artifact@<SHA>
        with:
          name: build
          path: dist/
      
      - name: Deploy to production
        run: ./deploy.sh production
```

## Deployment Checklist

- [ ] Staging environment configured and tested
- [ ] Production environment has manual approval gate
- [ ] Rollback strategy documented and tested
- [ ] Database migrations are backwards-compatible
- [ ] Post-deployment health checks automated
- [ ] Monitoring and alerting configured
- [ ] Deployment notifications sent to team
- [ ] Version tagging for easy rollback reference
- [ ] Deployment windows defined for production
- [ ] Incident response runbook prepared

## Resources

- [GitHub Environments Documentation](https://docs.github.com/en/actions/deployment/targeting-different-environments)
- [Kubernetes Deployment Strategies](https://kubernetes.io/docs/concepts/workloads/controllers/deployment/)
- [Istio Traffic Management](https://istio.io/latest/docs/concepts/traffic-management/)
- [Feature Flag Best Practices](https://martinfowler.com/articles/feature-toggles.html)

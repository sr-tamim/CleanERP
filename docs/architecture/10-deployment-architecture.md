# Deployment Architecture

## Overview

The GoldenFiberERP system is designed with a containerized, cloud-ready deployment architecture that supports multiple deployment scenarios from development to enterprise production environments. This document outlines the deployment strategies, infrastructure requirements, and operational considerations.

## Deployment Environments

### Development Environment

**Purpose**: Local development and testing
**Infrastructure**: Developer workstations with Docker Desktop

**Components**:
```yaml
# docker-compose.yml (Development)
version: '3.8'
services:
  goldenfibererp.api:
    build:
      context: src/Presentation/GoldenFiberERP.API
      dockerfile: Dockerfile
    ports:
      - "8080:8080"
      - "8081:8081"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgresql;Database=GoldenFiberERP_Dev;Username=postgres;Password=YourPassword123;
    depends_on:
      - postgresql
      - redis

  postgresql:
    image: postgres:16-alpine
    environment:
      - POSTGRES_DB=GoldenFiberERP_Dev
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=YourPassword123
    ports:
      - "5432:5432"
    volumes:
      - postgresql_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"
    volumes:
      - redis_data:/data

volumes:
  postgresql_data:
  redis_data:
```

**Features**:
- Hot reload for development
- Integrated debugging support
- File watching for automatic rebuilds
- Development-specific logging levels

### Staging Environment

**Purpose**: Pre-production testing and validation
**Infrastructure**: Cloud-based or dedicated staging servers

**Architecture**:
```
┌─────────────────────────────────────────────────────────┐
│                Load Balancer                            │
├─────────────────────────────────────────────────────────┤
│                    Web Tier                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   API Pod   │  │   API Pod   │  │   API Pod   │     │
│  │  (Replica)  │  │  (Replica)  │  │  (Replica)  │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
├─────────────────────────────────────────────────────────┤
│                   Data Tier                             │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │ PostgreSQL  │  │    Redis    │  │ File Storage│     │
│  │  (Primary)  │  │   (Cache)   │  │   (Blob)    │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
└─────────────────────────────────────────────────────────┘
```

### Production Environment

**Purpose**: Live system serving end users
**Infrastructure**: High-availability cloud deployment

**Architecture**:
```
┌─────────────────────────────────────────────────────────┐
│                  CDN / WAF                              │
├─────────────────────────────────────────────────────────┤
│               Application Gateway                       │
├─────────────────────────────────────────────────────────┤
│                   Web Tier                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   API Pod   │  │   API Pod   │  │   API Pod   │     │
│  │  (Zone A)   │  │  (Zone B)   │  │  (Zone C)   │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
├─────────────────────────────────────────────────────────┤
│                  Data Tier                              │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │ PostgreSQL  │  │    Redis    │  │ File Storage│     │
│  │(Primary/HA) │  │  (Cluster)  │  │ (Redundant) │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
├─────────────────────────────────────────────────────────┤
│                Monitoring Tier                          │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │
│  │   Logging   │  │ Monitoring  │  │   Alerts    │     │
│  │ (Centralized│  │ (Metrics)   │  │ (Automated) │     │
│  └─────────────┘  └─────────────┘  └─────────────┘     │
└─────────────────────────────────────────────────────────┘
```

## Container Strategy

### Docker Image Optimization

#### Multi-Stage Dockerfile
```dockerfile
# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy and restore projects
COPY ["src/Presentation/GoldenFiberERP.API/GoldenFiberERP.API.csproj", "src/Presentation/GoldenFiberERP.API/"]
COPY ["src/Core/GoldenFiberERP.Application/GoldenFiberERP.Application.csproj", "src/Core/GoldenFiberERP.Application/"]
COPY ["src/Core/GoldenFiberERP.Domain/GoldenFiberERP.Domain.csproj", "src/Core/GoldenFiberERP.Domain/"]
COPY ["src/Infrastructure/GoldenFiberERP.Infrastructure/GoldenFiberERP.Infrastructure.csproj", "src/Infrastructure/GoldenFiberERP.Infrastructure/"]

RUN dotnet restore "src/Presentation/GoldenFiberERP.API/GoldenFiberERP.API.csproj"

# Copy source code and build
COPY . .
WORKDIR "/src/src/Presentation/GoldenFiberERP.API"
RUN dotnet build "GoldenFiberERP.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "GoldenFiberERP.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN adduser --disabled-password --gecos '' appuser
USER appuser

# Copy published application
COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

EXPOSE 8080
ENTRYPOINT ["dotnet", "GoldenFiberERP.API.dll"]
```

#### Image Optimization Features
- **Multi-stage builds**: Minimize final image size
- **Layer caching**: Optimize build times
- **Non-root user**: Enhanced security
- **Health checks**: Container health monitoring
- **Minimal base images**: Reduced attack surface

### Container Registry Strategy

#### Private Registry Structure
```
registry.company.com/
├── goldenfibererp/
│   ├── api:latest
│   ├── api:v1.0.0
│   ├── api:develop
│   └── api:feature-xyz
├── infrastructure/
│   ├── migrations:latest
│   └── seed-data:latest
└── tools/
    ├── backup:latest
    └── monitor:latest
```

#### Image Tagging Strategy
- **latest**: Current production version
- **v{major}.{minor}.{patch}**: Semantic versioning
- **develop**: Latest development build
- **feature-{name}**: Feature branch builds
- **{commit-sha}**: Git commit-specific builds

## Kubernetes Deployment

### Namespace Organization
```yaml
# Namespace definitions
apiVersion: v1
kind: Namespace
metadata:
  name: goldenfibererp-prod
---
apiVersion: v1
kind: Namespace
metadata:
  name: goldenfibererp-staging
---
apiVersion: v1
kind: Namespace
metadata:
  name: goldenfibererp-dev
```

### Application Deployment
```yaml
# api-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: goldenfibererp-api
  namespace: goldenfibererp-prod
spec:
  replicas: 3
  selector:
    matchLabels:
      app: goldenfibererp-api
  template:
    metadata:
      labels:
        app: goldenfibererp-api
    spec:
      containers:
      - name: api
        image: registry.company.com/goldenfibererp/api:v1.0.0
        ports:
        - containerPort: 8080
        env:
        - name: ASPNETCORE_ENVIRONMENT
          value: "Production"
        - name: ConnectionStrings__DefaultConnection
          valueFrom:
            secretKeyRef:
              name: database-secret
              key: connection-string
        resources:
          requests:
            memory: "256Mi"
            cpu: "250m"
          limits:
            memory: "512Mi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: goldenfibererp-api-service
  namespace: goldenfibererp-prod
spec:
  selector:
    app: goldenfibererp-api
  ports:
  - protocol: TCP
    port: 80
    targetPort: 8080
  type: ClusterIP
```

### Database Deployment
```yaml
# database-deployment.yaml
apiVersion: apps/v1
kind: StatefulSet
metadata:
  name: postgresql
  namespace: goldenfibererp-prod
spec:
  serviceName: postgresql-service
  replicas: 1
  selector:
    matchLabels:
      app: postgresql
  template:
    metadata:
      labels:
        app: postgresql
    spec:
      containers:
      - name: postgresql
        image: postgres:16-alpine
        env:
        - name: POSTGRES_DB
          value: "GoldenFiberERP"
        - name: POSTGRES_USER
          value: "postgres"
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: database-secret
              key: postgres-password
        ports:
        - containerPort: 5432
        volumeMounts:
        - name: postgresql-storage
          mountPath: /var/lib/postgresql/data
        resources:
          requests:
            memory: "2Gi"
            cpu: "1"
          limits:
            memory: "4Gi"
            cpu: "2"
  volumeClaimTemplates:
  - metadata:
      name: postgresql-storage
    spec:
      accessModes: ["ReadWriteOnce"]
      resources:
        requests:
          storage: 50Gi
```

### Ingress Configuration
```yaml
# ingress.yaml
apiVersion: networking.k8s.io/v1
kind: Ingress
metadata:
  name: goldenfibererp-ingress
  namespace: goldenfibererp-prod
  annotations:
    nginx.ingress.kubernetes.io/ssl-redirect: "true"
    nginx.ingress.kubernetes.io/use-regex: "true"
    cert-manager.io/cluster-issuer: "letsencrypt-prod"
spec:
  tls:
  - hosts:
    - api.goldenfibererp.com
    secretName: goldenfibererp-tls
  rules:
  - host: api.goldenfibererp.com
    http:
      paths:
      - path: /
        pathType: Prefix
        backend:
          service:
            name: goldenfibererp-api-service
            port:
              number: 80
```

## Cloud Deployment Options

### Azure Deployment

#### Azure Container Apps
```yaml
# azure-container-app.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: container-app-config
data:
  container-app.yaml: |
    location: East US
    resourceGroup: goldenfibererp-prod
    containerAppEnvironment: goldenfibererp-env
    app:
      name: goldenfibererp-api
      image: registry.company.com/goldenfibererp/api:latest
      replicas:
        min: 2
        max: 10
      resources:
        cpu: 0.5
        memory: 1Gi
      ingress:
        external: true
        targetPort: 8080
        customDomains:
          - name: api.goldenfibererp.com
            certificateId: /subscriptions/.../certificates/ssl-cert
```

#### Azure App Service
```yaml
# azure-app-service.yaml
apiVersion: web.azure.com/v1
kind: AppService
metadata:
  name: goldenfibererp-api
spec:
  location: East US
  resourceGroup: goldenfibererp-prod
  appServicePlan: goldenfibererp-plan
  containerSettings:
    registry: registry.company.com
    image: goldenfibererp/api
    tag: latest
  appSettings:
    - name: ASPNETCORE_ENVIRONMENT
      value: Production        - name: ConnectionStrings__DefaultConnection
          secretRef: database-connection-string
```

### AWS Deployment

#### ECS Fargate
```json
{
  "family": "goldenfibererp-api",
  "networkMode": "awsvpc",
  "requiresCompatibilities": ["FARGATE"],
  "cpu": "512",
  "memory": "1024",
  "executionRoleArn": "arn:aws:iam::account:role/ecsTaskExecutionRole",
  "taskRoleArn": "arn:aws:iam::account:role/ecsTaskRole",
  "containerDefinitions": [
    {
      "name": "goldenfibererp-api",
      "image": "registry.company.com/goldenfibererp/api:latest",
      "portMappings": [
        {
          "containerPort": 8080,
          "protocol": "tcp"
        }
      ],
      "environment": [
        {
          "name": "ASPNETCORE_ENVIRONMENT",
          "value": "Production"
        }
      ],
      "secrets": [
        {
          "name": "ConnectionStrings__DefaultConnection",
          "valueFrom": "arn:aws:secretsmanager:region:account:secret:database-connection"
        }
      ],
      "logConfiguration": {
        "logDriver": "awslogs",
        "options": {
          "awslogs-group": "/ecs/goldenfibererp-api",
          "awslogs-region": "us-east-1",
          "awslogs-stream-prefix": "ecs"
        }
      }
    }
  ]
}
```

## CI/CD Pipeline

### GitHub Actions Workflow
```yaml
# .github/workflows/deploy.yml
name: Deploy GoldenFiberERP

on:
  push:
    branches: [main, develop]
  pull_request:
    branches: [main]

env:
  REGISTRY: ghcr.io
  IMAGE_NAME: goldenfibererp/api

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --configuration Release --no-restore
    
    - name: Test
      run: dotnet test --no-build --verbosity normal --collect:"XPlat Code Coverage"
    
    - name: Upload coverage reports
      uses: codecov/codecov-action@v3

  build-and-push:
    needs: test
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main' || github.ref == 'refs/heads/develop'
    
    steps:
    - name: Checkout
      uses: actions/checkout@v4
    
    - name: Login to Container Registry
      uses: docker/login-action@v3
      with:
        registry: ${{ env.REGISTRY }}
        username: ${{ github.actor }}
        password: ${{ secrets.GITHUB_TOKEN }}
    
    - name: Extract metadata
      id: meta
      uses: docker/metadata-action@v5
      with:
        images: ${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}
        tags: |
          type=ref,event=branch
          type=semver,pattern={{version}}
          type=sha
    
    - name: Build and push
      uses: docker/build-push-action@v5
      with:
        context: .
        file: src/Presentation/GoldenFiberERP.API/Dockerfile
        push: true
        tags: ${{ steps.meta.outputs.tags }}
        labels: ${{ steps.meta.outputs.labels }}

  deploy-staging:
    needs: build-and-push
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/develop'
    environment: staging
    
    steps:
    - name: Deploy to Staging
      run: |
        # Deploy to staging environment
        kubectl set image deployment/goldenfibererp-api \
          api=${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }} \
          -n goldenfibererp-staging

  deploy-production:
    needs: build-and-push
    runs-on: ubuntu-latest
    if: github.ref == 'refs/heads/main'
    environment: production
    
    steps:
    - name: Deploy to Production
      run: |
        # Deploy to production environment
        kubectl set image deployment/goldenfibererp-api \
          api=${{ env.REGISTRY }}/${{ env.IMAGE_NAME }}:${{ github.sha }} \
          -n goldenfibererp-prod
```

## PostgreSQL Production Configuration

### High Availability Setup
```yaml
# postgresql-ha-deployment.yaml
apiVersion: postgresql.cnpg.io/v1
kind: Cluster
metadata:
  name: postgresql-cluster
  namespace: goldenfibererp-prod
spec:
  instances: 3
  
  postgresql:
    parameters:
      max_connections: "200"
      shared_buffers: "256MB"
      effective_cache_size: "1GB"
      maintenance_work_mem: "64MB"
      checkpoint_completion_target: "0.9"
      wal_buffers: "16MB"
      default_statistics_target: "100"
      random_page_cost: "1.1"
      effective_io_concurrency: "200"
      work_mem: "4MB"
      min_wal_size: "1GB"
      max_wal_size: "4GB"
      
  bootstrap:
    initdb:
      database: GoldenFiberERP
      owner: goldenfibererp_user
      secret:
        name: postgresql-credentials
        
  storage:
    size: 100Gi
    storageClass: fast-ssd
    
  resources:
    requests:
      memory: "2Gi"
      cpu: "1"
    limits:
      memory: "4Gi"
      cpu: "2"
      
  monitoring:
    enabled: true
    podMonitor:
      enabled: true
      
  backup:
    retentionPolicy: "30d"
    data:
      compression: gzip
      encryption: AES256
---
apiVersion: v1
kind: Secret
metadata:
  name: postgresql-credentials
  namespace: goldenfibererp-prod
type: Opaque
stringData:
  username: goldenfibererp_user
  password: "your-secure-password"
  postgres-password: "postgres-admin-password"
```

### PostgreSQL Connection Pooling
```yaml
# pgbouncer-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: pgbouncer
  namespace: goldenfibererp-prod
spec:
  replicas: 2
  selector:
    matchLabels:
      app: pgbouncer
  template:
    metadata:
      labels:
        app: pgbouncer
    spec:
      containers:
      - name: pgbouncer
        image: pgbouncer/pgbouncer:latest
        env:
        - name: DATABASES_HOST
          value: postgresql-cluster-rw
        - name: DATABASES_PORT
          value: "5432"
        - name: DATABASES_USER
          valueFrom:
            secretKeyRef:
              name: postgresql-credentials
              key: username
        - name: DATABASES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: postgresql-credentials
              key: password
        - name: DATABASES_DBNAME
          value: GoldenFiberERP
        - name: POOL_MODE
          value: transaction
        - name: SERVER_RESET_QUERY
          value: DISCARD ALL
        - name: MAX_CLIENT_CONN
          value: "1000"
        - name: DEFAULT_POOL_SIZE
          value: "25"
        - name: MIN_POOL_SIZE
          value: "5"
        - name: RESERVE_POOL_SIZE
          value: "5"
        - name: RESERVE_POOL_TIMEOUT
          value: "5"
        - name: MAX_DB_CONNECTIONS
          value: "100"
        ports:
        - containerPort: 5432
        resources:
          requests:
            memory: "64Mi"
            cpu: "100m"
          limits:
            memory: "128Mi"
            cpu: "200m"
---
apiVersion: v1
kind: Service
metadata:
  name: pgbouncer-service
  namespace: goldenfibererp-prod
spec:
  selector:
    app: pgbouncer
  ports:
  - protocol: TCP
    port: 5432
    targetPort: 5432
  type: ClusterIP
```

### PostgreSQL Monitoring and Metrics
```yaml
# postgresql-monitoring.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: postgres-exporter-config
  namespace: goldenfibererp-prod
data:
  queries.yaml: |
    pg_database:
      query: "SELECT pg_database.datname, pg_database_size(pg_database.datname) as size FROM pg_database"
      master: true
      metrics:
        - datname:
            usage: "LABEL"
            description: "Name of the database"
        - size:
            usage: "GAUGE"
            description: "Disk space used by the database"
    
    pg_stat_user_tables:
      query: "SELECT schemaname, tablename, n_tup_ins, n_tup_upd, n_tup_del, n_live_tup, n_dead_tup FROM pg_stat_user_tables"
      master: true
      metrics:
        - schemaname:
            usage: "LABEL"
            description: "Name of the schema"
        - tablename:
            usage: "LABEL"
            description: "Name of the table"
        - n_tup_ins:
            usage: "COUNTER"
            description: "Number of rows inserted"
        - n_tup_upd:
            usage: "COUNTER"
            description: "Number of rows updated"
        - n_tup_del:
            usage: "COUNTER"
            description: "Number of rows deleted"
        - n_live_tup:
            usage: "GAUGE"
            description: "Number of live rows"
        - n_dead_tup:
            usage: "GAUGE"
            description: "Number of dead rows"
---
apiVersion: apps/v1
kind: Deployment
metadata:
  name: postgres-exporter
  namespace: goldenfibererp-prod
spec:
  replicas: 1
  selector:
    matchLabels:
      app: postgres-exporter
  template:
    metadata:
      labels:
        app: postgres-exporter
    spec:
      containers:
      - name: postgres-exporter
        image: prometheuscommunity/postgres-exporter:latest
        env:
        - name: DATA_SOURCE_NAME
          value: "postgresql://goldenfibererp_user:$(POSTGRES_PASSWORD)@postgresql-cluster-rw:5432/GoldenFiberERP?sslmode=require"
        - name: POSTGRES_PASSWORD
          valueFrom:
            secretKeyRef:
              name: postgresql-credentials
              key: password
        - name: PG_EXPORTER_EXTEND_QUERY_PATH
          value: "/etc/postgres_exporter/queries.yaml"
        ports:
        - containerPort: 9187
        volumeMounts:
        - name: queries
          mountPath: /etc/postgres_exporter
        resources:
          requests:
            memory: "32Mi"
            cpu: "50m"
          limits:
            memory: "64Mi"
            cpu: "100m"
      volumes:
      - name: queries
        configMap:
          name: postgres-exporter-config
```

## Database Deployment Strategy

### Migration Management
```csharp
// Database migration in startup with PostgreSQL considerations
public static async Task<WebApplication> MigrateDatabaseAsync(this WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Check database connection
        var canConnect = await context.Database.CanConnectAsync();
        if (!canConnect)
        {
            logger.LogError("Cannot connect to PostgreSQL database");
            throw new InvalidOperationException("Database connection failed");
        }
        
        if (app.Environment.IsProduction())
        {
            // Production: Apply migrations cautiously with transaction
            var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
            if (pendingMigrations.Any())
            {
                logger.LogInformation("Applying {Count} pending migrations to PostgreSQL", pendingMigrations.Count());
                
                // Use transaction for migration safety
                await using var transaction = await context.Database.BeginTransactionAsync();
                try
                {
                    await context.Database.MigrateAsync();
                    await transaction.CommitAsync();
                    logger.LogInformation("Migrations applied successfully");
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }
        else
        {
            // Development/Staging: Auto-migrate
            await context.Database.MigrateAsync();
            
            // Ensure PostgreSQL extensions are created
            await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS \"uuid-ossp\";");
            await context.Database.ExecuteSqlRawAsync("CREATE EXTENSION IF NOT EXISTS \"pg_trgm\";");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during PostgreSQL database migration");
        throw;
    }
    
    return app;
}

// PostgreSQL-specific migration utilities
public static class PostgreSqlMigrationExtensions
{
    public static async Task EnsurePostgreSqlExtensionsAsync(this ApplicationDbContext context)
    {
        var extensions = new[]
        {
            "uuid-ossp",    // UUID generation functions
            "pg_trgm",      // Trigram matching for full-text search
            "pgcrypto",     // Cryptographic functions
            "hstore"        // Key-value storage
        };
        
        foreach (var extension in extensions)
        {
            await context.Database.ExecuteSqlRawAsync($"CREATE EXTENSION IF NOT EXISTS \"{extension}\";");
        }
    }
    
    public static async Task CreateIndexesConcurrentlyAsync(this ApplicationDbContext context)
    {
        // Create indexes concurrently to avoid blocking in production
        var indexQueries = new[]
        {
            "CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_products_name_gin ON products USING gin(name gin_trgm_ops);",
            "CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_customers_email_unique ON customers(email) WHERE deleted_at IS NULL;",
            "CREATE INDEX CONCURRENTLY IF NOT EXISTS ix_orders_status_created_at ON orders(status, created_at);"
        };
        
        foreach (var query in indexQueries)
        {
            try
            {
                await context.Database.ExecuteSqlRawAsync(query);
            }
            catch (Exception ex)
            {
                // Log but don't fail migration for index creation
                var logger = context.GetService<ILogger<ApplicationDbContext>>();
                logger?.LogWarning(ex, "Failed to create index: {Query}", query);
            }
        }
    }
}
```

### Blue-Green Deployment
```yaml
# Blue-Green deployment strategy
apiVersion: argoproj.io/v1alpha1
kind: Rollout
metadata:
  name: goldenfibererp-api
spec:
  replicas: 3
  strategy:
    blueGreen:
      activeService: goldenfibererp-api-active
      previewService: goldenfibererp-api-preview
      autoPromotionEnabled: false
      scaleDownDelaySeconds: 30
      prePromotionAnalysis:
        templates:
        - templateName: success-rate
        args:
        - name: service-name
          value: goldenfibererp-api-preview
  selector:
    matchLabels:
      app: goldenfibererp-api
  template:
    metadata:
      labels:
        app: goldenfibererp-api
    spec:
      containers:
      - name: api
        image: registry.company.com/goldenfibererp/api:latest
```

## Monitoring and Observability

### Prometheus Metrics
```yaml
# prometheus-config.yaml
apiVersion: v1
kind: ConfigMap
metadata:
  name: prometheus-config
data:
  prometheus.yml: |
    global:
      scrape_interval: 15s
    scrape_configs:
    - job_name: 'goldenfibererp-api'
      static_configs:
      - targets: ['goldenfibererp-api-service:80']
      metrics_path: /metrics
      scrape_interval: 5s
```

### Grafana Dashboard
```json
{
  "dashboard": {
    "title": "GoldenFiberERP Metrics",
    "panels": [
      {
        "title": "Request Rate",
        "type": "graph",
        "targets": [
          {
            "expr": "rate(http_requests_total[5m])",
            "legendFormat": "{{method}} {{status}}"
          }
        ]
      },
      {
        "title": "Response Time",
        "type": "graph",
        "targets": [
          {
            "expr": "histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m]))",
            "legendFormat": "95th percentile"
          }
        ]
      }
    ]
  }
}
```

## Security Considerations

### Network Security
- **TLS encryption**: End-to-end encryption
- **Network policies**: Kubernetes network isolation
- **VPC/VNet**: Private network deployment
- **WAF**: Web application firewall

### Container Security
- **Image scanning**: Vulnerability assessment
- **Non-root users**: Principle of least privilege
- **Resource limits**: DoS protection
- **Secret management**: External secret stores

### Access Control
- **RBAC**: Role-based access control
- **Service accounts**: Minimal permissions
- **Pod security policies**: Container restrictions
- **Network policies**: Traffic isolation

## Backup and Disaster Recovery

### Database Backup Strategy
```bash
#!/bin/bash
# Automated backup script
BACKUP_PATH="/backups/$(date +%Y%m%d_%H%M%S)"
DB_NAME="GoldenFiberERP"
DB_USER="postgres"

# Create backup
kubectl exec -n goldenfibererp-prod postgresql-0 -- \
  pg_dump -h localhost -U $DB_USER -d $DB_NAME -f /var/lib/postgresql/backup/${DB_NAME}_$(date +%Y%m%d_%H%M%S).sql

# Upload to cloud storage
kubectl cp goldenfibererp-prod/postgresql-0:/var/lib/postgresql/backup/ $BACKUP_PATH
aws s3 sync $BACKUP_PATH s3://goldenfibererp-backups/database/
```

### Application State Backup
- **Configuration backups**: Store in version control
- **Certificate backups**: Secure storage with rotation
- **Log archival**: Long-term log retention
- **Disaster recovery testing**: Regular DR drills

## Performance Optimization

### Horizontal Pod Autoscaler
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: goldenfibererp-api-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: goldenfibererp-api
  minReplicas: 2
  maxReplicas: 10
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
```

### Caching Strategy
- **Application-level caching**: In-memory and distributed
- **CDN integration**: Static asset caching
- **Database query optimization**: Index tuning and query caching
- **API response caching**: HTTP caching headers

## PostgreSQL Performance Optimization

### Database Tuning
```sql
-- PostgreSQL performance configuration for production
-- postgresql.conf optimizations

-- Memory Settings
shared_buffers = 256MB                    # 25% of available RAM
effective_cache_size = 1GB               # 75% of available RAM
work_mem = 4MB                           # Memory for sort operations
maintenance_work_mem = 64MB              # Memory for maintenance operations

-- Checkpoint Settings
checkpoint_completion_target = 0.9      # Spread checkpoints over time
wal_buffers = 16MB                       # WAL buffer size
min_wal_size = 1GB                       # Minimum WAL file size
max_wal_size = 4GB                       # Maximum WAL file size

-- Planner Settings
random_page_cost = 1.1                   # SSD-optimized random page cost
effective_io_concurrency = 200           # Number of concurrent I/O operations

-- Connection Settings
max_connections = 200                     # Maximum concurrent connections
```

### Index Optimization
```sql
-- Create optimized indexes for common queries
CREATE INDEX CONCURRENTLY ix_products_category_active 
ON products(category, is_active) 
WHERE deleted_at IS NULL;

CREATE INDEX CONCURRENTLY ix_orders_customer_status_date 
ON orders(customer_id, status, created_at DESC);

CREATE INDEX CONCURRENTLY ix_order_items_product_id 
ON order_items(product_id) 
INCLUDE (quantity, unit_price);

-- Full-text search indexes
CREATE INDEX CONCURRENTLY ix_products_search 
ON products USING gin(to_tsvector('english', name || ' ' || description));

-- Partial indexes for better performance
CREATE INDEX CONCURRENTLY ix_products_active 
ON products(created_at DESC) 
WHERE is_active = true AND deleted_at IS NULL;
```

### Query Performance Monitoring
```csharp
// Infrastructure/Services/PostgreSqlPerformanceService.cs
public class PostgreSqlPerformanceService : IPerformanceService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PostgreSqlPerformanceService> _logger;

    public async Task<List<SlowQueryInfo>> GetSlowQueriesAsync()
    {
        var slowQueries = await _context.Database.SqlQueryRaw<SlowQueryInfo>(@"
            SELECT 
                query,
                calls,
                total_time,
                mean_time,
                rows,
                100.0 * shared_blks_hit / nullif(shared_blks_hit + shared_blks_read, 0) AS hit_percent
            FROM pg_stat_statements 
            ORDER BY total_time DESC 
            LIMIT 10").ToListAsync();

        return slowQueries;
    }

    public async Task<List<IndexUsageInfo>> GetIndexUsageAsync()
    {
        var indexUsage = await _context.Database.SqlQueryRaw<IndexUsageInfo>(@"
            SELECT 
                schemaname,
                tablename,
                indexname,
                idx_tup_read,
                idx_tup_fetch,
                idx_scan
            FROM pg_stat_user_indexes 
            ORDER BY idx_scan DESC").ToListAsync();

        return indexUsage;
    }

    public async Task<DatabaseSizeInfo> GetDatabaseSizeAsync()
    {
        var sizeInfo = await _context.Database.SqlQueryRaw<DatabaseSizeInfo>(@"
            SELECT 
                pg_database.datname AS database_name,
                pg_size_pretty(pg_database_size(pg_database.datname)) AS size
            FROM pg_database 
            WHERE datname = current_database()").FirstOrDefaultAsync();

        return sizeInfo;
    }
}

public record SlowQueryInfo(string Query, long Calls, double TotalTime, double MeanTime, long Rows, double HitPercent);
public record IndexUsageInfo(string SchemaName, string TableName, string IndexName, long IdxTupRead, long IdxTupFetch, long IdxScan);
public record DatabaseSizeInfo(string DatabaseName, string Size);
```

### Connection Pool Configuration
```csharp
// Infrastructure/Configuration/PostgreSqlConfiguration.cs
public static class PostgreSqlConfiguration
{
    public static IServiceCollection AddOptimizedPostgreSql(this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                // Enable connection pooling
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                
                // Configure command timeout
                npgsqlOptions.CommandTimeout(30);
                
                // Enable connection pooling at the ADO.NET level
                var builder = new NpgsqlConnectionStringBuilder(connectionString)
                {
                    Pooling = true,
                    MinPoolSize = 5,
                    MaxPoolSize = 100,
                    ConnectionLifetime = 300, // 5 minutes
                    ConnectionPruningInterval = 10,
                    ConnectionIdleLifetime = 600 // 10 minutes
                };
                
                npgsqlOptions.UseConnectionString(builder.ToString());
            });
            
            // Configure EF Core options
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });

        return services;
    }
}
```

# Infrastructure Layer

## Overview

The Infrastructure layer implements the interfaces defined in the Application and Domain layers. It handles all external concerns such as database access, file storage, email services, and third-party integrations. This layer is responsible for the technical implementation details that support the business logic.

## Design Principles

### 1. Implementation of Abstractions
- Implements repository interfaces from Domain layer
- Implements service interfaces from Application layer
- Provides concrete implementations for all external dependencies
- Maintains loose coupling through dependency injection

### 2. External Concerns
- Database access and ORM configuration
- File system operations
- External API integrations
- Email and notification services
- Caching implementations

### 3. Framework Dependencies
- Entity Framework Core for data access
- Third-party libraries and SDKs
- Infrastructure-specific configurations
- External service clients

## Current Implementation

The Infrastructure layer is currently implemented and includes:

**Projects**:
- `GoldenFiberERP.Infrastructure` - External services and infrastructure concerns
- `GoldenFiberERP.Persistence` - Database-specific implementations and repositories

### Current Project Structure

#### GoldenFiberERP.Infrastructure
**Location**: `src/Infrastructure/GoldenFiberERP.Infrastructure`

```
GoldenFiberERP.Infrastructure/
├── GoldenFiberERP.Infrastructure.csproj
├── DependencyInjection.cs     # Service registration
├── README.md                  # Infrastructure documentation
├── Extensions/                # Extension methods and utilities
└── Services/                  # External service implementations
    └── (planned implementations)
```

#### GoldenFiberERP.Persistence
**Location**: `src/Infrastructure/GoldenFiberERP.Persistence`

```
GoldenFiberERP.Persistence/
├── GoldenFiberERP.Persistence.csproj
├── DependencyInjection.cs     # Persistence service registration
├── README.md                  # Persistence layer documentation
├── Configurations/            # Entity configurations for EF Core
│   └── Settings/              # Settings entity configurations
│       └── CountryConfiguration.cs # Country EF configuration
├── Contexts/                  # Database contexts
│   └── ApplicationDbContext.cs # Main EF DbContext
├── Repositories/              # Repository implementations
│   ├── Common/                # Base repository patterns
│   └── Settings/              # Settings repositories
│       └── CountryRepository.cs # Country repository implementation
└── Seeders/                   # Database seeders and initial data
    └── (seeding implementations)
```

## Data Access Implementation

### Database Context

#### ApplicationDbContext
```csharp
// Contexts/ApplicationDbContext.cs
public class ApplicationDbContext : DbContext, IApplicationDbContext, IUnitOfWork
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;
    private readonly IDomainEventService? _domainEventService;
    private IDbContextTransaction? _currentTransaction;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDateTime dateTime,
        IDomainEventService? domainEventService = null) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
        _domainEventService = domainEventService;
    }

    // Entity Sets
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Country> Countries => Set<Country>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatic auditing for AuditableEntity
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (int.TryParse(_currentUserService.UserId, out var createdByUserId))
                        entry.Entity.CreatedBy = createdByUserId;
                    entry.Entity.CreatedAt = _dateTime.Now;
                    break;
                    
                case EntityState.Modified:
                    if (int.TryParse(_currentUserService.UserId, out var modifiedByUserId))
                        entry.Entity.UpdatedBy = modifiedByUserId;
                    entry.Entity.UpdatedAt = _dateTime.Now;
                    break;
            }
        }

        // Process domain events if service is available
        if (_domainEventService != null)
            await _domainEventService.PublishEvents(this);

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Apply all entity configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        // Global query filters for soft delete
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeleteEntity).IsAssignableFrom(entityType.ClrType))
            {
                // Configure soft delete filter
                var method = typeof(ApplicationDbContext)
                    .GetMethod(nameof(GetSoftDeleteFilter), BindingFlags.NonPublic | BindingFlags.Static)?
                    .MakeGenericMethod(entityType.ClrType);
                
                var filter = method?.Invoke(null, Array.Empty<object>());
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter((LambdaExpression)filter!);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    // Unit of Work pattern implementation
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _currentTransaction ??= await Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        try
        {
            await SaveChangesAsync();
            if (_currentTransaction != null)
                await _currentTransaction.CommitAsync();
        }
        catch
        {
            await RollbackTransactionAsync();
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        try
        {
            if (_currentTransaction != null)
                await _currentTransaction.RollbackAsync();
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }
}
```
                var property = Expression.Property(parameter, nameof(ISoftDeleteEntity.IsDeleted));
                var condition = Expression.Equal(property, Expression.Constant(false));
                var lambda = Expression.Lambda(condition, parameter);
                
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }

        // PostgreSQL-specific configurations
        modelBuilder.ConfigurePostgreSqlSpecifics();

        base.OnModelCreating(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService.UserId;
                    entry.Entity.CreatedAt = _dateTime.Now;
                    entry.Entity.UpdatedBy = _currentUserService.UserId;
                    entry.Entity.UpdatedAt = _dateTime.Now;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedBy = _currentUserService.UserId;
                    entry.Entity.UpdatedAt = _dateTime.Now;
                    break;

                case EntityState.Deleted:
                    if (entry.Entity is ISoftDeleteEntity softDeleteEntity)
                    {
                        entry.State = EntityState.Modified;
                        softDeleteEntity.IsDeleted = true;
                        softDeleteEntity.DeletedAt = _dateTime.Now;
                        softDeleteEntity.DeletedBy = _currentUserService.UserId;
                    }
                    break;
            }
        }

        // Dispatch domain events
        await _domainEventService.DispatchEventsAsync(this);

        return await base.SaveChangesAsync(cancellationToken);
    }

    // Unit of Work implementation
    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.CommitTransactionAsync(cancellationToken);
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        await Database.RollbackTransactionAsync(cancellationToken);
    }
}
```

### Entity Configurations

#### Product Configuration
```csharp
// Data/Configurations/ProductConfiguration.cs
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Category)
            .HasMaxLength(100);

        builder.Property(p => p.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Cost)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Unit)
            .HasMaxLength(20);

        // Indexes
        builder.HasIndex(p => p.Code)
            .IsUnique()
            .HasDatabaseName("IX_Products_Code");

        builder.HasIndex(p => p.Name)
            .HasDatabaseName("IX_Products_Name");

        builder.HasIndex(p => p.Category)
            .HasDatabaseName("IX_Products_Category");

        builder.HasIndex(p => p.IsActive)
            .HasDatabaseName("IX_Products_IsActive");

        // Audit configuration
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(p => p.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
```

#### Base Entity Configuration
```csharp
// Data/Configurations/BaseEntityConfiguration.cs
public abstract class BaseEntityConfiguration<T> : IEntityTypeConfiguration<T> where T : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<T> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .ValueGeneratedOnAdd();

        builder.Property(e => e.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(e => e.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");
    }
}
```

### Repository Implementations

#### Base Repository
```csharp
// Data/Repositories/BaseRepository.cs
public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly ApplicationDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public BaseRepository(ApplicationDbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task<T> AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        var entityEntry = await _dbSet.AddAsync(entity, cancellationToken);
        return entityEntry.Entity;
    }

    public virtual Task<T> UpdateAsync(T entity, CancellationToken cancellationToken = default)
    {
        _dbSet.Update(entity);
        return Task.FromResult(entity);
    }

    public virtual async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await GetByIdAsync(id, cancellationToken);
        if (entity != null)
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(e => e.Id == id, cancellationToken);
    }

    protected IQueryable<T> ApplySpecification(ISpecification<T> spec)
    {
        return SpecificationEvaluator<T>.GetQuery(_dbSet.AsQueryable(), spec);
    }
}
```

#### Product Repository
```csharp
// Data/Repositories/ProductRepository.cs
public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    public ProductRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<Product?> GetByCode(string productCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .FirstOrDefaultAsync(p => p.Code == productCode, cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetByCategory(string category, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Category == category && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> GetLowStockProducts(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.StockQuantity <= p.MinimumStockLevel && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Product>> SearchByName(string searchTerm, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(p => p.Name.Contains(searchTerm) && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> IsCodeUnique(string code, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Code == code);
        
        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return !await query.AnyAsync(cancellationToken);
    }

    public async Task<PagedResult<Product>> GetPagedAsync(
        int pageNumber, 
        int pageSize, 
        string searchTerm = "", 
        string category = "", 
        bool? isActive = null, 
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
            query = query.Where(p => p.Name.Contains(searchTerm) || p.Code.Contains(searchTerm));

        if (!string.IsNullOrWhiteSpace(category))
            query = query.Where(p => p.Category == category);

        if (isActive.HasValue)
            query = query.Where(p => p.IsActive == isActive.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(p => p.Name)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Product>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
```

## Service Implementations

### Email Service
```csharp
// Services/Email/EmailService.cs
public class EmailService : IEmailService
{
    private readonly ISmtpClient _smtpClient;
    private readonly EmailSettings _emailSettings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        ISmtpClient smtpClient,
        IOptions<EmailSettings> emailSettings,
        ILogger<EmailService> logger)
    {
        _smtpClient = smtpClient;
        _emailSettings = emailSettings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            var message = new MailMessage
            {
                From = new MailAddress(_emailSettings.FromAddress, _emailSettings.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            message.To.Add(to);

            await _smtpClient.SendMailAsync(message);
            
            _logger.LogInformation("Email sent successfully to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To} with subject {Subject}", to, subject);
            throw;
        }
    }

    public async Task SendEmailTemplateAsync<T>(string to, string templateName, T model, CancellationToken cancellationToken = default)
    {
        // Template rendering logic here
        var body = await RenderTemplateAsync(templateName, model);
        var subject = GetTemplateSubject(templateName);
        
        await SendEmailAsync(to, subject, body, cancellationToken);
    }

    private async Task<string> RenderTemplateAsync<T>(string templateName, T model)
    {
        // Template rendering implementation
        // Could use Razor, Handlebars, or other template engine
        throw new NotImplementedException();
    }

    private string GetTemplateSubject(string templateName)
    {
        // Get subject from template metadata
        throw new NotImplementedException();
    }
}
```

### File Service
```csharp
// Services/FileStorage/FileService.cs
public class FileService : IFileService
{
    private readonly FileStorageSettings _settings;
    private readonly ILogger<FileService> _logger;

    public FileService(IOptions<FileStorageSettings> settings, ILogger<FileService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<string> SaveFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        try
        {
            var uniqueFileName = GenerateUniqueFileName(fileName);
            var filePath = Path.Combine(_settings.BasePath, uniqueFileName);
            
            Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

            using var fileStreamWriter = new FileStream(filePath, FileMode.Create);
            await fileStream.CopyToAsync(fileStreamWriter, cancellationToken);

            _logger.LogInformation("File saved successfully: {FilePath}", filePath);
            
            return uniqueFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file: {FileName}", fileName);
            throw;
        }
    }

    public async Task<Stream> GetFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_settings.BasePath, filePath);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read);
    }

    public async Task DeleteFileAsync(string filePath, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_settings.BasePath, filePath);
        
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            _logger.LogInformation("File deleted: {FilePath}", filePath);
        }
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileName = Path.GetFileNameWithoutExtension(originalFileName);
        var uniqueId = Guid.NewGuid().ToString("N");
        
        return $"{fileName}_{uniqueId}{extension}";
    }
}
```

### Caching Service
```csharp
// Services/Caching/CacheService.cs
public class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly IDistributedCache _distributedCache;
    private readonly CacheSettings _settings;
    private readonly ILogger<CacheService> _logger;

    public CacheService(
        IMemoryCache memoryCache,
        IDistributedCache distributedCache,
        IOptions<CacheSettings> settings,
        ILogger<CacheService> logger)
    {
        _memoryCache = memoryCache;
        _distributedCache = distributedCache;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        // Try memory cache first
        if (_memoryCache.TryGetValue(key, out T? memoryValue))
        {
            _logger.LogDebug("Cache hit (memory): {Key}", key);
            return memoryValue;
        }

        // Try distributed cache
        var distributedValue = await _distributedCache.GetStringAsync(key, cancellationToken);
        if (distributedValue != null)
        {
            var deserializedValue = JsonSerializer.Deserialize<T>(distributedValue);
            
            // Store in memory cache for faster subsequent access
            _memoryCache.Set(key, deserializedValue, TimeSpan.FromMinutes(_settings.MemoryCacheExpirationMinutes));
            
            _logger.LogDebug("Cache hit (distributed): {Key}", key);
            return deserializedValue;
        }

        _logger.LogDebug("Cache miss: {Key}", key);
        return default;
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        var expirationTime = expiration ?? TimeSpan.FromMinutes(_settings.DefaultExpirationMinutes);
        
        // Set in memory cache
        _memoryCache.Set(key, value, expirationTime);
        
        // Set in distributed cache
        var serializedValue = JsonSerializer.Serialize(value);
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expirationTime
        };
        
        await _distributedCache.SetStringAsync(key, serializedValue, options, cancellationToken);
        
        _logger.LogDebug("Cache set: {Key}", key);
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _memoryCache.Remove(key);
        await _distributedCache.RemoveAsync(key, cancellationToken);
        
        _logger.LogDebug("Cache removed: {Key}", key);
    }
}
```

## Configuration and Dependency Injection

### Dependency Injection Setup
```csharp
// Configuration/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Unit of Work
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Repositories
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();

        // Services
        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<IFileService, FileService>();
        services.AddSingleton<ICacheService, CacheService>();

        // External Services
        services.AddHttpClient<IPaymentService, PaymentService>();
        services.AddHttpClient<IShippingService, ShippingService>();

        // Background Services
        services.AddHostedService<StockAlertBackgroundService>();
        services.AddHostedService<EmailProcessingBackgroundService>();

        // Configuration
        services.Configure<EmailSettings>(configuration.GetSection("EmailSettings"));
        services.Configure<FileStorageSettings>(configuration.GetSection("FileStorageSettings"));
        services.Configure<CacheSettings>(configuration.GetSection("CacheSettings"));

        // Caching
        services.AddMemoryCache();
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
        });

        // Identity
        services.AddIdentity<ApplicationUser, ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        return services;
    }
}
```

### Configuration Models
```csharp
// Configuration/Settings/EmailSettings.cs
public class EmailSettings
{
    public string SmtpServer { get; set; } = string.Empty;
    public int SmtpPort { get; set; }
    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; }
}

// Configuration/Settings/FileStorageSettings.cs
public class FileStorageSettings
{
    public string BasePath { get; set; } = string.Empty;
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024; // 10MB
    public string[] AllowedExtensions { get; set; } = Array.Empty<string>();
}

// Configuration/Settings/CacheSettings.cs
public class CacheSettings
{
    public int DefaultExpirationMinutes { get; set; } = 60;
    public int MemoryCacheExpirationMinutes { get; set; } = 15;
    public bool EnableDistributedCache { get; set; } = true;
}
```

## Background Services

### Stock Alert Service
```csharp
// Services/BackgroundServices/StockAlertBackgroundService.cs
public class StockAlertBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<StockAlertBackgroundService> _logger;

    public StockAlertBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<StockAlertBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var productRepository = scope.ServiceProvider.GetRequiredService<IProductRepository>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

                var lowStockProducts = await productRepository.GetLowStockProducts(stoppingToken);
                
                if (lowStockProducts.Any())
                {
                    await SendLowStockAlert(lowStockProducts, emailService);
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in stock alert background service");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }

    private async Task SendLowStockAlert(IEnumerable<Product> products, IEmailService emailService)
    {
        var subject = "Low Stock Alert";
        var body = GenerateLowStockEmailBody(products);
        
        // Send to configured administrators
        await emailService.SendEmailAsync("admin@company.com", subject, body);
        
        _logger.LogInformation("Low stock alert sent for {Count} products", products.Count());
    }

    private string GenerateLowStockEmailBody(IEnumerable<Product> products)
    {
        // Generate email body with product details
        var sb = new StringBuilder();
        sb.AppendLine("<h2>Low Stock Alert</h2>");
        sb.AppendLine("<p>The following products are running low on stock:</p>");
        sb.AppendLine("<ul>");
        
        foreach (var product in products)
        {
            sb.AppendLine($"<li>{product.Name} ({product.Code}) - Current Stock: {product.StockQuantity}, Minimum: {product.MinimumStockLevel}</li>");
        }
        
        sb.AppendLine("</ul>");
        
        return sb.ToString();
    }
}
```

## External Integrations

### Payment Gateway Integration
```csharp
// External/PaymentGateways/IPaymentService.cs
public interface IPaymentService
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default);
    Task<RefundResult> RefundPaymentAsync(RefundRequest request, CancellationToken cancellationToken = default);
    Task<PaymentStatus> GetPaymentStatusAsync(string transactionId, CancellationToken cancellationToken = default);
}

// External/PaymentGateways/StripePaymentService.cs
public class StripePaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly PaymentGatewaySettings _settings;
    private readonly ILogger<StripePaymentService> _logger;

    public StripePaymentService(
        HttpClient httpClient,
        IOptions<PaymentGatewaySettings> settings,
        ILogger<StripePaymentService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request, CancellationToken cancellationToken = default)
    {
        // Stripe payment processing implementation
        throw new NotImplementedException();
    }

    // Additional method implementations...
}
```

## Required Dependencies

### NuGet Packages
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.0" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.0" />
<PackageReference Include="Microsoft.Extensions.Caching.StackExchangeRedis" Version="8.0.0" />
<PackageReference Include="MailKit" Version="4.3.0" />
<PackageReference Include="AutoMapper" Version="12.0.1" />
<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.0" />
```

### Project References
```xml
<ProjectReference Include="..\Core\GoldenFiberERP.Application\GoldenFiberERP.Application.csproj" />
<ProjectReference Include="..\Core\GoldenFiberERP.Domain\GoldenFiberERP.Domain.csproj" />
```

The Infrastructure layer provides all the technical implementations needed to support the business logic defined in the Application and Domain layers, while maintaining proper separation of concerns and dependency inversion principles.

## Data Access Implementation

### PostgreSQL Configuration

#### Database Provider Setup
```csharp
// Infrastructure/DependencyInjection.cs - PostgreSQL Configuration
public static IServiceCollection AddPostgreSqlDatabase(this IServiceCollection services, 
    IConfiguration configuration)
{
    services.AddDbContext<ApplicationDbContext>(options =>
    {
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), 
            npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
            });
        
        // Enable sensitive data logging in development
        if (configuration.GetValue<bool>("Database:EnableSensitiveDataLogging"))
        {
            options.EnableSensitiveDataLogging();
        }
        
        // Enable detailed errors in development
        if (configuration.GetValue<bool>("Database:EnableDetailedErrors"))
        {
            options.EnableDetailedErrors();
        }
    });
    
    return services;
}
```

#### Connection String Configuration
```json
// appsettings.json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=GoldenFiberERP;Username=postgres;Password=your_password;Include Error Detail=true;",
    "Redis": "localhost:6379"
  },
  "Database": {
    "EnableSensitiveDataLogging": false,
    "EnableDetailedErrors": false,
    "CommandTimeout": 30
  }
}
```

#### PostgreSQL-Specific Entity Configurations
```csharp
// Data/Configurations/PostgreSqlConfiguration.cs
public static class PostgreSqlConfiguration
{
    public static void ConfigurePostgreSqlSpecifics(this ModelBuilder modelBuilder)
    {
        // Use PostgreSQL naming conventions
        foreach (var entity in modelBuilder.Model.GetEntityTypes())
        {
            // Convert table names to snake_case
            entity.SetTableName(entity.GetTableName()?.ToSnakeCase());
            
            // Convert column names to snake_case
            foreach (var property in entity.GetProperties())
            {
                property.SetColumnName(property.GetColumnName().ToSnakeCase());
            }
            
            // Convert index names to snake_case
            foreach (var index in entity.GetIndexes())
            {
                index.SetDatabaseName(index.GetDatabaseName()?.ToSnakeCase());
            }
        }
        
        // Configure PostgreSQL-specific data types
        modelBuilder.Entity<Product>(entity =>
        {
            // Use PostgreSQL UUID type
            entity.Property(e => e.Id)
                .HasColumnType("uuid")
                .HasDefaultValueSql("gen_random_uuid()");
                
            // Use PostgreSQL timestamp with timezone
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("NOW()");
                
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp with time zone")
                .HasDefaultValueSql("NOW()");
                
            // Use PostgreSQL money type for currency
            entity.Property(e => e.Price)
                .HasColumnType("money");
                
            // Use PostgreSQL text type for large text fields
            entity.Property(e => e.Description)
                .HasColumnType("text");
                
            // Use PostgreSQL jsonb for flexible data
            entity.Property(e => e.Metadata)
                .HasColumnType("jsonb");
        });
    }
}

public static class StringExtensions
{
    public static string ToSnakeCase(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;
            
        return Regex.Replace(text, "([a-z])([A-Z])", "$1_$2").ToLower();
    }
}
```

### Database Context

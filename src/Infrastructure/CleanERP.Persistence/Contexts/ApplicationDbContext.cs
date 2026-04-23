using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Domain.Entities.Inventory;
using CleanERP.Domain.Entities.Settings;
using CleanERP.Domain.Entities.Identity;
using CleanERP.Domain.Entities.Common;
using CleanERP.Persistence.Extensions;
using System.Reflection;

namespace CleanERP.Persistence.Contexts;

public class ApplicationDbContext : DbContext, IUnitOfWork
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

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Country> Countries => Set<Country>();

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    if (int.TryParse(_currentUserService.UserId, out var createdByUserId))
                        entry.Entity.CreatedBy = createdByUserId;
                    entry.Entity.CreatedAt = _dateTime.UtcNow;
                    break;

                case EntityState.Modified:
                    if (int.TryParse(_currentUserService.UserId, out var updatedByUserId))
                        entry.Entity.UpdatedBy = updatedByUserId;
                    entry.Entity.UpdatedAt = _dateTime.UtcNow;
                    break;
            }
        }

        // Dispatch domain events if service is available
        if (_domainEventService != null)
        {
            var domainEventEntities = ChangeTracker
                .Entries<BaseEntity>()
                .Select(entry => entry.Entity)
                .Where(entity => entity.DomainEvents.Any())
                .ToArray();

            var domainEvents = domainEventEntities
                .SelectMany(entity => entity.DomainEvents)
                .ToArray();

            foreach (var entity in domainEventEntities)
            {
                entity.ClearDomainEvents();
            }

            await _domainEventService.DispatchEventsAsync(domainEvents, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    #region Unit of Work Implementation

    public bool HasActiveTransaction => _currentTransaction != null;

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            return;
        }

        _currentTransaction = await Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await SaveChangesAsync(cancellationToken);
            
            if (_currentTransaction != null)
            {
                await _currentTransaction.CommitAsync(cancellationToken);
            }
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(cancellationToken);
            }
        }
        finally
        {
            if (_currentTransaction != null)
            {
                await _currentTransaction.DisposeAsync();
                _currentTransaction = null;
            }
        }
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> func, CancellationToken cancellationToken = default)
    {
        if (HasActiveTransaction)
        {
            return await func();
        }

        await BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await func();
            await CommitTransactionAsync(cancellationToken);
            return result;
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        await ExecuteInTransactionAsync(async () =>
        {
            await action();
            return 0; // Return dummy value for the generic method
        }, cancellationToken);
    }

    #endregion

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Apply all entity configurations from assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        
        // Apply PostgreSQL-specific configurations (snake_case naming)
        builder.ConfigurePostgreSqlSpecifics();
        
        base.OnModelCreating(builder);
    }
}

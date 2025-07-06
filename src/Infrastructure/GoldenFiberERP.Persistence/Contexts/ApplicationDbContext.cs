using Microsoft.EntityFrameworkCore;
using GoldenFiberERP.Application.Common.Interfaces;
using GoldenFiberERP.Domain.Entities.Inventory;
using GoldenFiberERP.Domain.Entities.Common;
using System.Reflection;

namespace GoldenFiberERP.Persistence.Contexts;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTime _dateTime;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService currentUserService,
        IDateTime dateTime) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTime = dateTime;
    }

    public DbSet<Product> Products => Set<Product>();

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

        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}

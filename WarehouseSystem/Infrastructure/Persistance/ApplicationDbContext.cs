// Infrastructure/Persistence/ApplicationDbContext.cs
using Domain.Common;
using Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    private readonly IAuditService _auditService;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IAuditService auditService) : base(options)
    {
        _auditService = auditService;
    }

    public DbSet<Customer> Customers { get; set; }
    public DbSet<Site> Sites { get; set; }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    _auditService.SetCreatedBy(entry.Entity);
                    break;
                case EntityState.Modified:
                    _auditService.SetModifiedBy(entry.Entity);
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}

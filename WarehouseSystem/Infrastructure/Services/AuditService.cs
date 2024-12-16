// Infrastructure/Services/AuditService.cs
using Domain.Interfaces;
using Domain.Models;

namespace Infrastructure.Services;

public class AuditService : IAuditService
{
    private const string DEFAULT_USER = "neduhas-exe";

    public void SetCreatedBy(AuditableEntity entity)
    {
        entity.CreatedBy = new User { UserName = DEFAULT_USER };
        entity.CreatedDate = DateTime.UtcNow; // 2024-12-15 20:07:29
    }

    public void SetModifiedBy(AuditableEntity entity)
    {
        entity.ModifiedBy = new User { UserName = DEFAULT_USER };
        entity.ModifiedDate = DateTime.UtcNow;
    }
}

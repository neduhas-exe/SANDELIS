// Infrastructure/Services/AuditService.cs
using Domain.Common;
using Domain.Interfaces;

namespace Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly ICurrentUserService _currentUserService;

    public AuditService(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public void SetCreatedBy(AuditableEntity entity)
    {
        entity.CreatedById = _currentUserService.UserId;
        entity.CreatedDate = DateTime.UtcNow;
    }

    public void SetModifiedBy(AuditableEntity entity)
    {
        entity.ModifiedById = _currentUserService.UserId;
        entity.ModifiedDate = DateTime.UtcNow;
    }
}

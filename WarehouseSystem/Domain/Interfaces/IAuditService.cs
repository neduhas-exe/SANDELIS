// Domain/Interfaces/IAuditService.cs
namespace Domain.Interfaces;

/// <summary>
/// Servisas skirtas valdyti esybių audito informaciją
/// </summary>
// Application/Interfaces/IAuditService.cs
public interface IAuditService
{
    void SetCreatedBy(AuditableEntity entity);
    void SetModifiedBy(AuditableEntity entity);
}

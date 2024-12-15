// Domain/Interfaces/IAuditService.cs
namespace Domain.Interfaces;

/// <summary>
/// Servisas skirtas valdyti esybių audito informaciją
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// Nustato sukūrimo audito informaciją naujai esybei
    /// </summary>
    /// <param name="entity">Esybė kuriai nustatoma audito informacija</param>
    void SetCreatedBy(AuditableEntity entity);

    /// <summary>
    /// Nustato modifikavimo audito informaciją esybei
    /// </summary>
    /// <param name="entity">Esybė kuriai atnaujinama audito informacija</param>
    void SetModifiedBy(AuditableEntity entity);
}

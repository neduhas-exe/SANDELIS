// Domain/Common/AuditableEntity.cs
namespace Domain.Common;

/// <summary>
/// Bazinė klasė esybėms, kurioms reikalingas auditavimas
/// </summary>
public abstract class AuditableEntity
{
    /// <summary>
    /// Įrašo sukūrimo data
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Įrašą sukūrusio vartotojo ID
    /// </summary>
    public long CreatedById { get; set; }

    /// <summary>
    /// Įrašą sukūręs vartotojas
    /// </summary>
    public User CreatedBy { get; set; }
    
    /// <summary>
    /// Paskutinio modifikavimo data
    /// </summary>
    public DateTime? ModifiedDate { get; set; }

    /// <summary>
    /// Paskutinį kartą modifikavusio vartotojo ID
    /// </summary>
    public long? ModifiedById { get; set; }

    /// <summary>
    /// Paskutinį kartą modifikavęs vartotojas
    /// </summary>
    public User ModifiedBy { get; set; }
}

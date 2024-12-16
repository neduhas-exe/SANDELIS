// Domain/Models/Site.cs


namespace Domain.Models;

/// <summary>
/// Objekto/vietos modelis
/// Saugo informaciją apie kliento objektus/vietas
/// </summary>
public class Site : AuditableEntity
{
    /// <summary>
    /// Unikalus objekto identifikatorius
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Nuoroda į klientą (foreign key)
    /// </summary>
    public long CustomerId { get; set; }

    /// <summary>
    /// Objekto pavadinimas
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Objekto adresas
    /// </summary>
    public string Address { get; set; }

    /// <summary>
    /// Kontaktinis asmuo objekte
    /// </summary>
    public string ContactPerson { get; set; }

    /// <summary>
    /// Kontaktinis telefono numeris
    /// </summary>
    public string ContactPhone { get; set; }

    /// <summary>
    /// Žymi ar objektas aktyvus
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigacijos property į Customer objektą
    public Customer Customer { get; set; }
}

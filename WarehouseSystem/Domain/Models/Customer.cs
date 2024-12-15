// Domain/Models/Customer.cs
using Domain.Common;

namespace Domain.Models;

/// <summary>
/// Kliento modelis
/// Saugo pagrindinę informaciją apie klientą
/// </summary>
public class Customer : AuditableEntity
{
    /// <summary>
    /// Unikalus kliento identifikatorius
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Kliento tipo identifikatorius
    /// Nurodo kokio tipo yra klientas (fizinis asmuo, įmonė ir t.t.)
    /// </summary>
    public CustomerType CustomerType { get; set; }

    /// <summary>
    /// Kliento pavadinimas
    /// Įmonėms - įmonės pavadinimas
    /// Fiziniams asmenims - vardas pavardė
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Įmonės kodas
    /// Null jeigu fizinis asmuo
    /// </summary>
    public string? CompanyCode { get; set; }

    /// <summary>
    /// PVM mokėtojo kodas
    /// Null jeigu ne PVM mokėtojas
    /// </summary>
    public string? VATCode { get; set; }

    /// <summary>
    /// Juridinis/registracijos adresas
    /// </summary>
    public string LegalAddress { get; set; }

    /// <summary>
    /// Kontaktinis asmuo
    /// </summary>
    public string ContactPersonName { get; set; }

    /// <summary>
    /// Kontaktinis el. paštas
    /// </summary>
    public string ContactEmail { get; set; }

    /// <summary>
    /// Kontaktinis telefonas
    /// </summary>
    public string ContactPhone { get; set; }

    /// <summary>
    /// Nustatytas kredito limitas
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Žymi ar klientas aktyvus sistemoje
    /// </summary>
    public bool IsActive { get; set; } = true;

    // Navigacijos property į Site objektus
    public ICollection<Site> Sites { get; set; } = new List<Site>();
}

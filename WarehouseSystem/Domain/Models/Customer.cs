// Domain/Models/Customer.cs
namespace Domain.Models;

public class Customer
{
    /// <summary>
    /// Unikalus kliento identifikatorius
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Kliento tipas (fizinis asmuo, UAB, etc.)
    /// </summary>
    public CustomerType CustomerType { get; set; }

    /// <summary>
    /// Įmonės pavadinimas arba fizinio asmens vardas pavardė
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Įmonės kodas (jei juridinis asmuo)
    /// Null jei fizinis asmuo
    /// </summary>
    public string? CompanyCode { get; set; }

    /// <summary>
    /// PVM mokėtojo kodas
    /// Null jei ne PVM mokėtojas
    /// </summary>
    public string? VATCode { get; set; }

    /// <summary>
    /// Juridinis/Registracijos adresas
    /// </summary>
    public string LegalAddress { get; set; }

    /// <summary>
    /// Pristatymo adresas (jei skiriasi nuo juridinio)
    /// </summary>
    public string? ShippingAddress { get; set; }

    /// <summary>
    /// Kontaktinio asmens vardas
    /// </summary>
    public string ContactPersonName { get; set; }

    /// <summary>
    /// Kontaktinio asmens el. paštas
    /// </summary>
    public string ContactEmail { get; set; }

    /// <summary>
    /// Kontaktinio asmens telefono numeris
    /// </summary>
    public string ContactPhone { get; set; }

    /// <summary>
    /// Kredito limitas eurais
    /// </summary>
    public decimal CreditLimit { get; set; }

    /// <summary>
    /// Ar klientas aktyvus
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Įrašo sukūrimo data
    /// </summary>
    public DateTime CreatedDate { get; set; }

    /// <summary>
    /// Kas sukūrė įrašą
    /// </summary>
    public string CreatedBy { get; set; }

    /// <summary>
    /// Paskutinio atnaujinimo data
    /// </summary>
    public DateTime? LastModifiedDate { get; set; }

    /// <summary>
    /// Kas paskutinis atnaujino įrašą
    /// </summary>
    public string? LastModifiedBy { get; set; }
}
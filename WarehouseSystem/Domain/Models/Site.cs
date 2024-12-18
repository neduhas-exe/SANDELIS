using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;  // Pridėkite šį using

namespace Domain.Models;

public class Site : AuditableEntity
{
    public long Id { get; set; }

    [Required]
    public long CustomerId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Address { get; set; }

    public string ContactPerson { get; set; }

    public string ContactPhone { get; set; }

    public bool IsActive { get; set; } = true;

    // Pakeičiame navigacijos property
    [JsonIgnore]
    public virtual Customer? Customer { get; set; }  // Pridėjome virtual ir padarėme nullable (?)
}
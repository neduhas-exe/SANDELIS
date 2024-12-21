using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

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

    public string Comments { get; set; } = string.Empty;

    public DateTime? LastCommentDate { get; set; }

    public decimal? TotalProductValue { get; set; }

    public int? TotalProductCount { get; set; }

    public virtual ICollection<SiteHistory> History { get; set; } = new List<SiteHistory>();

    [JsonIgnore]
    public virtual Customer? Customer { get; set; }
}
// Domain/Models/SiteHistory.cs
using System.Text.Json.Serialization;

namespace Domain.Models;

public class SiteHistory : AuditableEntity
{
    public long Id { get; set; }
    public long SiteId { get; set; }
    public string UserName { get; set; }
    public DateTime ChangeDate { get; set; }
    public string ChangeType { get; set; } // "Created", "Modified", "Comment Added", "Product Added", etc.
    public string ChangeSummary { get; set; }
    public string PreviousValues { get; set; } // JSON string of changed values
    public string NewValues { get; set; }      // JSON string of changed values
    public decimal? ProductValueChange { get; set; }
    public int? ProductQuantityChange { get; set; }
    public bool IsAdminAction { get; set; }

    [JsonIgnore]
    public virtual Site Site { get; set; }
}
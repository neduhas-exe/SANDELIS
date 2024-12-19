// Domain/Models/SiteChange.cs
namespace Domain.Models;

public class SiteChange
{
    public string FieldName { get; set; }
    public string OldValue { get; set; }
    public string NewValue { get; set; }
    public string ChangeReason { get; set; }
}
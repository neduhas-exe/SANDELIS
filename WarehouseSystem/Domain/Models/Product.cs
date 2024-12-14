public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;  // Required
    public string Barcode { get; set; } = string.Empty;  // Required
    public string? Description { get; set; }  // Optional
}

namespace Presentation.DTOs
{
    //TODO: Add other properties.
    //DTO should only have the properties you want to expose to the presentation layer (what properties the user can see).
    public class ProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Barcode { get; set; }
        public string Description { get; set; }
    }
}

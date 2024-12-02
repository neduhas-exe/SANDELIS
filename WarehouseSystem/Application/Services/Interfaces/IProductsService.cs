using Presentation.DTOs.Products;

namespace WarehouseSystem.Services.Interfaces
{
    // Produktų valdymo serviso interfeisas
    public interface IProductService
    {
        // Pagrindinės CRUD operacijos
        Task<ProductDto> GetProductByIdAsync(int id);
        Task<ProductDto> GetProductByEANAsync(string eanCode);
        Task<ProductDto> GetProductByQRCodeAsync(string qrCode);
        Task<IEnumerable<ProductDto>> GetAllProductsAsync();
        Task<ProductDto> CreateProductAsync(CreateProductDto productDto);
        Task<ProductDto> UpdateProductAsync(UpdateProductDto productDto);
        
        // QR kodų operacijos
        Task<IEnumerable<ProductQRCodeDto>> GetProductQRCodesAsync(int productId);
        Task<ProductQRCodeDto> AddQRCodeAsync(AddProductQRCodeDto qrCodeDto);
        Task<bool> UpdateQRCodeStatusAsync(UpdateQRCodeStatusDto statusDto);
        
        // Sandėlio operacijos
        Task<ProductLocationDto> GetProductLocationAsync(int productId, string warehouseId);
        Task<IEnumerable<ProductMovementDto>> GetProductMovementsAsync(
            int productId, 
            DateTime? startDate = null, 
            DateTime? endDate = null
        );
        
        // Paieškos ir filtravimo operacijos
        Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm);
        Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<ProductDto>> GetDiscontinuedProductsAsync();
        Task<IEnumerable<ProductDto>> GetLowStockProductsAsync();
        
        // CSV failų operacijos 
        Task ExportProductsToCsvAsync(string filePath);
        Task ImportProductsFromCsvAsync(string filePath);
        
        // Statistika ir ataskaitos
        Task<decimal> GetTotalStockValueAsync();
        Task<IDictionary<string, int>> GetProductCountByCategoryAsync();
        Task<IEnumerable<ProductDto>> GetTopSellingProductsAsync(int count);
    }
}

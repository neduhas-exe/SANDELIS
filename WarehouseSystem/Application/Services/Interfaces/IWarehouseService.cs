using Presentation.DTOs.Products;

namespace WarehouseSystem.Services.Interfaces
{
    /// <summary>
    /// Sandėlio operacijų serviso interfeisas
    /// </summary>
    public interface IWarehouseService
    {
        // Pagrindinės sandėlio operacijos
        Task<ProductLocationDto> GetLocationAsync(string warehouseId, string locationCode);
        Task<IEnumerable<ProductLocationDto>> GetAllLocationsAsync(string warehouseId);
        Task<IEnumerable<ProductLocationDto>> GetProductLocationsAsync(int productId);
        
        // Produktų judėjimo operacijos
        Task<bool> ReceiveProductAsync(
            int productId,
            string warehouseId,
            string locationCode,
            decimal quantity,
            string qrCodeId = null);
            
        Task<bool> TransferProductAsync(
            int productId,
            string sourceWarehouseId,
            string sourceLocationCode,
            string destinationWarehouseId,
            string destinationLocationCode,
            decimal quantity,
            string qrCodeId = null);
            
        Task<bool> ShipProductAsync(
            int productId,
            string warehouseId,
            string locationCode,
            decimal quantity,
            string referenceNumber,
            string qrCodeId = null);
        
        // Inventorizacijos operacijos
        Task<bool> AdjustStockAsync(
            int productId,
            string warehouseId,
            string locationCode,
            decimal newQuantity,
            string reason,
            string adjustedByUser);
            
        Task<ProductLocationDto> CountStockAsync(
            int productId,
            string warehouseId,
            string locationCode,
            decimal countedQuantity,
            string countedByUser);
        
        // Sandėlio vietos valdymas
        Task<bool> CreateLocationAsync(
            string warehouseId,
            string zone,
            string aisle,
            string rack,
            string shelf,
            string bin,
            decimal maxCapacity,
            string storageConditions = null);
            
        Task<bool> UpdateLocationAsync(
            string warehouseId,
            string locationCode,
            decimal? maxCapacity = null,
            string storageConditions = null,
            bool? isQuarantine = null);
            
        Task<bool> DeleteLocationAsync(
            string warehouseId,
            string locationCode);
        
        // Ataskaitos ir statistika
        Task<decimal> GetTotalStockQuantityAsync(int productId);
        Task<decimal> GetLocationUtilizationAsync(string warehouseId, string locationCode);
        Task<IDictionary<string, decimal>> GetStockLevelsAsync(string warehouseId);
        Task<IEnumerable<ProductLocationDto>> GetLowStockLocationsAsync(string warehouseId);
        
        // CSV operacijos
        Task ExportLocationsToCsvAsync(string filePath, string warehouseId = null);
        Task ImportLocationsFromCsvAsync(string filePath);
        Task<string> GenerateStockReportAsync(string warehouseId, DateTime reportDate);
    }
}

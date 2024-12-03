using Presentation.DTOs.Products;

namespace WarehouseSystem.Services.Interfaces
{
    /// <summary>
    /// Produktų judėjimo operacijų serviso interfeisas
    /// </summary>
    public interface IMovementService
    {
        // Pagrindinės judėjimo operacijos
        Task<ProductMovementDto> RegisterMovementAsync(
            int productId,
            string movementType,
            decimal quantity,
            string sourceLocation,
            string destinationLocation,
            string referenceNumber,
            string qrCodeId = null);
            
        Task<ProductMovementDto> GetMovementByIdAsync(int movementId);
        Task<IEnumerable<ProductMovementDto>> GetMovementsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null);

        // Produkto judėjimų operacijos
        Task<IEnumerable<ProductMovementDto>> GetProductMovementsAsync(
            int productId,
            DateTime? startDate = null,
            DateTime? endDate = null);
            
        Task<decimal> GetProductTotalIncomingAsync(
            int productId,
            DateTime? startDate = null,
            DateTime? endDate = null);
            
        Task<decimal> GetProductTotalOutgoingAsync(
            int productId,
            DateTime? startDate = null,
            DateTime? endDate = null);

        // QR kodo judėjimų operacijos
        Task<IEnumerable<ProductMovementDto>> GetQRCodeMovementsAsync(string qrCodeId);
        Task<string> GetQRCodeCurrentLocationAsync(string qrCodeId);
        Task<DateTime?> GetQRCodeLastMovementDateAsync(string qrCodeId);

        // Lokacijos judėjimų operacijos
        Task<IEnumerable<ProductMovementDto>> GetLocationMovementsAsync(
            string locationCode,
            DateTime? startDate = null,
            DateTime? endDate = null);
            
        Task<decimal> GetLocationTotalIncomingAsync(
            string locationCode,
            DateTime? startDate = null,
            DateTime? endDate = null);
            
        Task<decimal> GetLocationTotalOutgoingAsync(
            string locationCode,
            DateTime? startDate = null,
            DateTime? endDate = null);

        // Judėjimų paieška ir filtravimas
        Task<IEnumerable<ProductMovementDto>> SearchMovementsAsync(
            string searchTerm,
            string movementType = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
            
        Task<IEnumerable<ProductMovementDto>> GetMovementsByTypeAsync(
            string movementType,
            DateTime? startDate = null,
            DateTime? endDate = null);

        // Judėjimų statistika ir analizė
        Task<IDictionary<string, int>> GetMovementCountsByTypeAsync(
            DateTime? startDate = null,
            DateTime? endDate = null);
            
        Task<IDictionary<DateTime, decimal>> GetDailyMovementTotalsAsync(
            string movementType,
            DateTime startDate,
            DateTime endDate);
            
        Task<IEnumerable<ProductMovementDto>> GetTopMovingProductsAsync(
            int count,
            DateTime? startDate = null,
            DateTime? endDate = null);

        // CSV operacijos
        Task ExportMovementsToCsvAsync(
            string filePath,
            DateTime? startDate = null,
            DateTime? endDate = null);
            
        Task ImportMovementsFromCsvAsync(string filePath);
        Task<string> GenerateMovementReportAsync(
            DateTime startDate,
            DateTime endDate,
            string movementType = null);

        // Auditavimas ir validavimas
        Task<bool> ValidateMovementAsync(ProductMovementDto movement);
        Task<bool> CancelMovementAsync(int movementId, string reason, string canceledByUser);
        Task<bool> AdjustMovementAsync(
            int movementId,
            decimal newQuantity,
            string reason,
            string adjustedByUser);
    }
}

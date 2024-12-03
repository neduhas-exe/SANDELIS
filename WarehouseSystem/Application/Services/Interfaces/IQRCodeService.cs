using Presentation.DTOs.Products;

namespace WarehouseSystem.Services.Interfaces
{
    /// <summary>
    /// QR kodų generavimo ir valdymo serviso interfeisas
    /// </summary>
    public interface IQRCodeService
    {
        // QR kodų generavimas
        Task<string> GenerateProductQRAsync(int productId);
        Task<List<string>> GenerateBatchQRCodesAsync(int productId, string batchNumber, int quantity);
        Task<List<string>> GenerateUniqueItemQRCodesAsync(int productId, string batchNumber, int quantity);

        // QR kodų paieška ir validavimas
        Task<ProductQRCodeDto> GetQRCodeInfoAsync(string qrCodeId);
        Task<bool> ValidateQRCodeAsync(string qrCodeId);
        Task<bool> IsQRCodeActiveAsync(string qrCodeId);

        // QR kodų būsenų valdymas
        Task<bool> ActivateQRCodeAsync(string qrCodeId, string activatedByUser);
        Task<bool> DeactivateQRCodeAsync(string qrCodeId, string deactivatedByUser, string reason);
        Task<bool> MarkQRCodeAsUsedAsync(string qrCodeId, string usedByUser, string reference);
        Task<bool> MarkQRCodeAsDefectiveAsync(string qrCodeId, string markedByUser, string reason);

        // QR kodų susiejimas
        Task<bool> LinkQRCodeToProductAsync(string qrCodeId, int productId);
        Task<bool> LinkQRCodeToBatchAsync(string qrCodeId, string batchNumber);
        Task<bool> UnlinkQRCodeAsync(string qrCodeId, string unlinkedByUser);

        // QR kodų istorija
        Task<IEnumerable<UpdateQRCodeStatusDto>> GetQRCodeHistoryAsync(string qrCodeId);
        Task<IEnumerable<ProductQRCodeDto>> GetBatchQRCodesAsync(string batchNumber);
        Task<IEnumerable<ProductQRCodeDto>> GetProductQRCodesAsync(int productId);

        // QR kodų statistika
        Task<IDictionary<string, int>> GetQRCodeStatusCountsAsync();
        Task<int> GetActiveQRCodesCountAsync(int productId);
        Task<int> GetUsedQRCodesCountAsync(int productId);

        // CSV operacijos
        Task<string> ExportQRCodesToCsvAsync(int? productId = null);
        Task<bool> ImportQRCodesFromCsvAsync(string csvContent);
        Task<string> GenerateQRCodeReportAsync(DateTime startDate, DateTime endDate);
    }
}

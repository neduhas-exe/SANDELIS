using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Services
{
    public class QRCodeService : IQRCodeService
    {
        private readonly ILogger<QRCodeService> _logger;
        private readonly string _qrCodesFilePath;
        private readonly string _qrHistoryFilePath;
        private static readonly SemaphoreSlim _csvLock = new(1, 1);

        public QRCodeService(ILogger<QRCodeService> logger)
        {
            _logger = logger;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _qrCodesFilePath = Path.Combine(baseDirectory, "Data", "qr_codes.csv");
            _qrHistoryFilePath = Path.Combine(baseDirectory, "Data", "qr_history.csv");

            Directory.CreateDirectory(Path.Combine(baseDirectory, "Data"));
            InitializeCSVFiles();
        }

        private void InitializeCSVFiles()
        {
            if (!File.Exists(_qrCodesFilePath))
            {
                File.WriteAllText(_qrCodesFilePath, ProductQRCodeDto.GetCsvHeader());
            }
            if (!File.Exists(_qrHistoryFilePath))
            {
                File.WriteAllText(_qrHistoryFilePath, "QRCodeId,OldStatus,NewStatus,ChangedBy,ChangeReason,ChangeDate");
            }
        }

        // Pagalbinės funkcijos CSV darbui
        private async Task<List<string>> ReadAllLinesAsync(string filePath)
        {
            await _csvLock.WaitAsync();
            try
            {
                using var reader = new StreamReader(filePath);
                var lines = new List<string>();
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
                return lines;
            }
            finally
            {
                _csvLock.Release();
            }
        }

        private async Task WriteAllLinesAsync(string filePath, IEnumerable<string> lines)
        {
            await _csvLock.WaitAsync();
            try
            {
                await File.WriteAllLinesAsync(filePath, lines);
            }
            finally
            {
                _csvLock.Release();
            }
        }

        // QR kodų generavimas
        public async Task<string> GenerateProductQRAsync(int productId)
        {
            var qrCode = new ProductQRCodeDto
            {
                QRCodeId = GenerateUniqueQRCode(),
                ProductId = productId,
                QRCodeType = "Product",
                Status = "Active",
                StatusChangedAt = DateTime.Now
            };

            await SaveQRCodeAsync(qrCode);
            return qrCode.QRCodeId;
        }

        public async Task<List<string>> GenerateBatchQRCodesAsync(
            int productId, 
            string batchNumber, 
            int quantity)
        {
            var qrCodes = new List<string>();
            for (int i = 0; i < quantity; i++)
            {
                var qrCode = new ProductQRCodeDto
                {
                    QRCodeId = GenerateUniqueQRCode(),
                    ProductId = productId,
                    BatchNumber = batchNumber,
                    QRCodeType = "Batch",
                    Status = "Active",
                    StatusChangedAt = DateTime.Now
                };

                await SaveQRCodeAsync(qrCode);
                qrCodes.Add(qrCode.QRCodeId);
            }

            return qrCodes;
        }

        public async Task<List<string>> GenerateUniqueItemQRCodesAsync(
            int productId, 
            string batchNumber, 
            int quantity)
        {
            var qrCodes = new List<string>();
            for (int i = 0; i < quantity; i++)
            {
                var qrCode = new ProductQRCodeDto
                {
                    QRCodeId = GenerateUniqueQRCode(),
                    ProductId = productId,
                    BatchNumber = batchNumber,
                    QRCodeType = "Individual",
                    Status = "Active",
                    StatusChangedAt = DateTime.Now
                };

                await SaveQRCodeAsync(qrCode);
                qrCodes.Add(qrCode.QRCodeId);
            }

            return qrCodes;
        }

        // QR kodų paieška ir validavimas
        public async Task<ProductQRCodeDto> GetQRCodeInfoAsync(string qrCodeId)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            var qrCodeLine = lines.Skip(1)
                                .FirstOrDefault(l => l.StartsWith($"{qrCodeId},"));

            return qrCodeLine != null ? ParseQRCodeLine(qrCodeLine) : null;
        }

        public async Task<bool> ValidateQRCodeAsync(string qrCodeId)
        {
            var qrCode = await GetQRCodeInfoAsync(qrCodeId);
            return qrCode != null;
        }

        public async Task<bool> IsQRCodeActiveAsync(string qrCodeId)
        {
            var qrCode = await GetQRCodeInfoAsync(qrCodeId);
            return qrCode?.Status == "Active";
        }

        // QR kodų būsenų valdymas
        public async Task<bool> ActivateQRCodeAsync(string qrCodeId, string activatedByUser)
        {
            return await UpdateQRCodeStatusAsync(qrCodeId, "Active", activatedByUser, "Activation");
        }

        public async Task<bool> DeactivateQRCodeAsync(string qrCodeId, string deactivatedByUser, string reason)
        {
            return await UpdateQRCodeStatusAsync(qrCodeId, "Inactive", deactivatedByUser, reason);
        }

        public async Task<bool> MarkQRCodeAsUsedAsync(string qrCodeId, string usedByUser, string reference)
        {
            return await UpdateQRCodeStatusAsync(qrCodeId, "Used", usedByUser, $"Used in {reference}");
        }

        public async Task<bool> MarkQRCodeAsDefectiveAsync(string qrCodeId, string markedByUser, string reason)
        {
            return await UpdateQRCodeStatusAsync(qrCodeId, "Defective", markedByUser, reason);
        }

        // QR kodų susiejimas
        public async Task<bool> LinkQRCodeToProductAsync(string qrCodeId, int productId)
        {
            var qrCode = await GetQRCodeInfoAsync(qrCodeId);
            if (qrCode == null)
            {
                return false;
            }

            qrCode.ProductId = productId;
            return await UpdateQRCodeAsync(qrCode);
        }

        public async Task<bool> LinkQRCodeToBatchAsync(string qrCodeId, string batchNumber)
        {
            var qrCode = await GetQRCodeInfoAsync(qrCodeId);
            if (qrCode == null)
            {
                return false;
            }

            qrCode.BatchNumber = batchNumber;
            return await UpdateQRCodeAsync(qrCode);
        }

        public async Task<bool> UnlinkQRCodeAsync(string qrCodeId, string unlinkedByUser)
        {
            var qrCode = await GetQRCodeInfoAsync(qrCodeId);
            if (qrCode == null)
            {
                return false;
            }

            qrCode.ProductId = 0;
            qrCode.BatchNumber = null;
            await UpdateQRCodeStatusAsync(qrCodeId, "Unlinked", unlinkedByUser, "Unlinked from product");
            return await UpdateQRCodeAsync(qrCode);
        }

        // QR kodų istorija
        public async Task<IEnumerable<UpdateQRCodeStatusDto>> GetQRCodeHistoryAsync(string qrCodeId)
        {
            var lines = await ReadAllLinesAsync(_qrHistoryFilePath);
            return lines.Skip(1)
                       .Where(l => l.StartsWith($"{qrCodeId},"))
                       .Select(ParseQRHistoryLine)
                       .Where(h => h != null)
                       .OrderByDescending(h => h.UpdatedAt);
        }

        public async Task<IEnumerable<ProductQRCodeDto>> GetBatchQRCodesAsync(string batchNumber)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            return lines.Skip(1)
                       .Where(l => l.Contains($",{batchNumber},"))
                       .Select(ParseQRCodeLine)
                       .Where(q => q != null);
        }

        public async Task<IEnumerable<ProductQRCodeDto>> GetProductQRCodesAsync(int productId)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            return lines.Skip(1)
                       .Where(l => l.Contains($",{productId},"))
                       .Select(ParseQRCodeLine)
                       .Where(q => q != null);
        }

        // QR kodų statistika
        public async Task<IDictionary<string, int>> GetQRCodeStatusCountsAsync()
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            return lines.Skip(1)
                       .Select(ParseQRCodeLine)
                       .Where(q => q != null)
                       .GroupBy(q => q.Status)
                       .ToDictionary(
                           g => g.Key,
                           g => g.Count()
                       );
        }

        public async Task<int> GetActiveQRCodesCountAsync(int productId)
        {
            var qrCodes = await GetProductQRCodesAsync(productId);
            return qrCodes.Count(q => q.Status == "Active");
        }

        public async Task<int> GetUsedQRCodesCountAsync(int productId)
        {
            var qrCodes = await GetProductQRCodesAsync(productId);
            return qrCodes.Count(q => q.Status == "Used");
        }

        // CSV operacijos
        public async Task<string> ExportQRCodesToCsvAsync(int? productId = null)
        {
            var qrCodes = productId.HasValue
                ? await GetProductQRCodesAsync(productId.Value)
                : await GetAllQRCodesAsync();

            var sb = new StringBuilder();
            sb.AppendLine(ProductQRCodeDto.GetCsvHeader());
            foreach (var qrCode in qrCodes)
            {
                sb.AppendLine(qrCode.ToCsvLine());
            }

            return sb.ToString();
        }

        public async Task<bool> ImportQRCodesFromCsvAsync(string csvContent)
        {
            try
            {
                var lines = csvContent.Split('\n');
                var header = lines.First().Trim();

                if (header != ProductQRCodeDto.GetCsvHeader())
                {
                    throw new InvalidOperationException("CSV failo struktūra neatitinka reikalaujamos struktūros");
                }

                var existingQRCodes = await GetAllQRCodesAsync();
                var existingIds = existingQRCodes.Select(q => q.QRCodeId).ToHashSet();

                var newQRCodes = lines.Skip(1)
                                    .Select(l => ParseQRCodeLine(l))
                                    .Where(q => q != null && !existingIds.Contains(q.QRCodeId));

                foreach (var qrCode in newQRCodes)
                {
                    await SaveQRCodeAsync(qrCode);
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida importuojant QR kodus iš CSV");
                return false;
            }
        }

        public async Task<string> GenerateQRCodeReportAsync(DateTime startDate, DateTime endDate)
        {
            var qrCodes = await GetAllQRCodesAsync();
            var history = await GetAllQRCodeHistoryAsync();

            var report = new StringBuilder();
            report.AppendLine($"QR kodų ataskaita ({startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd})");
            report.AppendLine("----------------------------------------");

            // Statusų statistika
            var statusCounts = qrCodes.GroupBy(q => q.Status)
                                    .ToDictionary(g => g.Key, g => g.Count());

            report.AppendLine("\nQR kodų statusai:");
            foreach (var status in statusCounts)
            {
                report.AppendLine($"{status.Key}: {status.Value}");
            }

            // Dienos aktyvumas
            var dailyActivity = history.Where(h => h.UpdatedAt >= startDate && h.UpdatedAt <= endDate)
                                     .GroupBy(h => h.UpdatedAt.Date)
                                     .OrderBy(g => g.Key);

            report.AppendLine("\nDienos aktyvumas:");
            foreach (var day in dailyActivity)
            {
                report.AppendLine($"{day.Key:yyyy-MM-dd}: {day.Count()} pakeitimai");
            }

            // Tipo statistika
            var typeCounts = qrCodes.GroupBy(q => q.QRCodeType)
                                  .ToDictionary(g => g.Key, g => g.Count());

            report.AppendLine("\nQR kodų tipai:");
            foreach (var type in typeCounts)
            {
                report.AppendLine($"{type.Key}: {type.Value}");
            }

            return report.ToString();
        }

        // Privačios pagalbinės funkcijos
        private string GenerateUniqueQRCode()
        {
            return Guid.NewGuid().ToString("N");
        }

        private async Task<bool> UpdateQRCodeStatusAsync(
            string qrCodeId,
            string newStatus,
            string changedByUser,
            string reason)
        {
            var qrCode = await GetQRCodeInfoAsync(qrCodeId);
            if (qrCode == null)
            {
                return false;
            }

            var oldStatus = qrCode.Status;
            qrCode.Status = newStatus;
            qrCode.StatusChangedAt = DateTime.Now;
            qrCode.StatusChangedBy = changedByUser;

            if (await UpdateQRCodeAsync(qrCode))
            {
                await LogStatusChangeAsync(qrCodeId, oldStatus, newStatus, changedByUser, reason);
                return true;
            }

            return false;
        }

        private async Task LogStatusChangeAsync(
            string qrCodeId,
            string oldStatus,
            string newStatus,
            string changedByUser,
            string reason)
        {
            var historyLine = $"{qrCodeId},{oldStatus},{newStatus},{changedByUser},\"{reason}\",{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
            var lines = await ReadAllLinesAsync(_qrHistoryFilePath);
            lines.Add(historyLine);
            await WriteAllLinesAsync(_qrHistoryFilePath, lines);
        }

        private async Task<bool> UpdateQRCodeAsync(ProductQRCodeDto qrCode)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            var qrCodeLines = lines.ToList();
            var index = qrCodeLines.FindIndex(l => l.StartsWith($"{qrCode.QRCodeId},"));

            if (index == -1)
            {
                return false;
            }

            qrCodeLines[index] = qrCode.ToCsvLine();
            await WriteAllLinesAsync(_qrCodesFilePath, qrCodeLines);
            return true;
        }

        private async Task<bool> SaveQRCodeAsync(ProductQRCodeDto qrCode)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            lines.Add(qrCode.ToCsvLine());
            await WriteAllLinesAsync(_qrCodesFilePath, lines);
            return true;
        }

        private async Task<IEnumerable<ProductQRCodeDto>> GetAllQRCodesAsync()
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            return lines.Skip(1)
                       .Select(ParseQRCodeLine)
                       .Where(q => q != null);
        }

        private async Task<IEnumerable<UpdateQRCodeStatusDto>> GetAllQRCodeHistoryAsync()
        {
            var lines = await ReadAllLinesAsync(_qrHistoryFilePath);
            return lines.Skip(1)
                       .Select(ParseQRHistoryLine)
                       .Where(h => h != null);
        }

        private ProductQRCodeDto ParseQRCodeLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new ProductQRCodeDto
                {
                    QRCodeId = parts[0],
                    ProductId = int.Parse(parts[1]),
                    BatchNumber = parts[2],
                    Quantity = decimal.Parse(parts[3]),
                    QRCodeType = parts[4].Trim('"'),
                    ReceivedDate = DateTime.Parse(parts[5]),
                    ReceivedByUser = parts[6].Trim('"'),
                    WarehouseLocation = parts[7].Trim('"'),
                    PurchaseInvoice = parts[8].Trim('"'),
                    SupplierName = parts[9].Trim('"'),
                    Status = parts[17].Trim('"'),
                    StatusChangedAt = DateTime.Parse(parts[18]),
                    StatusChangedBy = parts[19].Trim('"')
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant QR kodo CSV eilutę: {Line}", line);
                return null;
            }
        }

        private UpdateQRCodeStatusDto ParseQRHistoryLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new UpdateQRCodeStatusDto
                {
                    QRCodeId = parts[0],
                    NewStatus = parts[2].Trim('"'),
                    UpdatedByUser = parts[3].Trim('"'),
                    UpdateReason = parts[4].Trim('"'),
                    UpdatedAt = DateTime.Parse(parts[5])
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant QR kodo istorijos CSV eilutę: {Line}", line);
                return null;
            }
        }
    }
}

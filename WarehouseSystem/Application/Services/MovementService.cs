using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Services
{
    public class MovementService : IMovementService
    {
        private readonly ILogger<MovementService> _logger;
        private readonly string _movementsFilePath;
        private readonly string _movementHistoryFilePath;
        private static readonly SemaphoreSlim _csvLock = new(1, 1);

        public MovementService(ILogger<MovementService> logger)
        {
            _logger = logger;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _movementsFilePath = Path.Combine(baseDirectory, "Data", "movements.csv");
            _movementHistoryFilePath = Path.Combine(baseDirectory, "Data", "movement_history.csv");

            Directory.CreateDirectory(Path.Combine(baseDirectory, "Data"));
            InitializeCSVFiles();
        }

        private void InitializeCSVFiles()
        {
            if (!File.Exists(_movementsFilePath))
            {
                File.WriteAllText(_movementsFilePath, ProductMovementDto.GetCsvHeader());
            }
            if (!File.Exists(_movementHistoryFilePath))
            {
                File.WriteAllText(_movementHistoryFilePath, 
                    "MovementId,ChangeType,OldQuantity,NewQuantity,ChangedBy,ChangeReason,ChangeDate");
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

        // Pagrindinės judėjimo operacijos
        public async Task<ProductMovementDto> RegisterMovementAsync(
            int productId,
            string movementType,
            decimal quantity,
            string sourceLocation,
            string destinationLocation,
            string referenceNumber,
            string qrCodeId = null)
        {
            try
            {
                var movement = new ProductMovementDto
                {
                    MovementId = await GetNextMovementIdAsync(),
                    ProductId = productId,
                    QRCodeId = qrCodeId,
                    MovementType = movementType,
                    Quantity = quantity,
                    SourceLocation = sourceLocation,
                    DestinationLocation = destinationLocation,
                    ReferenceNumber = referenceNumber,
                    MovementDate = DateTime.Now
                };

                if (await ValidateMovementAsync(movement))
                {
                    await SaveMovementAsync(movement);
                    return movement;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida registruojant judėjimą");
                return null;
            }
        }

        public async Task<ProductMovementDto> GetMovementByIdAsync(int movementId)
        {
            var lines = await ReadAllLinesAsync(_movementsFilePath);
            var movementLine = lines.Skip(1)
                                  .FirstOrDefault(l => l.StartsWith($"{movementId},"));

            return movementLine != null ? ParseMovementLine(movementLine) : null;
        }

        public async Task<IEnumerable<ProductMovementDto>> GetMovementsAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetAllMovementsAsync();
            return FilterMovementsByDate(movements, startDate, endDate);
        }

        // Produkto judėjimų operacijos
        public async Task<IEnumerable<ProductMovementDto>> GetProductMovementsAsync(
            int productId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetAllMovementsAsync();
            return FilterMovementsByDate(
                movements.Where(m => m.ProductId == productId),
                startDate,
                endDate
            );
        }

        public async Task<decimal> GetProductTotalIncomingAsync(
            int productId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetProductMovementsAsync(productId, startDate, endDate);
            return movements.Where(m => m.MovementType == "IN")
                          .Sum(m => m.Quantity);
        }

        public async Task<decimal> GetProductTotalOutgoingAsync(
            int productId,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetProductMovementsAsync(productId, startDate, endDate);
            return movements.Where(m => m.MovementType == "OUT")
                          .Sum(m => m.Quantity);
        }

        // QR kodo judėjimų operacijos
        public async Task<IEnumerable<ProductMovementDto>> GetQRCodeMovementsAsync(string qrCodeId)
        {
            var movements = await GetAllMovementsAsync();
            return movements.Where(m => m.QRCodeId == qrCodeId)
                          .OrderByDescending(m => m.MovementDate);
        }

        public async Task<string> GetQRCodeCurrentLocationAsync(string qrCodeId)
        {
            var movements = await GetQRCodeMovementsAsync(qrCodeId);
            var lastMovement = movements.FirstOrDefault();
            
            if (lastMovement == null)
                return null;

            return lastMovement.MovementType == "OUT" 
                ? null 
                : lastMovement.DestinationLocation;
        }

        public async Task<DateTime?> GetQRCodeLastMovementDateAsync(string qrCodeId)
        {
            var movements = await GetQRCodeMovementsAsync(qrCodeId);
            return movements.FirstOrDefault()?.MovementDate;
        }

        // Lokacijos judėjimų operacijos
        public async Task<IEnumerable<ProductMovementDto>> GetLocationMovementsAsync(
            string locationCode,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetAllMovementsAsync();
            var locationMovements = movements.Where(m => 
                m.SourceLocation == locationCode || 
                m.DestinationLocation == locationCode
            );
            
            return FilterMovementsByDate(locationMovements, startDate, endDate);
        }

        public async Task<decimal> GetLocationTotalIncomingAsync(
            string locationCode,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetLocationMovementsAsync(locationCode, startDate, endDate);
            return movements.Where(m => m.DestinationLocation == locationCode)
                          .Sum(m => m.Quantity);
        }

        public async Task<decimal> GetLocationTotalOutgoingAsync(
            string locationCode,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetLocationMovementsAsync(locationCode, startDate, endDate);
            return movements.Where(m => m.SourceLocation == locationCode)
                          .Sum(m => m.Quantity);
        }

        // Judėjimų paieška ir filtravimas
        public async Task<IEnumerable<ProductMovementDto>> SearchMovementsAsync(
            string searchTerm,
            string movementType = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<ProductMovementDto>();

            searchTerm = searchTerm.ToLower();
            var movements = await GetAllMovementsAsync();
            
            var filteredMovements = movements.Where(m =>
                m.ReferenceNumber?.ToLower().Contains(searchTerm) == true ||
                m.QRCodeId?.ToLower().Contains(searchTerm) == true ||
                m.SourceLocation?.ToLower().Contains(searchTerm) == true ||
                m.DestinationLocation?.ToLower().Contains(searchTerm) == true ||
                m.Notes?.ToLower().Contains(searchTerm) == true
            );

            if (!string.IsNullOrWhiteSpace(movementType))
            {
                filteredMovements = filteredMovements.Where(m => 
                    m.MovementType.Equals(movementType, StringComparison.OrdinalIgnoreCase)
                );
            }

            return FilterMovementsByDate(filteredMovements, startDate, endDate);
        }

        public async Task<IEnumerable<ProductMovementDto>> GetMovementsByTypeAsync(
            string movementType,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetAllMovementsAsync();
            var typeMovements = movements.Where(m => 
                m.MovementType.Equals(movementType, StringComparison.OrdinalIgnoreCase)
            );
            
            return FilterMovementsByDate(typeMovements, startDate, endDate);
        }

        // Judėjimų statistika ir analizė
        public async Task<IDictionary<string, int>> GetMovementCountsByTypeAsync(
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetMovementsAsync(startDate, endDate);
            return movements.GroupBy(m => m.MovementType)
                          .ToDictionary(
                              g => g.Key,
                              g => g.Count()
                          );
        }

        public async Task<IDictionary<DateTime, decimal>> GetDailyMovementTotalsAsync(
            string movementType,
            DateTime startDate,
            DateTime endDate)
        {
            var movements = await GetMovementsByTypeAsync(movementType, startDate, endDate);
            return movements.GroupBy(m => m.MovementDate.Date)
                          .ToDictionary(
                              g => g.Key,
                              g => g.Sum(m => m.Quantity)
                          );
        }

        public async Task<IEnumerable<ProductMovementDto>> GetTopMovingProductsAsync(
            int count,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetMovementsAsync(startDate, endDate);
            return movements.GroupBy(m => m.ProductId)
                          .Select(g => new
                          {
                              ProductId = g.Key,
                              TotalQuantity = g.Sum(m => m.Quantity)
                          })
                          .OrderByDescending(x => x.TotalQuantity)
                          .Take(count)
                          .Select(x => movements.First(m => m.ProductId == x.ProductId));
        }

        // CSV operacijos
        public async Task ExportMovementsToCsvAsync(
            string filePath,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var movements = await GetMovementsAsync(startDate, endDate);
            var lines = new List<string> { ProductMovementDto.GetCsvHeader() };
            lines.AddRange(movements.Select(m => m.ToCsvLine()));
            await File.WriteAllLinesAsync(filePath, lines);
        }

        public async Task ImportMovementsFromCsvAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var header = lines.First();
            
            if (header != ProductMovementDto.GetCsvHeader())
            {
                throw new InvalidOperationException("CSV failo struktūra neatitinka reikalaujamos struktūros");
            }

            var currentMovements = await GetAllMovementsAsync();
            var existingIds = currentMovements.Select(m => m.MovementId).ToHashSet();

            var newMovements = lines.Skip(1)
                                  .Select(ParseMovementLine)
                                  .Where(m => m != null && !existingIds.Contains(m.MovementId));

            foreach (var movement in newMovements)
            {
                await SaveMovementAsync(movement);
            }
        }

        public async Task<string> GenerateMovementReportAsync(
            DateTime startDate,
            DateTime endDate,
            string movementType = null)
        {
            var movements = await GetMovementsAsync(startDate, endDate);
            if (!string.IsNullOrWhiteSpace(movementType))
            {
                movements = movements.Where(m => 
                    m.MovementType.Equals(movementType, StringComparison.OrdinalIgnoreCase)
                );
            }

            var report = new StringBuilder();
            report.AppendLine("\nTop 5 lokacijos:");
            foreach (var loc in topLocations)
            {
                report.AppendLine($"{loc.Location}: {loc.Count} judėjimai, {loc.Quantity:N2} vnt.");
            }

            return report.ToString();
        }

        // Auditavimas ir validavimas
        public async Task<bool> ValidateMovementAsync(ProductMovementDto movement)
        {
            if (movement == null)
                return false;

            if (movement.Quantity <= 0)
            {
                _logger.LogWarning("Kiekis turi būti teigiamas");
                return false;
            }

            switch (movement.MovementType?.ToUpper())
            {
                case "IN":
                    if (string.IsNullOrEmpty(movement.DestinationLocation))
                    {
                        _logger.LogWarning("Gavimo lokacija privaloma IN tipo judėjimui");
                        return false;
                    }
                    break;

                case "OUT":
                    if (string.IsNullOrEmpty(movement.SourceLocation))
                    {
                        _logger.LogWarning("Išsiuntimo lokacija privaloma OUT tipo judėjimui");
                        return false;
                    }
                    break;

                case "TRANSFER":
                    if (string.IsNullOrEmpty(movement.SourceLocation) || 
                        string.IsNullOrEmpty(movement.DestinationLocation))
                    {
                        _logger.LogWarning("Abu lokacijos laukai privalomi TRANSFER tipo judėjimui");
                        return false;
                    }
                    break;

                default:
                    _logger.LogWarning($"Neatpažintas judėjimo tipas: {movement.MovementType}");
                    return false;
            }

            return true;
        }

        public async Task<bool> CancelMovementAsync(
            int movementId, 
            string reason, 
            string canceledByUser)
        {
            var movement = await GetMovementByIdAsync(movementId);
            if (movement == null)
                return false;

            // Sukuriame atvirkštinį judėjimą
            var reversalMovement = new ProductMovementDto
            {
                MovementId = await GetNextMovementIdAsync(),
                ProductId = movement.ProductId,
                QRCodeId = movement.QRCodeId,
                MovementType = GetReversalType(movement.MovementType),
                Quantity = movement.Quantity,
                SourceLocation = movement.DestinationLocation,
                DestinationLocation = movement.SourceLocation,
                ReferenceNumber = $"CANCEL-{movement.MovementId}",
                Notes = $"Atšauktas judėjimas {movement.MovementId}: {reason}",
                MovedByUser = canceledByUser,
                MovementDate = DateTime.Now
            };

            await SaveMovementAsync(reversalMovement);
            await LogMovementChangeAsync(
                movementId,
                "CANCEL",
                movement.Quantity,
                0,
                canceledByUser,
                reason
            );

            return true;
        }

        public async Task<bool> AdjustMovementAsync(
            int movementId,
            decimal newQuantity,
            string reason,
            string adjustedByUser)
        {
            var movement = await GetMovementByIdAsync(movementId);
            if (movement == null || newQuantity <= 0)
                return false;

            var oldQuantity = movement.Quantity;
            movement.Quantity = newQuantity;
            movement.Notes = $"{movement.Notes}\nKoreguotas kiekis: {reason}";

            var lines = await ReadAllLinesAsync(_movementsFilePath);
            var movementLines = lines.ToList();
            var index = movementLines.FindIndex(l => l.StartsWith($"{movementId},"));

            if (index != -1)
            {
                movementLines[index] = movement.ToCsvLine();
                await WriteAllLinesAsync(_movementsFilePath, movementLines);
                
                await LogMovementChangeAsync(
                    movementId,
                    "ADJUST",
                    oldQuantity,
                    newQuantity,
                    adjustedByUser,
                    reason
                );

                return true;
            }

            return false;
        }

        // Privačios pagalbinės funkcijos
        private async Task<int> GetNextMovementIdAsync()
        {
            var lines = await ReadAllLinesAsync(_movementsFilePath);
            return lines.Skip(1)
                       .Select(l => int.Parse(l.Split(',')[0]))
                       .DefaultIfEmpty(0)
                       .Max() + 1;
        }

        private async Task<bool> SaveMovementAsync(ProductMovementDto movement)
        {
            if (!await ValidateMovementAsync(movement))
                return false;

            var lines = await ReadAllLinesAsync(_movementsFilePath);
            lines.Add(movement.ToCsvLine());
            await WriteAllLinesAsync(_movementsFilePath, lines);
            return true;
        }

        private async Task<IEnumerable<ProductMovementDto>> GetAllMovementsAsync()
        {
            var lines = await ReadAllLinesAsync(_movementsFilePath);
            return lines.Skip(1)
                       .Select(ParseMovementLine)
                       .Where(m => m != null);
        }

        private IEnumerable<ProductMovementDto> FilterMovementsByDate(
            IEnumerable<ProductMovementDto> movements,
            DateTime? startDate,
            DateTime? endDate)
        {
            if (startDate.HasValue)
            {
                movements = movements.Where(m => m.MovementDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                movements = movements.Where(m => m.MovementDate <= endDate.Value);
            }
            return movements;
        }

        private string GetReversalType(string originalType)
        {
            switch (originalType?.ToUpper())
            {
                case "IN": return "OUT";
                case "OUT": return "IN";
                case "TRANSFER": return "TRANSFER";
                default: return originalType;
            }
        }

        private async Task LogMovementChangeAsync(
            int movementId,
            string changeType,
            decimal oldQuantity,
            decimal newQuantity,
            string changedByUser,
            string reason)
        {
            var historyLine = $"{movementId},{changeType},{oldQuantity},{newQuantity}," +
                            $"\"{changedByUser}\",\"{reason}\",{DateTime.Now:yyyy-MM-dd HH:mm:ss}";

            var lines = await ReadAllLinesAsync(_movementHistoryFilePath);
            lines.Add(historyLine);
            await WriteAllLinesAsync(_movementHistoryFilePath, lines);
        }

        private ProductMovementDto ParseMovementLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new ProductMovementDto
                {
                    MovementId = int.Parse(parts[0]),
                    ProductId = int.Parse(parts[1]),
                    QRCodeId = parts[2],
                    MovementType = parts[3].Trim('"'),
                    Quantity = decimal.Parse(parts[4]),
                    UnitOfMeasure = parts[5].Trim('"'),
                    SourceLocation = parts[6].Trim('"'),
                    DestinationLocation = parts[7].Trim('"'),
                    MovementDate = DateTime.Parse(parts[8]),
                    MovedByUser = parts[9].Trim('"'),
                    ReferenceNumber = parts[10].Trim('"'),
                    Notes = parts[11].Trim('"')
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant judėjimo CSV eilutę: {Line}", line);
                return null;
            }
        }
    }
}endLine($"Judėjimų ataskaita ({startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd})");
            report.AppendLine("----------------------------------------");

            // Bendri kiekiai pagal tipą
            var typeStats = movements.GroupBy(m => m.MovementType)
                                   .Select(g => new
                                   {
                                       Type = g.Key,
                                       Count = g.Count(),
                                       TotalQuantity = g.Sum(m => m.Quantity)
                                   });

            report.AppendLine("\nJudėjimai pagal tipą:");
            foreach (var stat in typeStats)
            {
                report.AppendLine($"{stat.Type}: {stat.Count} judėjimai, {stat.TotalQuantity:N2} vnt.");
            }

            // Dienos statistika
            var dailyStats = movements.GroupBy(m => m.MovementDate.Date)
                                    .OrderBy(g => g.Key);

            report.AppendLine("\nDienos statistika:");
            foreach (var day in dailyStats)
            {
                var inQuantity = day.Where(m => m.MovementType == "IN").Sum(m => m.Quantity);
                var outQuantity = day.Where(m => m.MovementType == "OUT").Sum(m => m.Quantity);
                
                report.AppendLine($"{day.Key:yyyy-MM-dd}: IN: {inQuantity:N2}, OUT: {outQuantity:N2}");
            }

            // Top lokacijos
            var topLocations = movements.GroupBy(m => m.DestinationLocation)
                                      .Where(g => !string.IsNullOrEmpty(g.Key))
                                      .Select(g => new
                                      {
                                          Location = g.Key,
                                          Count = g.Count(),
                                          Quantity = g.Sum(m => m.Quantity)
                                      })
                                      .OrderByDescending(x => x.Count)
                                      .Take(5);

            report.App

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly ILogger<WarehouseService> _logger;
        private readonly string _locationsFilePath;
        private readonly string _movementsFilePath;
        private static readonly SemaphoreSlim _csvLock = new(1, 1);
        
        public WarehouseService(ILogger<WarehouseService> logger)
        {
            _logger = logger;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _locationsFilePath = Path.Combine(baseDirectory, "Data", "locations.csv");
            _movementsFilePath = Path.Combine(baseDirectory, "Data", "movements.csv");
            
            // Sukuriame Data direktoriją jei jos nėra
            Directory.CreateDirectory(Path.Combine(baseDirectory, "Data"));
            
            // Inicializuojame CSV failus
            InitializeCSVFiles();
        }

        private void InitializeCSVFiles()
        {
            if (!File.Exists(_locationsFilePath))
            {
                File.WriteAllText(_locationsFilePath, ProductLocationDto.GetCsvHeader());
            }
            if (!File.Exists(_movementsFilePath))
            {
                File.WriteAllText(_movementsFilePath, ProductMovementDto.GetCsvHeader());
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

        // Lokacijų operacijos
        public async Task<ProductLocationDto> GetLocationAsync(string warehouseId, string locationCode)
        {
            var lines = await ReadAllLinesAsync(_locationsFilePath);
            var locationLine = lines.Skip(1)
                                  .FirstOrDefault(l => l.StartsWith($"{warehouseId},") && 
                                                     GetLocationCode(l) == locationCode);

            return locationLine != null ? ParseLocationLine(locationLine) : null;
        }

        public async Task<IEnumerable<ProductLocationDto>> GetAllLocationsAsync(string warehouseId)
        {
            var lines = await ReadAllLinesAsync(_locationsFilePath);
            return lines.Skip(1)
                       .Where(l => l.StartsWith($"{warehouseId},"))
                       .Select(ParseLocationLine)
                       .Where(l => l != null);
        }

        public async Task<IEnumerable<ProductLocationDto>> GetProductLocationsAsync(int productId)
        {
            var lines = await ReadAllLinesAsync(_locationsFilePath);
            return lines.Skip(1)
                       .Where(l => l.Contains($",{productId},"))
                       .Select(ParseLocationLine)
                       .Where(l => l != null);
        }

        // Produktų judėjimo operacijos
        public async Task<bool> ReceiveProductAsync(
            int productId,
            string warehouseId,
            string locationCode,
            decimal quantity,
            string qrCodeId = null)
        {
            try
            {
                var location = await GetLocationAsync(warehouseId, locationCode);
                if (location == null)
                {
                    _logger.LogWarning($"Lokacija {locationCode} nerasta sandėlyje {warehouseId}");
                    return false;
                }

                // Atnaujiname lokacijos kiekį
                location.Quantity += quantity;
                await UpdateLocationQuantityAsync(location);

                // Registruojame judėjimą
                var movement = new ProductMovementDto
                {
                    MovementId = await GetNextMovementIdAsync(),
                    ProductId = productId,
                    QRCodeId = qrCodeId,
                    MovementType = "IN",
                    Quantity = quantity,
                    UnitOfMeasure = location.UnitOfMeasure,
                    DestinationLocation = locationCode,
                    MovementDate = DateTime.Now
                };

                await RegisterMovementAsync(movement);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Klaida priimant produktą {productId} į sandėlį");
                return false;
            }
        }

        public async Task<bool> TransferProductAsync(
            int productId,
            string sourceWarehouseId,
            string sourceLocationCode,
            string destinationWarehouseId,
            string destinationLocationCode,
            decimal quantity,
            string qrCodeId = null)
        {
            try
            {
                var sourceLocation = await GetLocationAsync(sourceWarehouseId, sourceLocationCode);
                var destLocation = await GetLocationAsync(destinationWarehouseId, destinationLocationCode);

                if (sourceLocation == null || destLocation == null)
                {
                    _logger.LogWarning("Viena iš lokacijų nerasta");
                    return false;
                }

                if (sourceLocation.Quantity < quantity)
                {
                    _logger.LogWarning("Nepakankamas kiekis šaltinio lokacijoje");
                    return false;
                }

                // Atnaujiname kiekius
                sourceLocation.Quantity -= quantity;
                destLocation.Quantity += quantity;

                await UpdateLocationQuantityAsync(sourceLocation);
                await UpdateLocationQuantityAsync(destLocation);

                // Registruojame judėjimą
                var movement = new ProductMovementDto
                {
                    MovementId = await GetNextMovementIdAsync(),
                    ProductId = productId,
                    QRCodeId = qrCodeId,
                    MovementType = "TRANSFER",
                    Quantity = quantity,
                    UnitOfMeasure = sourceLocation.UnitOfMeasure,
                    SourceLocation = sourceLocationCode,
                    DestinationLocation = destinationLocationCode,
                    MovementDate = DateTime.Now
                };

                await RegisterMovementAsync(movement);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida perkeliant produktą tarp lokacijų");
                return false;
            }
        }

        public async Task<bool> ShipProductAsync(
            int productId,
            string warehouseId,
            string locationCode,
            decimal quantity,
            string referenceNumber,
            string qrCodeId = null)
        {
            try
            {
                var location = await GetLocationAsync(warehouseId, locationCode);
                if (location == null || location.Quantity < quantity)
                {
                    return false;
                }

                // Atnaujiname kiekį
                location.Quantity -= quantity;
                await UpdateLocationQuantityAsync(location);

                // Registruojame judėjimą
                var movement = new ProductMovementDto
                {
                    MovementId = await GetNextMovementIdAsync(),
                    ProductId = productId,
                    QRCodeId = qrCodeId,
                    MovementType = "OUT",
                    Quantity = quantity,
                    UnitOfMeasure = location.UnitOfMeasure,
                    SourceLocation = locationCode,
                    ReferenceNumber = referenceNumber,
                    MovementDate = DateTime.Now
                };

                await RegisterMovementAsync(movement);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida išsiunčiant produktą iš sandėlio");
                return false;
            }
        }

        // Inventorizacijos operacijos
        public async Task<bool> AdjustStockAsync(
            int productId,
            string warehouseId,
            string locationCode,
            decimal newQuantity,
            string reason,
            string adjustedByUser)
        {
            try
            {
                var location = await GetLocationAsync(warehouseId, locationCode);
                if (location == null)
                {
                    return false;
                }

                var difference = newQuantity - location.Quantity;
                location.Quantity = newQuantity;
                await UpdateLocationQuantityAsync(location);

                // Registruojame koregavimo judėjimą
                var movement = new ProductMovementDto
                {
                    MovementId = await GetNextMovementIdAsync(),
                    ProductId = productId,
                    MovementType = "ADJUSTMENT",
                    Quantity = Math.Abs(difference),
                    UnitOfMeasure = location.UnitOfMeasure,
                    SourceLocation = locationCode,
                    Notes = $"Koregavimas: {reason}",
                    MovedByUser = adjustedByUser,
                    MovementDate = DateTime.Now
                };

                await RegisterMovementAsync(movement);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida koreguojant produkto kiekį");
                return false;
            }
        }

        public async Task<ProductLocationDto> CountStockAsync(
            int productId,
            string warehouseId,
            string locationCode,
            decimal countedQuantity,
            string countedByUser)
        {
            try
            {
                var location = await GetLocationAsync(warehouseId, locationCode);
                if (location == null)
                {
                    return null;
                }

                var difference = countedQuantity - location.Quantity;
                if (difference != 0)
                {
                    await AdjustStockAsync(
                        productId,
                        warehouseId,
                        locationCode,
                        countedQuantity,
                        "Inventorizacija",
                        countedByUser
                    );
                }

                return await GetLocationAsync(warehouseId, locationCode);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atliekant inventorizaciją");
                return null;
            }
        }

        // Sandėlio vietos valdymas
        public async Task<bool> CreateLocationAsync(
            string warehouseId,
            string zone,
            string aisle,
            string rack,
            string shelf,
            string bin,
            decimal maxCapacity,
            string storageConditions = null)
        {
            try
            {
                var locationCode = GenerateLocationCode(zone, aisle, rack, shelf, bin);
                var existingLocation = await GetLocationAsync(warehouseId, locationCode);
                
                if (existingLocation != null)
                {
                    return false;
                }

                var newLocation = new ProductLocationDto
                {
                    WarehouseId = warehouseId,
                    Zone = zone,
                    Aisle = aisle,
                    Rack = rack,
                    Shelf = shelf,
                    Bin = bin,
                    MaxCapacity = maxCapacity,
                    StorageConditions = storageConditions,
                    LastUpdated = DateTime.Now
                };

                var lines = await ReadAllLinesAsync(_locationsFilePath);
                lines.Add(newLocation.ToCsvLine());
                await WriteAllLinesAsync(_locationsFilePath, lines);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant naują sandėlio vietą");
                return false;
            }
        }

        public async Task<bool> UpdateLocationAsync(
            string warehouseId,
            string locationCode,
            decimal? maxCapacity = null,
            string storageConditions = null,
            bool? isQuarantine = null)
        {
            try
            {
                var lines = await ReadAllLinesAsync(_locationsFilePath);
                var locationLines = lines.ToList();
                var index = locationLines.FindIndex(l => 
                    l.StartsWith($"{warehouseId},") && GetLocationCode(l) == locationCode);

                if (index == -1)
                {
                    return false;
                }

                var location = ParseLocationLine(locationLines[index]);
                
                if (maxCapacity.HasValue)
                    location.MaxCapacity = maxCapacity.Value;
                if (storageConditions != null)
                    location.StorageConditions = storageConditions;
                if (isQuarantine.HasValue)
                    location.IsQuarantine = isQuarantine.Value;
                
                location.LastUpdated = DateTime.Now;

                locationLines[index] = location.ToCsvLine();
                await WriteAllLinesAsync(_locationsFilePath, locationLines);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant sandėlio vietą");
                return false;
            }
        }

        public async Task<bool> DeleteLocationAsync(string warehouseId, string locationCode)
        {
            try
            {
                var location = await GetLocationAsync(warehouseId, locationCode);
                if (location == null)
                {
                    return false;
                }

                if (location.Quantity > 0)
                {
                    _logger.LogWarning("Negalima ištrinti lokacijos, kurioje yra prekių");
                    return false;
                }

                var lines = await ReadAllLinesAsync(_locationsFilePath);
                var locationLines = lines.Where(l => 
                    !(l.StartsWith($"{warehouseId},") && GetLocationCode(l) == locationCode));
                
                await WriteAllLinesAsync(_locationsFilePath, locationLines);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant sandėlio vietą");
                return false;
            }
        }

        // Ataskaitos ir statistika
        public async Task<decimal> GetTotalStockQuantityAsync(int productId)
        {
            var locations = await GetProductLocationsAsync(productId);
            return locations.Sum(l => l.Quantity);
        }

        public async Task<decimal> GetLocationUtilizationAsync(string warehouseId, string locationCode)
        {
            var location = await GetLocationAsync(warehouseId, locationCode);
            if (location == null || location.MaxCapacity == 0)
            {
                return 0;
            }
            return (location.Quantity / location.MaxCapacity) * 100;
        }

        public async Task<IDictionary<string, decimal>> GetStockLevelsAsync(string warehouseId)
        {
            var locations = await GetAllLocationsAsync(warehouseId);
            return locations.GroupBy(l => l.Zone)
                          .ToDictionary(
                              g => g.Key,
                              g => g.Sum(l => l.Quantity)
                          );
        }

        public async Task<IEnumerable<ProductLocationDto>> GetLowStockLocationsAsync(string warehouseId)
        {
            var locations = await GetAllLocationsAsync(warehouseId);
            return locations.Where(l => 
                l.MinimumQuantity > 0 && 
                l.Quantity <= l.MinimumQuantity
            );
        }

        // CSV operacijos
        public async Task ExportLocationsToCsvAsync(string filePath, string warehouseId = null)
        {
            var locations = string.IsNullOrEmpty(warehouseId) 
                ? await GetAllLocationsAsync(warehouseId)
                : (await GetAllLocationsAsync(warehouseId)).Where(l => l.WarehouseId == warehouseId);

            var lines = new List<string> { ProductLocationDto.GetCsvHeader() };
            lines.AddRange(locations.Select(l => l.ToCsvLine()));
            await File.WriteAllLinesAsync(filePath, lines);
        }

        public async Task ImportLocationsFromCsvAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var header = lines.First();
            
            if (header != ProductLocationDto.GetCsvHeader())
            {
                throw new InvalidOperationException("CSV failo struktūra neatitinka reikalaujamos struktūros");
            }

            var currentLocations = await ReadAllLinesAsync(_locationsFilePath);
            var newLocations = lines.Skip(1)
                                  .Where(l => !currentLocations.Contains(l));

            currentLocations.AddRange(newLocations);
            await WriteAllLinesAsync(_locationsFilePath, currentLocations);
        }

        public async Task<string> GenerateStockReportAsync(string warehouseId, DateTime reportDate)
        {
            var locations = await GetAllLocationsAsync(warehouseId);
            var movements = await GetMovementsForDateAsync(warehouseId, reportDate);
            
            var report = new StringBuilder();
            report.AppendLine($"Sandėlio {warehouseId} ataskaita už {reportDate:yyyy-MM-dd}");
            report.AppendLine("----------------------------------------");
            
            // Bendri kiekiai pagal zonas
            var zoneStocks = locations.GroupBy(l => l.Zone)
                                    .Select(g => new
                                    {
                                        Zone = g.Key,
                                        TotalQuantity = g.Sum(l => l.Quantity),
                                        Utilization = g.Sum(l => l.Quantity) / g.Sum(l => l.MaxCapacity) * 100
                                    });

            report.AppendLine("\nKiekiai pagal zonas:");
            foreach (var stock in zoneStocks)
            {
                report.AppendLine($"Zona {stock.Zone}: {stock.TotalQuantity:N2} vnt. (Užpildymas: {stock.Utilization:N1}%)");
            }

            // Dienos judėjimai
            report.AppendLine("\nDienos judėjimai:");
            var dailyMovements = movements.GroupBy(m => m.MovementType)
                                        .Select(g => new
                                        {
                                            Type = g.Key,
                                            Count = g.Count(),
                                            TotalQuantity = g.Sum(m => m.Quantity)
                                        });

            foreach (var movement in dailyMovements)
            {
                report.AppendLine($"{movement.Type}: {movement.Count} judėjimai, {movement.TotalQuantity:N2} vnt.");
            }

            // Žemo likučio pozicijos
            var lowStock = locations.Where(l => l.MinimumQuantity > 0 && l.Quantity <= l.MinimumQuantity);
            if (lowStock.Any())
            {
                report.AppendLine("\nŽemo likučio pozicijos:");
                foreach (var location in lowStock)
                {
                    report.AppendLine($"Lokacija {GetLocationCode(location)}: {location.Quantity:N2} vnt. (min: {location.MinimumQuantity:N2})");
                }
            }

            return report.ToString();
        }

        // Pagalbinės funkcijos
        private string GetLocationCode(string csvLine)
        {
            var parts = csvLine.Split(',');
            return GenerateLocationCode(
                parts[1].Trim('"'),  // Zone
                parts[2].Trim('"'),  // Aisle
                parts[3].Trim('"'),  // Rack
                parts[4].Trim('"'),  // Shelf
                parts[5].Trim('"')   // Bin
            );
        }

        private string GetLocationCode(ProductLocationDto location)
        {
            return GenerateLocationCode(
                location.Zone,
                location.Aisle,
                location.Rack,
                location.Shelf,
                location.Bin
            );
        }

        private string GenerateLocationCode(string zone, string aisle, string rack, string shelf, string bin)
        {
            return $"{zone}-{aisle}-{rack}-{shelf}-{bin}";
        }

        private async Task<int> GetNextMovementIdAsync()
        {
            var lines = await ReadAllLinesAsync(_movementsFilePath);
            return lines.Skip(1)
                       .Select(l => int.Parse(l.Split(',')[0]))
                       .DefaultIfEmpty(0)
                       .Max() + 1;
        }

        private async Task RegisterMovementAsync(ProductMovementDto movement)
        {
            var lines = await ReadAllLinesAsync(_movementsFilePath);
            lines.Add(movement.ToCsvLine());
            await WriteAllLinesAsync(_movementsFilePath, lines);
        }

        private async Task UpdateLocationQuantityAsync(ProductLocationDto location)
        {
            var lines = await ReadAllLinesAsync(_locationsFilePath);
            var locationLines = lines.ToList();
            var index = locationLines.FindIndex(l => 
                l.StartsWith($"{location.WarehouseId},") && 
                GetLocationCode(l) == GetLocationCode(location));

            if (index != -1)
            {
                locationLines[index] = location.ToCsvLine();
                await WriteAllLinesAsync(_locationsFilePath, locationLines);
            }
        }

        private async Task<IEnumerable<ProductMovementDto>> GetMovementsForDateAsync(
            string warehouseId,
            DateTime date)
        {
            var lines = await ReadAllLinesAsync(_movementsFilePath);
            return lines.Skip(1)
                       .Select(ParseMovementLine)
                       .Where(m => m != null &&
                                 m.MovementDate.Date == date.Date &&
                                 (m.SourceLocation?.StartsWith($"{warehouseId}-") == true ||
                                  m.DestinationLocation?.StartsWith($"{warehouseId}-") == true));
        }

        private ProductLocationDto ParseLocationLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new ProductLocationDto
                {
                    WarehouseId = parts[0],
                    Zone = parts[1].Trim('"'),
                    Aisle = parts[2].Trim('"'),
                    Rack = parts[3].Trim('"'),
                    Shelf = parts[4].Trim('"'),
                    Bin = parts[5].Trim('"'),
                    Quantity = decimal.Parse(parts[6]),
                    MaxCapacity = decimal.Parse(parts[7]),
                    UnitOfMeasure = parts[8].Trim('"'),
                    MinimumQuantity = decimal.Parse(parts[9]),
                    IsQuarantine = bool.Parse(parts[10]),
                    StorageConditions = parts[11].Trim('"'),
                    LastUpdated = DateTime.Parse(parts[17])
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant lokacijos CSV eilutę: {Line}", line);
                return null;
            }
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
}

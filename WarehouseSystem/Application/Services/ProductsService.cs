using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Services
{
    public class ProductService : IProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly string _productsFilePath;
        private readonly string _qrCodesFilePath;
        private readonly string _movementsFilePath;
        private readonly string _locationsFilePath;
        
        // Naudojame cache greitesniam duomenų nuskaitymui
        private static readonly ConcurrentDictionary<int, ProductDto> _productsCache = new();
        private static readonly SemaphoreSlim _csvLock = new(1, 1);

        public ProductService(ILogger<ProductService> logger)
        {
            _logger = logger;
            // Nustatome kelią iki CSV failų
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _productsFilePath = Path.Combine(baseDirectory, "Data", "products.csv");
            _qrCodesFilePath = Path.Combine(baseDirectory, "Data", "qr_codes.csv");
            _movementsFilePath = Path.Combine(baseDirectory, "Data", "movements.csv");
            _locationsFilePath = Path.Combine(baseDirectory, "Data", "locations.csv");
            
            // Sukuriame Data direktoriją jei jos nėra
            Directory.CreateDirectory(Path.Combine(baseDirectory, "Data"));
            
            // Sukuriame pradinius CSV failus jei jų nėra
            InitializeCSVFiles();
        }

        private void InitializeCSVFiles()
        {
            if (!File.Exists(_productsFilePath))
            {
                File.WriteAllText(_productsFilePath, ProductDto.GetCsvHeader());
            }
            if (!File.Exists(_qrCodesFilePath))
            {
                File.WriteAllText(_qrCodesFilePath, ProductQRCodeDto.GetCsvHeader());
            }
            if (!File.Exists(_movementsFilePath))
            {
                File.WriteAllText(_movementsFilePath, ProductMovementDto.GetCsvHeader());
            }
            if (!File.Exists(_locationsFilePath))
            {
                File.WriteAllText(_locationsFilePath, ProductLocationDto.GetCsvHeader());
            }
        }

        // Pagalbinė funkcija saugiam CSV nuskaitymui
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

        // Pagalbinė funkcija saugiam CSV įrašymui
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

        // Produktų CRUD operacijų implementacija
        public async Task<ProductDto> GetProductByIdAsync(int id)
        {
            // Pirma tikriname cache
            if (_productsCache.TryGetValue(id, out var cachedProduct))
            {
                return cachedProduct;
            }

            var lines = await ReadAllLinesAsync(_productsFilePath);
            var productLine = lines.Skip(1) // Praleidžiame antraštę
                                 .FirstOrDefault(l => l.StartsWith($"{id},"));

            if (productLine == null)
            {
                return null;
            }

            // Čia reikės implementuoti CSV eilutės konvertavimą į ProductDto
            var product = ParseProductLine(productLine);
            _productsCache.TryAdd(id, product);
            return product;
        }

        public async Task<ProductDto> CreateProductAsync(CreateProductDto productDto)
        {
            var lines = await ReadAllLinesAsync(_productsFilePath);
            var lastId = lines.Skip(1)
                            .Select(l => int.Parse(l.Split(',')[0]))
                            .DefaultIfEmpty(0)
                            .Max();

            var newProduct = new ProductDto
            {
                Id = lastId + 1,
                Name = productDto.Name,
                NameEn = productDto.NameEn,
                // Užpildome likusius laukus...
            };

            var newLine = newProduct.ToCsvLine();
            lines.Add(newLine);
            await WriteAllLinesAsync(_productsFilePath, lines);

            _productsCache.TryAdd(newProduct.Id, newProduct);
            return newProduct;
        }

        public async Task<ProductDto> GetProductByEANAsync(string eanCode)
        {
            var lines = await ReadAllLinesAsync(_productsFilePath);
            var productLine = lines.Skip(1)
                                 .FirstOrDefault(l => l.Split(',')[3] == eanCode); // EANCode yra 4-tas stulpelis

            if (productLine == null)
            {
                return null;
            }

            return ParseProductLine(productLine);
        }

        public async Task<ProductDto> GetProductByQRCodeAsync(string qrCode)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            var qrCodeLine = lines.Skip(1)
                                .FirstOrDefault(l => l.Split(',')[0] == qrCode);

            if (qrCodeLine == null)
            {
                return null;
            }

            var productId = int.Parse(qrCodeLine.Split(',')[1]); // ProductId yra 2-as stulpelis
            return await GetProductByIdAsync(productId);
        }

        public async Task<IEnumerable<ProductDto>> GetAllProductsAsync()
        {
            var lines = await ReadAllLinesAsync(_productsFilePath);
            return lines.Skip(1)
                       .Select(ParseProductLine)
                       .Where(p => p != null);
        }

        public async Task<ProductDto> UpdateProductAsync(UpdateProductDto productDto)
        {
            var lines = await ReadAllLinesAsync(_productsFilePath);
            var productLines = lines.ToList();
            var index = productLines.FindIndex(l => l.StartsWith($"{productDto.Id},"));

            if (index == -1)
            {
                throw new KeyNotFoundException($"Produktas su ID {productDto.Id} nerastas");
            }

            var currentProduct = ParseProductLine(productLines[index]);
            
            // Atnaujiname tik tuos laukus, kurie yra ne null UpdateProductDto objekte
            if (productDto.Name != null) currentProduct.Name = productDto.Name;
            if (productDto.NameEn != null) currentProduct.NameEn = productDto.NameEn;
            if (productDto.Description != null) currentProduct.Description = productDto.Description;
            if (productDto.RetailPrice.HasValue) currentProduct.RetailPrice = productDto.RetailPrice.Value;
            if (productDto.PurchasePrice.HasValue) currentProduct.PurchasePrice = productDto.PurchasePrice.Value;
            if (productDto.WholesalePrice.HasValue) currentProduct.WholesalePrice = productDto.WholesalePrice.Value;
            if (productDto.Status != null) currentProduct.Status = productDto.Status;
            currentProduct.UpdatedAt = DateTime.Now;

            productLines[index] = currentProduct.ToCsvLine();
            await WriteAllLinesAsync(_productsFilePath, productLines);

            // Atnaujiname cache
            _productsCache.AddOrUpdate(currentProduct.Id, currentProduct, (_, __) => currentProduct);
            
            return currentProduct;
        }

        public async Task<IEnumerable<ProductQRCodeDto>> GetProductQRCodesAsync(int productId)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            return lines.Skip(1)
                       .Where(l => l.Split(',')[1] == productId.ToString())
                       .Select(ParseQRCodeLine);
        }

        public async Task<ProductQRCodeDto> AddQRCodeAsync(AddProductQRCodeDto qrCodeDto)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            var newQRCode = new ProductQRCodeDto
            {
                QRCodeId = Guid.NewGuid().ToString(),  // Generuojame unikalų QR kodą
                ProductId = qrCodeDto.ProductId,
                BatchNumber = qrCodeDto.BatchNumber,
                Quantity = qrCodeDto.BatchQuantity,
                QRCodeType = qrCodeDto.QRCodeType,
                ReceivedDate = DateTime.Now,
                ReceivedByUser = qrCodeDto.ReceivedByUser,
                Status = "Active",
                StatusChangedAt = DateTime.Now,
                StatusChangedBy = qrCodeDto.ReceivedByUser
            };

            lines.Add(newQRCode.ToCsvLine());
            await WriteAllLinesAsync(_qrCodesFilePath, lines.ToList());

            return newQRCode;
        }

        public async Task<bool> UpdateQRCodeStatusAsync(UpdateQRCodeStatusDto statusDto)
        {
            var lines = await ReadAllLinesAsync(_qrCodesFilePath);
            var qrCodeLines = lines.ToList();
            var index = qrCodeLines.FindIndex(l => l.StartsWith(statusDto.QRCodeId + ","));

            if (index == -1)
            {
                return false;
            }

            var qrCode = ParseQRCodeLine(qrCodeLines[index]);
            qrCode.Status = statusDto.NewStatus;
            qrCode.StatusChangedAt = statusDto.UpdatedAt;
            qrCode.StatusChangedBy = statusDto.UpdatedByUser;

            qrCodeLines[index] = qrCode.ToCsvLine();
            await WriteAllLinesAsync(_qrCodesFilePath, qrCodeLines);

            return true;
        }

        public async Task<ProductLocationDto> GetProductLocationAsync(int productId, string warehouseId)
        {
            var lines = await ReadAllLinesAsync(_locationsFilePath);
            var locationLine = lines.Skip(1)
                                  .FirstOrDefault(l => l.Contains($",{productId},") && l.StartsWith(warehouseId + ","));

            return locationLine != null ? ParseLocationLine(locationLine) : null;
        }

        public async Task<IEnumerable<ProductMovementDto>> GetProductMovementsAsync(
            int productId, 
            DateTime? startDate = null, 
            DateTime? endDate = null)
        {
            var lines = await ReadAllLinesAsync(_movementsFilePath);
            return lines.Skip(1)
                       .Select(ParseMovementLine)
                       .Where(m => m.ProductId == productId &&
                                 (!startDate.HasValue || m.MovementDate >= startDate) &&
                                 (!endDate.HasValue || m.MovementDate <= endDate))
                       .OrderByDescending(m => m.MovementDate);
        }

        public async Task<IEnumerable<ProductDto>> SearchProductsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<ProductDto>();

            searchTerm = searchTerm.ToLower();
            var products = await GetAllProductsAsync();
            
            return products.Where(p => 
                p.Name.ToLower().Contains(searchTerm) ||
                p.NameEn.ToLower().Contains(searchTerm) ||
                p.EANCode.Contains(searchTerm) ||
                p.Description.ToLower().Contains(searchTerm)
            );
        }

        private ProductDto ParseProductLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new ProductDto
                {
                    Id = int.Parse(parts[0]),
                    Name = parts[1].Trim('"'),
                    NameEn = parts[2].Trim('"'),
                    EANCode = parts[3],
                    Description = parts[8].Trim('"'),
                    WeightNet = decimal.Parse(parts[9]),
                    WeightGross = decimal.Parse(parts[10]),
                    UnitOfMeasure = parts[11],
                    Category = parts[12].Trim('"'),
                    SubCategory = parts[13].Trim('"'),
                    Manufacturer = parts[14].Trim('"'),
                    // ... papildyti likusius laukus
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant produkto CSV eilutę: {Line}", line);
                return null;
            }
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
                    UnitOfMeasure = parts[8].Trim('"')
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

        // CSV eksporto/importo operacijos
        public async Task ExportProductsToCsvAsync(string filePath)
        {
            var products = await GetAllProductsAsync();
            var lines = new List<string> { ProductDto.GetCsvHeader() };
            lines.AddRange(products.Select(p => p.ToCsvLine()));
            await File.WriteAllLinesAsync(filePath, lines);
        }

        public async Task ImportProductsFromCsvAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var header = lines.First(); // Patikriname ar header sutampa
            if (header != ProductDto.GetCsvHeader())
            {
                throw new InvalidOperationException("CSV failo struktūra neatitinka reikalaujamos struktūros");
            }

            // Nuskaitome esamus produktus patikrinimui
            var existingProducts = await GetAllProductsAsync();
            var existingIds = existingProducts.Select(p => p.Id).ToHashSet();

            var newProducts = lines.Skip(1)
                                 .Select(ParseProductLine)
                                 .Where(p => p != null && !existingIds.Contains(p.Id));

            // Įrašome naujus produktus į CSV
            var currentLines = await ReadAllLinesAsync(_productsFilePath);
            currentLines.AddRange(newProducts.Select(p => p.ToCsvLine()));
            await WriteAllLinesAsync(_productsFilePath, currentLines);

            // Atnaujiname cache
            foreach (var product in newProducts)
            {
                _productsCache.TryAdd(product.Id, product);
            }
        }

        // Filtravimo funkcijos
        public async Task<IEnumerable<ProductDto>> GetProductsByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return Enumerable.Empty<ProductDto>();

            var products = await GetAllProductsAsync();
            return products.Where(p => 
                p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)
            );
        }

        public async Task<IEnumerable<ProductDto>> GetDiscontinuedProductsAsync()
        {
            var products = await GetAllProductsAsync();
            return products.Where(p => p.IsDiscontinued);
        }

        public async Task<IEnumerable<ProductDto>> GetLowStockProductsAsync()
        {
            var products = await GetAllProductsAsync();
            return products.Where(p => p.MinimumStock > 0)  // Tikriname tik tuos, kuriems nustatyta minimali riba
                         .Select(async p =>
                         {
                             var locations = await GetProductLocationAsync(p.Id, "MAIN"); // Tikriname pagrindinį sandėlį
                             if (locations != null)
                             {
                                 var totalStock = locations.Quantity;
                                 if (totalStock <= p.MinimumStock)
                                 {
                                     return p;
                                 }
                             }
                             return null;
                         })
                         .WhenAll()
                         .Where(p => p != null);
        }

        // Statistika ir ataskaitos
        public async Task<decimal> GetTotalStockValueAsync()
        {
            var products = await GetAllProductsAsync();
            decimal totalValue = 0;

            foreach (var product in products)
            {
                var locations = await GetProductLocationAsync(product.Id, "MAIN");
                if (locations != null)
                {
                    totalValue += locations.Quantity * product.PurchasePrice;
                }
            }

            return totalValue;
        }

        public async Task<IDictionary<string, int>> GetProductCountByCategoryAsync()
        {
            var products = await GetAllProductsAsync();
            return products.GroupBy(p => p.Category)
                          .ToDictionary(
                              g => g.Key,
                              g => g.Count()
                          );
        }

        public async Task<IEnumerable<ProductDto>> GetTopSellingProductsAsync(int count)
        {
            // Gauname visus produktų judėjimus per paskutinį mėnesį
            var endDate = DateTime.Now;
            var startDate = endDate.AddMonths(-1);
            
            var movements = await ReadAllLinesAsync(_movementsFilePath);
            var productMovements = movements.Skip(1)
                                          .Select(ParseMovementLine)
                                          .Where(m => m != null &&
                                                    m.MovementType == "OUT" &&
                                                    m.MovementDate >= startDate &&
                                                    m.MovementDate <= endDate)
                                          .GroupBy(m => m.ProductId)
                                          .Select(g => new
                                          {
                                              ProductId = g.Key,
                                              TotalQuantity = g.Sum(m => m.Quantity)
                                          })
                                          .OrderByDescending(x => x.TotalQuantity)
                                          .Take(count);

            var products = new List<ProductDto>();
            foreach (var movement in productMovements)
            {
                var product = await GetProductByIdAsync(movement.ProductId);
                if (product != null)
                {
                    products.Add(product);
                }
            }

            return products;
        }

        // Pagalbinė funkcija asinchroniniam IEnumerable apdorojimui
        private static async Task<T[]> WhenAll<T>(this IEnumerable<Task<T>> tasks)
        {
            return await Task.WhenAll(tasks);
        }
        {
            // TODO: Implementuoti CSV eilutės konvertavimą į ProductDto
            throw new NotImplementedException();
        }
    }
}

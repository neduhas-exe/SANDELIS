using System;
using System.Threading.Tasks;
using WarehouseSystem.Services.Interfaces;

namespace WarehouseSystem.Application.Validators
{
    public class ProductBusinessRuleValidator
    {
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;
        private readonly IQRCodeService _qrCodeService;

        public ProductBusinessRuleValidator(
            IProductService productService,
            IWarehouseService warehouseService,
            IQRCodeService qrCodeService)
        {
            _productService = productService;
            _warehouseService = warehouseService;
            _qrCodeService = qrCodeService;
        }

        public async Task<ValidationResult> ValidateProductCreation(int productId, string eanCode)
        {
            var result = new ValidationResult();

            // Patikriname ar produkto ID yra unikalus
            var existingProduct = await _productService.GetProductByIdAsync(productId);
            if (existingProduct != null)
            {
                result.AddError($"Produktas su ID {productId} jau egzistuoja");
            }

            // Patikriname ar EAN kodas yra unikalus
            if (!string.IsNullOrEmpty(eanCode))
            {
                var productWithEan = await _productService.GetProductByEANAsync(eanCode);
                if (productWithEan != null)
                {
                    result.AddError($"Produktas su EAN kodu {eanCode} jau egzistuoja");
                }
            }

            return result;
        }

        public async Task<ValidationResult> ValidateProductUpdate(int productId, decimal? newPrice)
        {
            var result = new ValidationResult();

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                result.AddError($"Produktas su ID {productId} nerastas");
                return result;
            }

            // Tikriname ar nėra aktyvių judėjimų
            var movements = await _warehouseService.GetProductMovementsAsync(productId, DateTime.Now.AddHours(-1));
            if (movements.Any())
            {
                result.AddError("Negalima atnaujinti produkto, kuris turi aktyvių judėjimų per paskutinę valandą");
            }

            // Tikriname kainų logiką
            if (newPrice.HasValue)
            {
                if (newPrice.Value < product.PurchasePrice)
                {
                    result.AddWarning("Nauja kaina yra mažesnė už pirkimo kainą");
                }

                var priceChange = Math.Abs((newPrice.Value - product.RetailPrice) / product.RetailPrice * 100);
                if (priceChange > 20)
                {
                    result.AddWarning($"Kainos pokytis ({priceChange:F1}%) viršija rekomenduojamą 20% ribą");
                }
            }

            return result;
        }

        public async Task<ValidationResult> ValidateProductDeletion(int productId)
        {
            var result = new ValidationResult();

            // Tikriname ar yra likučių
            var locations = await _warehouseService.GetProductLocationsAsync(productId);
            if (locations.Any(l => l.Quantity > 0))
            {
                result.AddError("Negalima ištrinti produkto, kuris turi likučių sandėlyje");
            }

            // Tikriname ar yra aktyvių QR kodų
            var qrCodes = await _qrCodeService.GetProductQRCodesAsync(productId);
            if (qrCodes.Any(q => q.Status == "Active"))
            {
                result.AddError("Negalima ištrinti produkto, kuris turi aktyvių QR kodų");
            }

            // Tikriname ar nėra neužbaigtų judėjimų
            var movements = await _warehouseService.GetProductMovementsAsync(productId, DateTime.Now.AddDays(-1));
            if (movements.Any())
            {
                result.AddError("Negalima ištrinti produkto, kuris turi judėjimų per paskutinę parą");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateProductPriceChange(int productId, decimal newPrice)
        {
            var result = new ValidationResult();
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
            {
                result.AddError($"Produktas su ID {productId} nerastas");
                return result;
            }

            // Minimali marža
            var margin = ((newPrice - product.PurchasePrice) / product.PurchasePrice) * 100;
            if (margin < 10)
            {
                result.AddWarning($"Marža ({margin:F1}%) yra mažesnė už rekomenduojamą 10% ribą");
            }

            // Maksimalus kainos pokytis per dieną
            var priceHistory = await _productService.GetPriceHistoryAsync(productId, DateTime.Now.AddDays(-1));
            if (priceHistory.Any())
            {
                var maxDailyChange = 30m; // 30%
                var lastPrice = priceHistory.OrderByDescending(p => p.ChangeDate).First().Price;
                var priceChange = Math.Abs((newPrice - lastPrice) / lastPrice * 100);

                if (priceChange > maxDailyChange)
                {
                    result.AddError($"Kainos pokytis ({priceChange:F1}%) viršija leistiną {maxDailyChange}% ribą per dieną");
                }
            }

            return result;
        }

        public async Task<ValidationResult> ValidateMinimumStockLevel(int productId, decimal minimumStock)
        {
            var result = new ValidationResult();

            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                result.AddError($"Produktas su ID {productId} nerastas");
                return result;
            }

            // Tikriname ar minimalus kiekis nėra per didelis
            var averageMonthlyUsage = await GetAverageMonthlyUsage(productId);
            var recommendedMinStock = Math.Ceiling(averageMonthlyUsage * 0.5m); // 2 savaičių atsarga

            if (minimumStock > recommendedMinStock * 2)
            {
                result.AddWarning($"Nurodytas minimalus kiekis ({minimumStock}) yra žymiai didesnis už rekomenduojamą ({recommendedMinStock})");
            }

            // Tikriname ar minimalus kiekis nėra per mažas greitai judantiems produktams
            if (averageMonthlyUsage > 100 && minimumStock < recommendedMinStock * 0.5m)
            {
                result.AddWarning($"Nurodytas minimalus kiekis ({minimumStock}) gali būti per mažas, atsižvelgiant į produkto apyvartą");
            }

            return result;
        }

        private async Task<decimal> GetAverageMonthlyUsage(int productId)
        {
            var movements = await _warehouseService.GetProductMovementsAsync(
                productId,
                DateTime.Now.AddMonths(-3),
                DateTime.Now
            );

            var outgoingMovements = movements.Where(m => m.MovementType == "OUT");
            if (!outgoingMovements.Any())
                return 0;

            return outgoingMovements.Sum(m => m.Quantity) / 3; // 3 mėnesių vidurkis
        }
    }
}

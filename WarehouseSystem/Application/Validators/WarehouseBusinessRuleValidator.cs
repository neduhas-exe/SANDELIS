using System;
using System.Threading.Tasks;
using System.Linq;
using WarehouseSystem.Services.Interfaces;

namespace WarehouseSystem.Application.Validators
{
    public class WarehouseBusinessRuleValidator
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly IQRCodeService _qrCodeService;

        public WarehouseBusinessRuleValidator(
            IWarehouseService warehouseService,
            IProductService productService,
            IQRCodeService qrCodeService)
        {
            _warehouseService = warehouseService;
            _productService = productService;
            _qrCodeService = qrCodeService;
        }

        public async Task<ValidationResult> ValidateProductReceive(
            int productId, 
            string warehouseId, 
            string locationCode, 
            decimal quantity,
            string qrCodeId = null)
        {
            var result = new ValidationResult();

            // Tikriname ar produktas egzistuoja ir yra aktyvus
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                result.AddError($"Produktas su ID {productId} nerastas");
                return result;
            }

            if (!product.IsActive)
            {
                result.AddError("Negalima priimti neaktyvaus produkto");
            }

            // Tikriname lokaciją
            var location = await _warehouseService.GetLocationAsync(warehouseId, locationCode);
            if (location == null)
            {
                result.AddError($"Lokacija {locationCode} nerasta sandėlyje {warehouseId}");
                return result;
            }

            // Tikriname ar lokacija nėra karantino zonoje
            if (location.IsQuarantine)
            {
                result.AddWarning("Priėmimas į karantino zoną. Būtina papildoma kokybės kontrolė.");
            }

            // Tikriname ar užteks vietos
            var availableCapacity = location.MaxCapacity - location.Quantity;
            if (quantity > availableCapacity)
            {
                result.AddError($"Nepakanka vietos lokacijoje. Laisva talpa: {availableCapacity} {location.UnitOfMeasure}");
            }

            // Tikriname QR kodą jei pateiktas
            if (!string.IsNullOrEmpty(qrCodeId))
            {
                var qrCode = await _qrCodeService.GetQRCodeInfoAsync(qrCodeId);
                if (qrCode == null)
                {
                    result.AddError($"QR kodas {qrCodeId} nerastas");
                }
                else
                {
                    if (qrCode.ProductId != productId)
                    {
                        result.AddError("QR kodas priklauso kitam produktui");
                    }
                    if (qrCode.Status != "Active")
                    {
                        result.AddError($"QR kodas yra {qrCode.Status} būsenoje");
                    }
                }
            }

            return result;
        }

        public async Task<ValidationResult> ValidateProductTransfer(
            int productId,
            string sourceWarehouseId,
            string sourceLocationCode,
            string destinationWarehouseId,
            string destinationLocationCode,
            decimal quantity)
        {
            var result = new ValidationResult();

            // Tikriname išsiuntimo lokaciją
            var sourceLocation = await _warehouseService.GetLocationAsync(sourceWarehouseId, sourceLocationCode);
            if (sourceLocation == null)
            {
                result.AddError($"Išsiuntimo lokacija {sourceLocationCode} nerasta");
                return result;
            }

            // Tikriname ar užtenka kiekio išsiuntimo lokacijoje
            if (sourceLocation.Quantity < quantity)
            {
                result.AddError($"Nepakankamas kiekis išsiuntimo lokacijoje. Turimas kiekis: {sourceLocation.Quantity} {sourceLocation.UnitOfMeasure}");
            }

            // Tikriname gavimo lokaciją
            var destinationLocation = await _warehouseService.GetLocationAsync(destinationWarehouseId, destinationLocationCode);
            if (destinationLocation == null)
            {
                result.AddError($"Gavimo lokacija {destinationLocationCode} nerasta");
                return result;
            }

            // Tikriname ar užteks vietos gavimo lokacijoje
            var availableCapacity = destinationLocation.MaxCapacity - destinationLocation.Quantity;
            if (quantity > availableCapacity)
            {
                result.AddError($"Nepakanka vietos gavimo lokacijoje. Laisva talpa: {availableCapacity} {destinationLocation.UnitOfMeasure}");
            }

            // Tikriname ar matavimo vienetai sutampa
            if (sourceLocation.UnitOfMeasure != destinationLocation.UnitOfMeasure)
            {
                result.AddError("Skirtingi matavimo vienetai išsiuntimo ir gavimo lokacijose");
            }

            // Tikriname karantino zonas
            if (!sourceLocation.IsQuarantine && destinationLocation.IsQuarantine)
            {
                result.AddWarning("Perkėlimas į karantino zoną. Reikalingas kokybės skyriaus patvirtinimas.");
            }

            // Tikriname saugojimo sąlygas
            if (!string.IsNullOrEmpty(sourceLocation.StorageConditions) && 
                !string.IsNullOrEmpty(destinationLocation.StorageConditions) &&
                sourceLocation.StorageConditions != destinationLocation.StorageConditions)
            {
                result.AddWarning("Skirtingos saugojimo sąlygos išsiuntimo ir gavimo lokacijose");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateProductShipment(
            int productId,
            string warehouseId,
            string locationCode,
            decimal quantity,
            string referenceNumber)
        {
            var result = new ValidationResult();

            // Tikriname lokaciją
            var location = await _warehouseService.GetLocationAsync(warehouseId, locationCode);
            if (location == null)
            {
                result.AddError($"Lokacija {locationCode} nerasta");
                return result;
            }

            // Tikriname ar užtenka kiekio
            if (location.Quantity < quantity)
            {
                result.AddError($"Nepakankamas kiekis. Turimas kiekis: {location.Quantity} {location.UnitOfMeasure}");
            }

            // Tikriname ar lokacija nėra karantino zonoje
            if (location.IsQuarantine)
            {
                result.AddError("Negalima išsiųsti produktų iš karantino zonos");
            }

            // FIFO tikrinimas
            var oldestStock = await GetOldestStockLocation(productId, warehouseId);
            if (oldestStock != null && oldestStock.LocationCode != locationCode)
            {
                result.AddWarning($"Nesilaikoma FIFO principo. Seniausias likutis yra lokacijoje {oldestStock.LocationCode}");
            }

            // Tikriname ar lieka pakankamai minimaliam likučiui
            var product = await _productService.GetProductByIdAsync(productId);
            var totalStock = await _warehouseService.GetTotalStockQuantityAsync(productId);
            if (totalStock - quantity < product.MinimumStock)
            {
                result.AddWarning("Po išsiuntimo bendras likutis bus mažesnis už minimalų");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateStockCount(
            int productId,
            string warehouseId,
            string locationCode,
            decimal countedQuantity)
        {
            var result = new ValidationResult();

            // Tikriname ar lokacija egzistuoja
            var location = await _warehouseService.GetLocationAsync(warehouseId, locationCode);
            if (location == null)
            {
                result.AddError($"Lokacija {locationCode} nerasta");
                return result;
            }

            // Tikriname ar yra didelis skirtumas nuo sistemos duomenų
            var difference = Math.Abs(countedQuantity - location.Quantity);
            var differencePercentage = (difference / location.Quantity) * 100;
            
            if (differencePercentage > 10)
            {
                result.AddWarning($"Didelis kiekio skirtumas ({differencePercentage:F1}%). Reikalingas pakartotinis skaičiavimas.");
            }

            // Tikriname ar nėra aktyvių operacijų
            var movements = await _warehouseService.GetProductMovementsAsync(
                productId,
                DateTime.Now.AddHours(-1));

            if (movements.Any())
            {
                result.AddWarning("Rasti aktyvūs judėjimai per paskutinę valandą. Rekomenduojama palaukti kol operacijos bus užbaigtos.");
            }

            return result;
        }

        private async Task<ProductLocationDto> GetOldestStockLocation(int productId, string warehouseId)
        {
            var locations = await _warehouseService.GetProductLocationsAsync(productId);
            return locations
                .Where(l => l.WarehouseId == warehouseId && l.Quantity > 0 && !l.IsQuarantine)
                .OrderBy(l => l.LastUpdated)
                .FirstOrDefault();
        }
    }
}

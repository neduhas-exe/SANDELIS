using System;
using System.Threading.Tasks;
using System.Linq;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Application.Validators
{
    public class MovementBusinessRuleValidator
    {
        private readonly IMovementService _movementService;
        private readonly IWarehouseService _warehouseService;
        private readonly IProductService _productService;
        private readonly IQRCodeService _qrCodeService;

        public MovementBusinessRuleValidator(
            IMovementService movementService,
            IWarehouseService warehouseService,
            IProductService productService,
            IQRCodeService qrCodeService)
        {
            _movementService = movementService;
            _warehouseService = warehouseService;
            _productService = productService;
            _qrCodeService = qrCodeService;
        }

        public async Task<ValidationResult> ValidateMovementRegistration(ProductMovementRegistrationDto movement)
        {
            var result = new ValidationResult();

            // Tikriname ar produktas egzistuoja ir yra aktyvus
            var product = await _productService.GetProductByIdAsync(movement.ProductId);
            if (product == null)
            {
                result.AddError($"Produktas su ID {movement.ProductId} nerastas");
                return result;
            }

            if (!product.IsActive)
            {
                result.AddError("Negalima atlikti operacijų su neaktyviu produktu");
            }

            // Tikriname lokacijų egzistavimą priklausomai nuo judėjimo tipo
            switch (movement.MovementType.ToUpper())
            {
                case "IN":
                    if (string.IsNullOrEmpty(movement.DestinationLocation))
                    {
                        result.AddError("Nenurodyta gavimo lokacija");
                    }
                    break;

                case "OUT":
                    if (string.IsNullOrEmpty(movement.SourceLocation))
                    {
                        result.AddError("Nenurodyta išsiuntimo lokacija");
                    }
                    break;

                case "TRANSFER":
                    if (string.IsNullOrEmpty(movement.SourceLocation) || 
                        string.IsNullOrEmpty(movement.DestinationLocation))
                    {
                        result.AddError("Būtina nurodyti abi lokacijas perkėlimui");
                    }
                    if (movement.SourceLocation == movement.DestinationLocation)
                    {
                        result.AddError("Išsiuntimo ir gavimo lokacijos negali būti tos pačios");
                    }
                    break;

                default:
                    result.AddError($"Neteisingas judėjimo tipas: {movement.MovementType}");
                    break;
            }

            // QR kodo validacija
            if (!string.IsNullOrEmpty(movement.QRCodeId))
            {
                var qrValidation = await ValidateQRCodeForMovement(
                    movement.QRCodeId, 
                    movement.ProductId, 
                    movement.MovementType);
                result.MergeWith(qrValidation);
            }

            // Jei yra klaidos, nereikia toliau tikrinti
            if (!result.IsValid) return result;

            // Tikriname ar pakanka kiekio išsiuntimo lokacijoje
            if (movement.MovementType != "IN")
            {
                var sourceLocation = await _warehouseService.GetLocationAsync(
                    ExtractWarehouseId(movement.SourceLocation), 
                    movement.SourceLocation);

                if (sourceLocation.Quantity < movement.Quantity)
                {
                    result.AddError($"Nepakankamas kiekis išsiuntimo lokacijoje. Turimas: {sourceLocation.Quantity}");
                }
            }

            // Tikriname ar yra vietos gavimo lokacijoje
            if (movement.MovementType != "OUT")
            {
                var destLocation = await _warehouseService.GetLocationAsync(
                    ExtractWarehouseId(movement.DestinationLocation), 
                    movement.DestinationLocation);

                var availableCapacity = destLocation.MaxCapacity - destLocation.Quantity;
                if (movement.Quantity > availableCapacity)
                {
                    result.AddError($"Nepakanka vietos gavimo lokacijoje. Laisva: {availableCapacity}");
                }
            }

            // Tikriname ar nėra per daug judėjimų per trumpą laiką
            var recentMovements = await _movementService.GetMovementsAsync(DateTime.Now.AddMinutes(-5));
            var recentCount = recentMovements.Count(m => m.ProductId == movement.ProductId);
            if (recentCount > 10)
            {
                result.AddWarning("Pastebėtas didelis judėjimų skaičius per trumpą laiką");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateMovementCancellation(int movementId, MovementCancellationDto cancellation)
        {
            var result = new ValidationResult();

            // Tikriname ar judėjimas egzistuoja
            var movement = await _movementService.GetMovementByIdAsync(movementId);
            if (movement == null)
            {
                result.AddError($"Judėjimas su ID {movementId} nerastas");
                return result;
            }

            // Tikriname ar praėjo leistinas laikas atšaukimui (pvz., 1 valanda)
            if (movement.MovementDate < DateTime.Now.AddHours(-1))
            {
                result.AddError("Negalima atšaukti judėjimo, kuris įvyko prieš daugiau nei valandą");
            }

            // Tikriname ar nėra vėlesnių susijusių judėjimų
            var laterMovements = await _movementService.GetProductMovementsAsync(
                movement.ProductId,
                movement.MovementDate,
                DateTime.Now);

            if (laterMovements.Any(m => m.MovementId != movementId))
            {
                result.AddError("Negalima atšaukti judėjimo, nes yra vėlesnių operacijų");
            }

            // Tikriname priežastį
            if (string.IsNullOrWhiteSpace(cancellation.Reason))
            {
                result.AddError("Būtina nurodyti atšaukimo priežastį");
            }
            else if (cancellation.Reason.Length < 10)
            {
                result.AddError("Atšaukimo priežastis turi būti bent 10 simbolių ilgio");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateMovementAdjustment(
            int movementId, 
            MovementQuantityAdjustmentDto adjustment)
        {
            var result = new ValidationResult();

            // Tikriname ar judėjimas egzistuoja
            var movement = await _movementService.GetMovementByIdAsync(movementId);
            if (movement == null)
            {
                result.AddError($"Judėjimas su ID {movementId} nerastas");
                return result;
            }

            // Tikriname ar praėjo leistinas laikas koregavimui (pvz., 24 valandos)
            if (movement.MovementDate < DateTime.Now.AddHours(-24))
            {
                result.AddError("Negalima koreguoti judėjimo, kuris įvyko prieš daugiau nei 24 valandas");
            }

            // Tikriname kiekio pokytį
            var quantityChange = Math.Abs(adjustment.NewQuantity - movement.Quantity);
            var changePercentage = (quantityChange / movement.Quantity) * 100;

            if (changePercentage > 20)
            {
                result.AddWarning($"Didelis kiekio pokytis ({changePercentage:F1}%). Reikalingas papildomas patvirtinimas.");
            }

            // Tikriname priežastį
            if (string.IsNullOrWhiteSpace(adjustment.Reason))
            {
                result.AddError("Būtina nurodyti koregavimo priežastį");
            }
            else if (adjustment.Reason.Length < 10)
            {
                result.AddError("Koregavimo priežastis turi būti bent 10 simbolių ilgio");
            }

            // Tikriname ar pakanka kiekio/vietos po koregavimo
            if (movement.MovementType != "IN")
            {
                var sourceLocation = await _warehouseService.GetLocationAsync(
                    ExtractWarehouseId(movement.SourceLocation), 
                    movement.SourceLocation);

                var additionalQuantity = adjustment.NewQuantity - movement.Quantity;
                if (additionalQuantity > 0 && sourceLocation.Quantity < additionalQuantity)
                {
                    result.AddError($"Nepakankamas kiekis išsiuntimo lokacijoje papildomam kiekiui");
                }
            }

            if (movement.MovementType != "OUT")
            {
                var destLocation = await _warehouseService.GetLocationAsync(
                    ExtractWarehouseId(movement.DestinationLocation), 
                    movement.DestinationLocation);

                var additionalQuantity = adjustment.NewQuantity - movement.Quantity;
                var availableCapacity = destLocation.MaxCapacity - destLocation.Quantity;

                if (additionalQuantity > 0 && additionalQuantity > availableCapacity)
                {
                    result.AddError($"Nepakanka vietos gavimo lokacijoje papildomam kiekiui");
                }
            }

            return result;
        }

        private async Task<ValidationResult> ValidateQRCodeForMovement(
            string qrCodeId, 
            int productId, 
            string movementType)
        {
            var result = new ValidationResult();

            var qrCode = await _qrCodeService.GetQRCodeInfoAsync(qrCodeId);
            if (qrCode == null)
            {
                result.AddError($"QR kodas {qrCodeId} nerastas");
                return result;
            }

            if (qrCode.ProductId != productId)
            {
                result.AddError("QR kodas priklauso kitam produktui");
            }

            if (qrCode.Status != "Active")
            {
                result.AddError($"QR kodas yra {qrCode.Status} būsenoje");
            }

            if (movementType == "OUT" && qrCode.QRCodeType == "Batch")
            {
                var batchMovements = await _movementService.GetQRCodeMovementsAsync(qrCodeId);
                var totalOut = batchMovements
                    .Where(m => m.MovementType == "OUT")
                    .Sum(m => m.Quantity);

                if (totalOut >= qrCode.Quantity)
                {
                    result.AddError("Šios partijos QR kodo kiekis jau išnaudotas");
                }
            }

            return result;
        }

        private string ExtractWarehouseId(string locationCode)
        {
            // Pavyzdinis metodas - reikia pritaikyti pagal realų lokacijos kodo formatą
            return locationCode.Split('-')[0];
        }
    }
}

using System;
using System.Threading.Tasks;
using System.Linq;
using WarehouseSystem.Services.Interfaces;

namespace WarehouseSystem.Application.Validation.BusinessRules
{
    public class QRCodeBusinessRuleValidator
    {
        private readonly IQRCodeService _qrCodeService;
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;

        public QRCodeBusinessRuleValidator(
            IQRCodeService qrCodeService,
            IProductService productService,
            IWarehouseService warehouseService)
        {
            _qrCodeService = qrCodeService;
            _productService = productService;
            _warehouseService = warehouseService;
        }

        public async Task<ValidationResult> ValidateQRCodeGeneration(int productId, string batchNumber, int quantity)
        {
            var result = new ValidationResult();

            // Tikriname ar produktas egzistuoja
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                result.AddError($"Produktas su ID {productId} nerastas");
                return result;
            }

            // Tikriname ar produktas aktyvus
            if (!product.IsActive)
            {
                result.AddError("Negalima generuoti QR kodų neaktyviam produktui");
            }

            // Tikriname partijos numerio unikalumą
            if (!string.IsNullOrEmpty(batchNumber))
            {
                var existingBatchQRs = await _qrCodeService.GetBatchQRCodesAsync(batchNumber);
                if (existingBatchQRs.Any())
                {
                    result.AddError($"Partijos numeris {batchNumber} jau naudojamas");
                }
            }

            // Tikriname generuojamų QR kodų kiekį
            if (quantity > 1000)
            {
                result.AddError("Negalima generuoti daugiau nei 1000 QR kodų vienu metu");
            }

            // Tikriname ar neviršijame limito
            var activeQRCount = await _qrCodeService.GetActiveQRCodesCountAsync(productId);
            if (activeQRCount + quantity > 10000)
            {
                result.AddError("Viršytas maksimalus aktyvių QR kodų skaičius produktui (10000)");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateQRCodeStatusChange(string qrCodeId, string newStatus, string reason)
        {
            var result = new ValidationResult();

            var qrCode = await _qrCodeService.GetQRCodeInfoAsync(qrCodeId);
            if (qrCode == null)
            {
                result.AddError($"QR kodas {qrCodeId} nerastas");
                return result;
            }

            // Tikriname ar galima keisti statusą
            if (qrCode.Status == "Used" && newStatus != "Damaged")
            {
                result.AddError("Panaudoto QR kodo statusą galima keisti tik į 'Damaged'");
            }

            if (qrCode.Status == "Damaged" || qrCode.Status == "Lost")
            {
                result.AddError("Negalima keisti sugadinto arba prarasto QR kodo statuso");
            }

            // Tikriname ar nėra aktyvių judėjimų
            var movements = await _warehouseService.GetProductMovementsAsync(
                qrCode.ProductId,
                DateTime.Now.AddHours(-1));

            if (movements.Any(m => m.QRCodeId == qrCodeId))
            {
                result.AddError("Negalima keisti QR kodo statuso, kai yra aktyvių judėjimų per paskutinę valandą");
            }

            // Validuojame priežastį
            if (string.IsNullOrWhiteSpace(reason))
            {
                result.AddError("Būtina nurodyti statuso keitimo priežastį");
            }
            else if (reason.Length < 10)
            {
                result.AddError("Statuso keitimo priežastis turi būti bent 10 simbolių ilgio");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateQRCodeLink(string qrCodeId, int productId)
        {
            var result = new ValidationResult();

            // Tikriname ar QR kodas egzistuoja
            var qrCode = await _qrCodeService.GetQRCodeInfoAsync(qrCodeId);
            if (qrCode == null)
            {
                result.AddError($"QR kodas {qrCodeId} nerastas");
                return result;
            }

            // Tikriname ar produktas egzistuoja ir yra aktyvus
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                result.AddError($"Produktas su ID {productId} nerastas");
                return result;
            }

            if (!product.IsActive)
            {
                result.AddError("Negalima susieti QR kodo su neaktyviu produktu");
            }

            // Tikriname ar QR kodas jau nėra susietas
            if (qrCode.ProductId != 0 && qrCode.ProductId != productId)
            {
                result.AddError("QR kodas jau susietas su kitu produktu");
            }

            // Tikriname ar QR kodas nėra panaudotas/sugadintas
            if (qrCode.Status != "Active")
            {
                result.AddError($"Negalima susieti QR kodo, kurio statusas yra '{qrCode.Status}'");
            }

            return result;
        }

        public async Task<ValidationResult> ValidateProductBatchQRGeneration(int productId, string batchNumber, decimal quantity)
        {
            var result = new ValidationResult();

            // Tikriname ar produktas palaiko partijos QR kodus
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                result.AddError($"Produktas su ID {productId} nerastas");
                return result;
            }

            if (!product.HasBatchQR)
            {
                result.AddError("Produktui nėra įjungta partijos QR kodų funkcija");
            }

            // Tikriname partijos numerio formatą
            if (!IsValidBatchNumber(batchNumber))
            {
                result.AddError("Neteisingas partijos numerio formatas. Naudokite formatą: YYYYMMDD-XXX");
            }

            // Tikriname kiekį
            var minBatchQuantity = product.IsBatchProduct ? 1 : 100;
            if (quantity < minBatchQuantity)
            {
                result.AddError($"Minimalus partijos kiekis: {minBatchQuantity}");
            }

            var maxBatchQuantity = product.IsBatchProduct ? 10000 : 1000;
            if (quantity > maxBatchQuantity)
            {
                result.AddError($"Maksimalus partijos kiekis: {maxBatchQuantity}");
            }

            return result;
        }

        private bool IsValidBatchNumber(string batchNumber)
        {
            if (string.IsNullOrEmpty(batchNumber)) return false;

            // Formatas: YYYYMMDD-XXX
            if (!batchNumber.Contains("-")) return false;

            var parts = batchNumber.Split('-');
            if (parts.Length != 2) return false;

            // Tikriname datą
            if (parts[0].Length != 8) return false;
            if (!DateTime.TryParseExact(parts[0], "yyyyMMdd", null, 
                System.Globalization.DateTimeStyles.None, out _))
            {
                return false;
            }

            // Tikriname sekos numerį
            if (parts[1].Length != 3) return false;
            if (!int.TryParse(parts[1], out _)) return false;

            return true;
        }
    }
}

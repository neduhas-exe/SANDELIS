using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for AddProductQRCodeDto
    /// </summary>
    public class AddProductQRCodeDtoValidator : AbstractValidator<AddProductQRCodeDto>
    {
        public AddProductQRCodeDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Partijos numeris yra privalomas")
                .MaximumLength(50).WithMessage("Partijos numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => x.QRCodeType == "Batch");

            RuleFor(x => x.BatchQuantity)
                .GreaterThan(0).WithMessage("Partijos kiekis turi būti didesnis už 0")
                .When(x => x.QRCodeType == "Batch");

            RuleFor(x => x.QRCodeType)
                .NotEmpty().WithMessage("QR kodo tipas yra privalomas")
                .Must(BeValidQRCodeType).WithMessage("Neteisingas QR kodo tipas. Galimi variantai: Batch, Individual");

            RuleFor(x => x.NumberOfQRCodes)
                .GreaterThan(0).WithMessage("QR kodų kiekis turi būti didesnis už 0")
                .LessThanOrEqualTo(1000).WithMessage("Negalima generuoti daugiau nei 1000 QR kodų vienu metu");

            RuleFor(x => x.PurchaseInvoice)
                .MaximumLength(50).WithMessage("Sąskaitos numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.PurchaseInvoice));

            RuleFor(x => x.SupplierName)
                .MaximumLength(200).WithMessage("Tiekėjo pavadinimas negali būti ilgesnis nei 200 simbolių")
                .When(x => !string.IsNullOrEmpty(x.SupplierName));

            RuleFor(x => x.PurchasePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Pirkimo kaina negali būti neigiama");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Valiuta yra privaloma")
                .Must(BeValidCurrency).WithMessage("Neteisinga valiuta. Galimi variantai: EUR, USD");

            RuleFor(x => x.VATRate)
                .InclusiveBetween(0, 100).WithMessage("PVM tarifas turi būti tarp 0 ir 100");

            RuleFor(x => x.Location)
                .MaximumLength(50).WithMessage("Vietos kodas negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Location));

            RuleFor(x => x.ReceivedByUser)
                .NotEmpty().WithMessage("Priėmusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Pastabos negali būti ilgesnės nei 1000 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            RuleFor(x => x.ReceivedDate)
                .NotEmpty().WithMessage("Priėmimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Priėmimo data negali būti ateityje");
        }

        private bool BeValidQRCodeType(string qrCodeType)
        {
            var validTypes = new[] { "Batch", "Individual" };
            return validTypes.Contains(qrCodeType);
        }

        private bool BeValidCurrency(string currency)
        {
            var validCurrencies = new[] { "EUR", "USD" };
            return validCurrencies.Contains(currency?.ToUpper());
        }
    }
}

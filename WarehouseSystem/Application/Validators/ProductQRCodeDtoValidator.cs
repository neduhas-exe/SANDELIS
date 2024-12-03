using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for ProductQRCodeDto
    /// </summary>
    public class ProductQRCodeDtoValidator : AbstractValidator<ProductQRCodeDto>
    {
        public ProductQRCodeDtoValidator()
        {
            RuleFor(x => x.QRCodeId)
                .NotEmpty().WithMessage("QR kodo ID yra privalomas")
                .MaximumLength(50).WithMessage("QR kodo ID negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.BatchNumber)
                .MaximumLength(50).WithMessage("Partijos numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.BatchNumber));

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Kiekis negali būti neigiamas");

            RuleFor(x => x.QRCodeType)
                .NotEmpty().WithMessage("QR kodo tipas yra privalomas")
                .Must(BeValidQRCodeType).WithMessage("Neteisingas QR kodo tipas. Galimi variantai: Batch, Individual");

            RuleFor(x => x.ReceivedDate)
                .NotEmpty().WithMessage("Gavimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Gavimo data negali būti ateityje");

            RuleFor(x => x.ReceivedByUser)
                .NotEmpty().WithMessage("Priėmusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.WarehouseLocation)
                .MaximumLength(50).WithMessage("Sandėlio vietos kodas negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.WarehouseLocation));

            RuleFor(x => x.PurchaseInvoice)
                .MaximumLength(50).WithMessage("Pirkimo sąskaitos numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.PurchaseInvoice));

            RuleFor(x => x.SupplierName)
                .MaximumLength(200).WithMessage("Tiekėjo pavadinimas negali būti ilgesnis nei 200 simbolių")
                .When(x => !string.IsNullOrEmpty(x.SupplierName));

            RuleFor(x => x.SupplierInvoiceNumber)
                .MaximumLength(50).WithMessage("Tiekėjo sąskaitos numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.SupplierInvoiceNumber));

            RuleFor(x => x.SupplierOrderNumber)
                .MaximumLength(50).WithMessage("Tiekėjo užsakymo numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.SupplierOrderNumber));

            RuleFor(x => x.BatchPurchasePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Partijos pirkimo kaina negali būti neigiama");

            RuleFor(x => x.BatchPurchasePriceVAT)
                .GreaterThanOrEqualTo(0).WithMessage("Partijos pirkimo kaina su PVM negali būti neigiama")
                .GreaterThanOrEqualTo(x => x.BatchPurchasePrice)
                    .WithMessage("Partijos pirkimo kaina su PVM negali būti mažesnė už kainą be PVM");

            RuleFor(x => x.VATRate)
                .InclusiveBetween(0, 100).WithMessage("PVM tarifas turi būti tarp 0 ir 100");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Valiuta yra privaloma")
                .Must(BeValidCurrency).WithMessage("Neteisinga valiuta. Galimi variantai: EUR, USD");

            RuleFor(x => x.PurchaseDate)
                .NotEmpty().WithMessage("Pirkimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Pirkimo data negali būti ateityje");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Būsena yra privaloma")
                .Must(BeValidStatus).WithMessage("Neteisinga būsena. Galimos reikšmės: Active, Used, Damaged, Lost");

            RuleFor(x => x.StatusChangedAt)
                .NotEmpty().WithMessage("Būsenos pakeitimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Būsenos pakeitimo data negali būti ateityje");

            RuleFor(x => x.StatusChangedBy)
                .NotEmpty().WithMessage("Būseną pakeitusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");
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

        private bool BeValidStatus(string status)
        {
            var validStatuses = new[] { "Active", "Used", "Damaged", "Lost" };
            return validStatuses.Contains(status);
        }
    }
}

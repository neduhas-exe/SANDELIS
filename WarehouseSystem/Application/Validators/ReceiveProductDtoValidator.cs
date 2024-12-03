using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for ReceiveProductDto
    /// </summary>
    public class ReceiveProductDtoValidator : AbstractValidator<ReceiveProductDto>
    {
        public ReceiveProductDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.LocationCode)
                .NotEmpty().WithMessage("Vietos kodas yra privalomas")
                .MaximumLength(50).WithMessage("Vietos kodas negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Kiekis turi būti didesnis už 0");

            RuleFor(x => x.QRCodeId)
                .MaximumLength(50).WithMessage("QR kodo ID negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.QRCodeId));

            RuleFor(x => x.ReceivedByUser)
                .NotEmpty().WithMessage("Priėmusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.PurchaseInvoice)
                .MaximumLength(50).WithMessage("Sąskaitos numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.PurchaseInvoice));

            RuleFor(x => x.SupplierName)
                .MaximumLength(200).WithMessage("Tiekėjo pavadinimas negali būti ilgesnis nei 200 simbolių")
                .When(x => !string.IsNullOrEmpty(x.SupplierName));

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Pastabos negali būti ilgesnės nei 1000 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}

using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for ShipProductDto
    /// </summary>
    public class ShipProductDtoValidator : AbstractValidator<ShipProductDto>
    {
        public ShipProductDtoValidator()
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

            RuleFor(x => x.ReferenceNumber)
                .NotEmpty().WithMessage("Siuntimo numeris yra privalomas")
                .MaximumLength(50).WithMessage("Siuntimo numeris negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.ShippedByUser)
                .NotEmpty().WithMessage("Išsiuntusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.ShipmentType)
                .NotEmpty().WithMessage("Siuntimo tipas yra privalomas")
                .Must(BeValidShipmentType).WithMessage("Neteisingas siuntimo tipas. Galimi variantai: Sales, Return, Transfer");

            RuleFor(x => x.CustomerInfo)
                .MaximumLength(500).WithMessage("Kliento informacija negali būti ilgesnė nei 500 simbolių")
                .When(x => !string.IsNullOrEmpty(x.CustomerInfo));

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Pastabos negali būti ilgesnės nei 1000 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }

        private bool BeValidShipmentType(string shipmentType)
        {
            var validTypes = new[] { "Sales", "Return", "Transfer" };
            return validTypes.Contains(shipmentType);
        }
    }
}

using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for ProductMovementDto
    /// </summary>
    public class ProductMovementDtoValidator : AbstractValidator<ProductMovementDto>
    {
        public ProductMovementDtoValidator()
        {
            RuleFor(x => x.MovementId)
                .GreaterThan(0).WithMessage("Judėjimo ID privalo būti teigiamas skaičius");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.QRCodeId)
                .MaximumLength(50).WithMessage("QR kodo ID negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.QRCodeId));

            RuleFor(x => x.MovementType)
                .NotEmpty().WithMessage("Judėjimo tipas yra privalomas")
                .Must(BeValidMovementType).WithMessage("Neteisingas judėjimo tipas. Galimi variantai: IN, OUT, TRANSFER");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Kiekis turi būti didesnis už 0");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Matavimo vienetas yra privalomas")
                .Must(BeValidUnitOfMeasure).WithMessage("Neteisingas matavimo vienetas. Galimi variantai: VNT, KG, M, L");

            RuleFor(x => x.SourceLocation)
                .NotEmpty().WithMessage("Pradinė vieta yra privaloma")
                .MaximumLength(50).WithMessage("Pradinės vietos kodas negali būti ilgesnis nei 50 simbolių")
                .When(x => x.MovementType != "IN");

            RuleFor(x => x.DestinationLocation)
                .NotEmpty().WithMessage("Galinė vieta yra privaloma")
                .MaximumLength(50).WithMessage("Galinės vietos kodas negali būti ilgesnis nei 50 simbolių")
                .When(x => x.MovementType != "OUT");

            RuleFor(x => x.MovementDate)
                .NotEmpty().WithMessage("Judėjimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Judėjimo data negali būti ateityje");

            RuleFor(x => x.MovedByUser)
                .NotEmpty().WithMessage("Perkėlusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(50).WithMessage("Susijusios operacijos numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Pastabos negali būti ilgesnės nei 1000 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Notes));

            // Custom validations for movement types
            When(x => x.MovementType == "IN", () =>
            {
                RuleFor(x => x.SourceLocation)
                    .Empty().WithMessage("Pradinė vieta neturi būti nurodyta judėjimo tipui IN");
            });

            When(x => x.MovementType == "OUT", () =>
            {
                RuleFor(x => x.DestinationLocation)
                    .Empty().WithMessage("Galinė vieta neturi būti nurodyta judėjimo tipui OUT");
            });

            When(x => x.MovementType == "TRANSFER", () =>
            {
                RuleFor(x => x.SourceLocation)
                    .NotEmpty().WithMessage("Pradinė vieta yra privaloma judėjimo tipui TRANSFER");
                RuleFor(x => x.DestinationLocation)
                    .NotEmpty().WithMessage("Galinė vieta yra privaloma judėjimo tipui TRANSFER");
            });
        }

        private bool BeValidMovementType(string movementType)
        {
            var validTypes = new[] { "IN", "OUT", "TRANSFER" };
            return validTypes.Contains(movementType?.ToUpper());
        }

        private bool BeValidUnitOfMeasure(string unitOfMeasure)
        {
            var validUnits = new[] { "VNT", "KG", "M", "L" };
            return validUnits.Contains(unitOfMeasure?.ToUpper());
        }
    }
}

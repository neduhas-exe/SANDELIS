using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for ProductMovementRegistrationDto
    /// </summary>
    public class ProductMovementRegistrationDtoValidator : AbstractValidator<ProductMovementRegistrationDto>
    {
        public ProductMovementRegistrationDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.MovementType)
                .NotEmpty().WithMessage("Judėjimo tipas yra privalomas")
                .Must(BeValidMovementType).WithMessage("Neteisingas judėjimo tipas. Galimi variantai: IN, OUT, TRANSFER");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Kiekis turi būti didesnis už 0");

            RuleFor(x => x.SourceLocation)
                .MaximumLength(50).WithMessage("Pradinės vietos kodas negali būti ilgesnis nei 50 simbolių")
                .NotEmpty().WithMessage("Pradinės vietos kodas yra privalomas")
                .When(x => x.MovementType != "IN");

            RuleFor(x => x.DestinationLocation)
                .MaximumLength(50).WithMessage("Galinės vietos kodas negali būti ilgesnis nei 50 simbolių")
                .NotEmpty().WithMessage("Galinės vietos kodas yra privalomas")
                .When(x => x.MovementType != "OUT");

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(50).WithMessage("Operacijos numeris negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.ReferenceNumber));

            RuleFor(x => x.QRCodeId)
                .MaximumLength(50).WithMessage("QR kodo ID negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.QRCodeId));

            // Custom validation rules for each movement type
            When(x => x.MovementType == "IN", () => {
                RuleFor(x => x.SourceLocation)
                    .Null().WithMessage("Pradinė vieta neturėtų būti nurodyta judėjimo tipui IN");
                RuleFor(x => x.DestinationLocation)
                    .NotEmpty().WithMessage("Galinė vieta yra privaloma judėjimo tipui IN");
            });

            When(x => x.MovementType == "OUT", () => {
                RuleFor(x => x.DestinationLocation)
                    .Null().WithMessage("Galinė vieta neturėtų būti nurodyta judėjimo tipui OUT");
                RuleFor(x => x.SourceLocation)
                    .NotEmpty().WithMessage("Pradinė vieta yra privaloma judėjimo tipui OUT");
            });

            When(x => x.MovementType == "TRANSFER", () => {
                RuleFor(x => x.SourceLocation)
                    .NotEmpty().WithMessage("Pradinė vieta yra privaloma judėjimo tipui TRANSFER");
                RuleFor(x => x.DestinationLocation)
                    .NotEmpty().WithMessage("Galinė vieta yra privaloma judėjimo tipui TRANSFER")
                    .NotEqual(x => x.SourceLocation).WithMessage("Pradinė ir galinė vietos negali būti tos pačios");
            });
        }

        private bool BeValidMovementType(string movementType)
        {
            var validTypes = new[] { "IN", "OUT", "TRANSFER" };
            return validTypes.Contains(movementType?.ToUpper());
        }
    }
}

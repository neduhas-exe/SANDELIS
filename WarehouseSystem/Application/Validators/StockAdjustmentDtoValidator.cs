using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for StockAdjustmentDto
    /// </summary>
    public class StockAdjustmentDtoValidator : AbstractValidator<StockAdjustmentDto>
    {
        public StockAdjustmentDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.LocationCode)
                .NotEmpty().WithMessage("Vietos kodas yra privalomas")
                .MaximumLength(50).WithMessage("Vietos kodas negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.NewQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Naujas kiekis negali būti neigiamas");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Koregavimo priežastis yra privaloma")
                .MaximumLength(500).WithMessage("Koregavimo priežastis negali būti ilgesnė nei 500 simbolių");

            RuleFor(x => x.AdjustedByUser)
                .NotEmpty().WithMessage("Koregavusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.ApprovedByUser)
                .NotEmpty().WithMessage("Patvirtinusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių")
                .NotEqual(x => x.AdjustedByUser).WithMessage("Patvirtinantis vartotojas negali būti tas pats asmuo, kuris atliko koregavimą");

            RuleFor(x => x.AdjustmentDate)
                .NotEmpty().WithMessage("Koregavimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Koregavimo data negali būti ateityje");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Pastabos negali būti ilgesnės nei 1000 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}

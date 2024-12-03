using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for StockCountDto
    /// </summary>
    public class StockCountDtoValidator : AbstractValidator<StockCountDto>
    {
        public StockCountDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.LocationCode)
                .NotEmpty().WithMessage("Vietos kodas yra privalomas")
                .MaximumLength(50).WithMessage("Vietos kodas negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.CountedQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Suskaičiuotas kiekis negali būti neigiamas");

            RuleFor(x => x.CountedByUser)
                .NotEmpty().WithMessage("Skaičiavusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.CountDate)
                .NotEmpty().WithMessage("Skaičiavimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Skaičiavimo data negali būti ateityje");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Pastabos negali būti ilgesnės nei 1000 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}

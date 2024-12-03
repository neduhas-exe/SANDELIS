using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for MovementQuantityAdjustmentDto
    /// </summary>
    public class MovementQuantityAdjustmentDtoValidator : AbstractValidator<MovementQuantityAdjustmentDto>
    {
        public MovementQuantityAdjustmentDtoValidator()
        {
            RuleFor(x => x.NewQuantity)
                .GreaterThan(0).WithMessage("Naujas kiekis turi būti didesnis už 0")
                .Must(BeReasonableQuantity).WithMessage("Kiekis atrodo neįprastai didelis. Prašome patikrinti.");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Koregavimo priežastis yra privaloma")
                .MaximumLength(500).WithMessage("Koregavimo priežastis negali būti ilgesnė nei 500 simbolių")
                .MinimumLength(10).WithMessage("Koregavimo priežastis turi būti bent 10 simbolių ilgio");

            RuleFor(x => x.AdjustedByUser)
                .NotEmpty().WithMessage("Koregavusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");
        }

        private bool BeReasonableQuantity(decimal quantity)
        {
            // Pavyzdinis patikrinimas - reikėtų pritaikyti pagal realius verslo poreikius
            const decimal maxReasonableQuantity = 1000000; // 1 milijonas
            return quantity <= maxReasonableQuantity;
        }
    }
}

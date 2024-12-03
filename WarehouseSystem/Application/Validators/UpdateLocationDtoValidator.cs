using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for UpdateLocationDto
    /// </summary>
    public class UpdateLocationDtoValidator : AbstractValidator<UpdateLocationDto>
    {
        public UpdateLocationDtoValidator()
        {
            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Maksimali talpa turi būti didesnė už 0")
                .When(x => x.MaxCapacity.HasValue);

            RuleFor(x => x.StorageConditions)
                .MaximumLength(500).WithMessage("Saugojimo sąlygų aprašymas negali būti ilgesnis nei 500 simbolių")
                .When(x => !string.IsNullOrEmpty(x.StorageConditions));

            RuleFor(x => x.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimalus kiekis negali būti neigiamas")
                .When(x => x.MinimumQuantity.HasValue);

            RuleFor(x => x.Temperature)
                .InclusiveBetween(-50, 50).WithMessage("Temperatūra turi būti tarp -50°C ir +50°C")
                .When(x => x.Temperature.HasValue);

            RuleFor(x => x.Humidity)
                .InclusiveBetween(0, 100).WithMessage("Drėgmė turi būti tarp 0% ir 100%")
                .When(x => x.Humidity.HasValue);

            RuleFor(x => x.UpdatedByUser)
                .NotEmpty().WithMessage("Atnaujinusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.UpdateReason)
                .NotEmpty().WithMessage("Atnaujinimo priežastis yra privaloma")
                .MaximumLength(500).WithMessage("Atnaujinimo priežastis negali būti ilgesnė nei 500 simbolių");
        }
    }
}

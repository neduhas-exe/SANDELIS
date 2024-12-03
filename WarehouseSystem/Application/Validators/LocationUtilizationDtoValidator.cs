using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for LocationUtilizationDto
    /// </summary>
    public class LocationUtilizationDtoValidator : AbstractValidator<LocationUtilizationDto>
    {
        public LocationUtilizationDtoValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.LocationCode)
                .NotEmpty().WithMessage("Vietos kodas yra privalomas")
                .MaximumLength(50).WithMessage("Vietos kodas negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.CurrentQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Esamas kiekis negali būti neigiamas");

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Maksimali talpa turi būti didesnė už 0");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Matavimo vienetas yra privalomas")
                .Must(BeValidUnitOfMeasure).WithMessage("Neteisingas matavimo vienetas. Galimi variantai: VNT, KG, M, L");

            RuleFor(x => x.LastUpdated)
                .NotEmpty().WithMessage("Paskutinio atnaujinimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Paskutinio atnaujinimo data negali būti ateityje");
        }

        private bool BeValidUnitOfMeasure(string unitOfMeasure)
        {
            var validUnits = new[] { "VNT", "KG", "M", "L" };
            return validUnits.Contains(unitOfMeasure?.ToUpper());
        }
    }
}

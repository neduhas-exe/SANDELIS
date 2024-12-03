using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for CreateLocationDto
    /// </summary>
    public class CreateLocationDtoValidator : AbstractValidator<CreateLocationDto>
    {
        public CreateLocationDtoValidator()
        {
            RuleFor(x => x.Zone)
                .NotEmpty().WithMessage("Zona yra privaloma")
                .MaximumLength(10).WithMessage("Zonos kodas negali būti ilgesnis nei 10 simbolių")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Zonos kodas turi būti sudarytas iš didžiųjų raidžių ir skaičių");

            RuleFor(x => x.Aisle)
                .NotEmpty().WithMessage("Praėjimas yra privalomas")
                .MaximumLength(10).WithMessage("Praėjimo kodas negali būti ilgesnis nei 10 simbolių")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Praėjimo kodas turi būti sudarytas iš didžiųjų raidžių ir skaičių");

            RuleFor(x => x.Rack)
                .NotEmpty().WithMessage("Lentynų blokas yra privalomas")
                .MaximumLength(10).WithMessage("Lentynų bloko kodas negali būti ilgesnis nei 10 simbolių")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Lentynų bloko kodas turi būti sudarytas iš didžiųjų raidžių ir skaičių");

            RuleFor(x => x.Shelf)
                .NotEmpty().WithMessage("Lentyna yra privaloma")
                .MaximumLength(10).WithMessage("Lentynos kodas negali būti ilgesnis nei 10 simbolių")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Lentynos kodas turi būti sudarytas iš didžiųjų raidžių ir skaičių");

            RuleFor(x => x.Bin)
                .MaximumLength(10).WithMessage("Dėžės kodas negali būti ilgesnis nei 10 simbolių")
                .Matches(@"^[A-Z0-9]+$").WithMessage("Dėžės kodas turi būti sudarytas iš didžiųjų raidžių ir skaičių")
                .When(x => !string.IsNullOrEmpty(x.Bin));

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Maksimali talpa turi būti didesnė už 0");

            RuleFor(x => x.StorageConditions)
                .MaximumLength(500).WithMessage("Saugojimo sąlygų aprašymas negali būti ilgesnis nei 500 simbolių")
                .When(x => !string.IsNullOrEmpty(x.StorageConditions));

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Matavimo vienetas yra privalomas")
                .Must(BeValidUnitOfMeasure).WithMessage("Neteisingas matavimo vienetas. Galimi variantai: VNT, KG, M, L");

            RuleFor(x => x.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimalus kiekis negali būti neigiamas");

            // Custom rule to validate location code uniqueness
            RuleFor(x => new { x.Zone, x.Aisle, x.Rack, x.Shelf, x.Bin })
                .Must(location => BeUniqueLocationCode(location.Zone, location.Aisle, location.Rack, location.Shelf, location.Bin))
                .WithMessage("Lokacijos kodas turi būti unikalus");
        }

        private bool BeValidUnitOfMeasure(string unitOfMeasure)
        {
            var validUnits = new[] { "VNT", "KG", "M", "L" };
            return validUnits.Contains(unitOfMeasure?.ToUpper());
        }

        private bool BeUniqueLocationCode(string zone, string aisle, string rack, string shelf, string bin)
        {
            // Šią funkciją reikėtų realizuoti su tikru duomenų patikrinimu
            // Čia tik demonstracinė versija
            var locationCode = $"{zone}-{aisle}-{rack}-{shelf}" + (!string.IsNullOrEmpty(bin) ? $"-{bin}" : "");
            return !string.IsNullOrEmpty(locationCode); // Placeholder return
        }
    }
}

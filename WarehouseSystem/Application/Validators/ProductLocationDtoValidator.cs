using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for ProductLocationDto
    /// </summary>
    public class ProductLocationDtoValidator : AbstractValidator<ProductLocationDto>
    {
        public ProductLocationDtoValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.Zone)
                .NotEmpty().WithMessage("Zona yra privaloma")
                .MaximumLength(10).WithMessage("Zonos kodas negali būti ilgesnis nei 10 simbolių");

            RuleFor(x => x.Aisle)
                .NotEmpty().WithMessage("Praėjimas yra privalomas")
                .MaximumLength(10).WithMessage("Praėjimo kodas negali būti ilgesnis nei 10 simbolių");

            RuleFor(x => x.Rack)
                .NotEmpty().WithMessage("Lentynų blokas yra privalomas")
                .MaximumLength(10).WithMessage("Lentynų bloko kodas negali būti ilgesnis nei 10 simbolių");

            RuleFor(x => x.Shelf)
                .NotEmpty().WithMessage("Lentyna yra privaloma")
                .MaximumLength(10).WithMessage("Lentynos kodas negali būti ilgesnis nei 10 simbolių");

            RuleFor(x => x.Bin)
                .MaximumLength(10).WithMessage("Dėžės kodas negali būti ilgesnis nei 10 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Bin));

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Kiekis negali būti neigiamas");

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Maksimali talpa turi būti didesnė už 0");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Matavimo vienetas yra privalomas")
                .Must(BeValidUnitOfMeasure).WithMessage("Neteisingas matavimo vienetas. Galimi variantai: VNT, KG, M, L");

            RuleFor(x => x.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimalus kiekis negali būti neigiamas");

            RuleFor(x => x.StorageConditions)
                .MaximumLength(500).WithMessage("Saugojimo sąlygų aprašymas negali būti ilgesnis nei 500 simbolių")
                .When(x => !string.IsNullOrEmpty(x.StorageConditions));

            RuleFor(x => x.Temperature)
                .InclusiveBetween(-50, 50).WithMessage("Temperatūra turi būti tarp -50°C ir +50°C")
                .When(x => x.Temperature != 0);

            RuleFor(x => x.Humidity)
                .InclusiveBetween(0, 100).WithMessage("Drėgmė turi būti tarp 0% ir 100%")
                .When(x => x.Humidity != 0);

            RuleFor(x => x.StockValue)
                .GreaterThanOrEqualTo(0).WithMessage("Prekių vertė negali būti neigiama");

            RuleFor(x => x.StockValueVAT)
                .GreaterThanOrEqualTo(0).WithMessage("Prekių vertė su PVM negali būti neigiama")
                .GreaterThanOrEqualTo(x => x.StockValue)
                    .WithMessage("Prekių vertė su PVM negali būti mažesnė už vertę be PVM");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Valiuta yra privaloma")
                .Must(BeValidCurrency).WithMessage("Neteisinga valiuta. Galimi variantai: EUR, USD");

            RuleFor(x => x.LastUpdated)
                .NotEmpty().WithMessage("Paskutinio atnaujinimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Paskutinio atnaujinimo data negali būti ateityje");

            RuleFor(x => x.UpdatedByUser)
                .NotEmpty().WithMessage("Atnaujinusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.LastOperationType)
                .NotEmpty().WithMessage("Paskutinės operacijos tipas yra privalomas")
                .Must(BeValidOperationType).WithMessage("Neteisingas operacijos tipas. Galimi variantai: IN, OUT, TRANSFER, ADJUSTMENT");
        }

        private bool BeValidUnitOfMeasure(string unitOfMeasure)
        {
            var validUnits = new[] { "VNT", "KG", "M", "L" };
            return validUnits.Contains(unitOfMeasure?.ToUpper());
        }

        private bool BeValidCurrency(string currency)
        {
            var validCurrencies = new[] { "EUR", "USD" };
            return validCurrencies.Contains(currency?.ToUpper());
        }

        private bool BeValidOperationType(string operationType)
        {
            var validTypes = new[] { "IN", "OUT", "TRANSFER", "ADJUSTMENT" };
            return validTypes.Contains(operationType?.ToUpper());
        }
    }
}

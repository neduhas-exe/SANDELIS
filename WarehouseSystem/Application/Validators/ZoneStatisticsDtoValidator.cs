using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for ZoneStatisticsDto
    /// </summary>
    public class ZoneStatisticsDtoValidator : AbstractValidator<ZoneStatisticsDto>
    {
        public ZoneStatisticsDtoValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.Zone)
                .NotEmpty().WithMessage("Zona yra privaloma")
                .MaximumLength(10).WithMessage("Zonos kodas negali būti ilgesnis nei 10 simbolių");

            RuleFor(x => x.TotalLocations)
                .GreaterThanOrEqualTo(0).WithMessage("Bendras lokacijų skaičius negali būti neigiamas");

            RuleFor(x => x.EmptyLocations)
                .GreaterThanOrEqualTo(0).WithMessage("Tuščių lokacijų skaičius negali būti neigiamas")
                .LessThanOrEqualTo(x => x.TotalLocations).WithMessage("Tuščių lokacijų skaičius negali viršyti bendro lokacijų skaičiaus");

            RuleFor(x => x.PartiallyFullLocations)
                .GreaterThanOrEqualTo(0).WithMessage("Dalinai užpildytų lokacijų skaičius negali būti neigiamas");

            RuleFor(x => x.FullLocations)
                .GreaterThanOrEqualTo(0).WithMessage("Pilnai užpildytų lokacijų skaičius negali būti neigiamas");

            RuleFor(x => x)
                .Must(x => x.EmptyLocations + x.PartiallyFullLocations + x.FullLocations == x.TotalLocations)
                .WithMessage("Lokacijų skaičių suma turi būti lygi bendram lokacijų skaičiui");

            RuleFor(x => x.TotalCapacity)
                .GreaterThan(0).WithMessage("Bendra talpa turi būti didesnė už 0");

            RuleFor(x => x.UsedCapacity)
                .GreaterThanOrEqualTo(0).WithMessage("Panaudota talpa negali būti neigiama")
                .LessThanOrEqualTo(x => x.TotalCapacity).WithMessage("Panaudota talpa negali viršyti bendros talpos");

            RuleFor(x => x.LastUpdated)
                .NotEmpty().WithMessage("Paskutinio atnaujinimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Paskutinio atnaujinimo data negali būti ateityje");
        }
    }
}

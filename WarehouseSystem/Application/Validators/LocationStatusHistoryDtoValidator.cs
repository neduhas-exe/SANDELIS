using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for LocationStatusHistoryDto
    /// </summary>
    public class LocationStatusHistoryDtoValidator : AbstractValidator<LocationStatusHistoryDto>
    {
        public LocationStatusHistoryDtoValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.LocationCode)
                .NotEmpty().WithMessage("Vietos kodas yra privalomas")
                .MaximumLength(50).WithMessage("Vietos kodas negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.ChangeType)
                .NotEmpty().WithMessage("Pakeitimo tipas yra privalomas")
                .Must(BeValidChangeType).WithMessage("Neteisingas pakeitimo tipas. Galimi variantai: Created, Updated, Deleted");

            RuleFor(x => x.OldQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Senas kiekis negali būti neigiamas")
                .When(x => x.OldQuantity.HasValue);

            RuleFor(x => x.NewQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Naujas kiekis negali būti neigiamas")
                .When(x => x.NewQuantity.HasValue);

            RuleFor(x => x.ChangedByUser)
                .NotEmpty().WithMessage("Pakeitusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.ChangeReason)
                .NotEmpty().WithMessage("Pakeitimo priežastis yra privaloma")
                .MaximumLength(500).WithMessage("Pakeitimo priežastis negali būti ilgesnė nei 500 simbolių");

            RuleFor(x => x.ChangeDate)
                .NotEmpty().WithMessage("Pakeitimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Pakeitimo data negali būti ateityje");
        }

        private bool BeValidChangeType(string changeType)
        {
            var validTypes = new[] { "Created", "Updated", "Deleted" };
            return validTypes.Contains(changeType);
        }
    }
}

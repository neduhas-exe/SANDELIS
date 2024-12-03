using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for MovementCancellationDto
    /// </summary>
    public class MovementCancellationDtoValidator : AbstractValidator<MovementCancellationDto>
    {
        public MovementCancellationDtoValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Atšaukimo priežastis yra privaloma")
                .MaximumLength(500).WithMessage("Atšaukimo priežastis negali būti ilgesnė nei 500 simbolių")
                .MinimumLength(10).WithMessage("Atšaukimo priežastis turi būti bent 10 simbolių ilgio");

            RuleFor(x => x.CanceledByUser)
                .NotEmpty().WithMessage("Atšaukusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            // Čia galima pridėti papildomą validaciją
            RuleFor(x => x.Reason)
                .Must(ContainValidReason).WithMessage("Priežastis turi atitikti vieną iš galimų atšaukimo priežasčių");
        }

        private bool ContainValidReason(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return false;

            var validReasons = new[]
            {
                "KLAIDA",
                "ATŠAUKTAS",
                "NETEISINGAS_KIEKIS",
                "NETEISINGAS_PRODUKTAS",
                "NETEISINGAS_KODAS",
                "KITA"
            };

            return validReasons.Any(r => reason.ToUpper().Contains(r));
        }
    }
}

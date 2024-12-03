using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for UpdateProductDto
    /// </summary>
    public class UpdateProductDtoValidator : AbstractValidator<UpdateProductDto>
    {
        public UpdateProductDtoValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.Name)
                .MaximumLength(200).WithMessage("Pavadinimas negali būti ilgesnis nei 200 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.NameEn)
                .MaximumLength(200).WithMessage("Angliškas pavadinimas negali būti ilgesnis nei 200 simbolių")
                .When(x => !string.IsNullOrEmpty(x.NameEn));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Aprašymas negali būti ilgesnis nei 2000 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.NewQRCode)
                .MaximumLength(50).WithMessage("QR kodas negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.NewQRCode));

            RuleFor(x => x.WeightNet)
                .GreaterThanOrEqualTo(0).WithMessage("Neto svoris negali būti neigiamas")
                .When(x => x.WeightNet.HasValue);

            RuleFor(x => x.WeightGross)
                .GreaterThanOrEqualTo(0).WithMessage("Bruto svoris negali būti neigiamas")
                .GreaterThanOrEqualTo(x => x.WeightNet ?? 0)
                    .WithMessage("Bruto svoris negali būti mažesnis už neto svorį")
                .When(x => x.WeightGross.HasValue);

            RuleFor(x => x.RetailPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Pardavimo kaina negali būti neigiama")
                .When(x => x.RetailPrice.HasValue);

            RuleFor(x => x.PurchasePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Pirkimo kaina negali būti neigiama")
                .When(x => x.PurchasePrice.HasValue);

            RuleFor(x => x.WholesalePrice)
                .GreaterThanOrEqualTo(0).WithMessage("Didmeninė kaina negali būti neigiama")
                .When(x => x.WholesalePrice.HasValue);

            RuleFor(x => x.VATRate)
                .InclusiveBetween(0, 100).WithMessage("PVM tarifas turi būti tarp 0 ir 100")
                .When(x => x.VATRate.HasValue);

            RuleFor(x => x.Status)
                .Must(BeValidStatus).WithMessage("Neteisinga būsena. Galimos reikšmės: Active, Discontinued, OutOfStock")
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.UpdatedByUser)
                .NotEmpty().WithMessage("Atnaujinusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.UpdateReason)
                .NotEmpty().WithMessage("Atnaujinimo priežastis yra privaloma")
                .MaximumLength(500).WithMessage("Atnaujinimo priežastis negali būti ilgesnė nei 500 simbolių");

            RuleFor(x => x.UpdatedAt)
                .NotEmpty().WithMessage("Atnaujinimo data yra privaloma")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Atnaujinimo data negali būti ateityje");
        }

        private bool BeValidStatus(string status)
        {
            if (string.IsNullOrEmpty(status)) return true;
            var validStatuses = new[] { "Active", "Discontinued", "OutOfStock" };
            return validStatuses.Contains(status);
        }
    }
}

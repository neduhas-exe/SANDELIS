using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for UpdateQRCodeStatusDto
    /// </summary>
    public class UpdateQRCodeStatusDtoValidator : AbstractValidator<UpdateQRCodeStatusDto>
    {
        public UpdateQRCodeStatusDtoValidator()
        {
            RuleFor(x => x.QRCodeId)
                .NotEmpty().WithMessage("QR kodo ID yra privalomas")
                .MaximumLength(50).WithMessage("QR kodo ID negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.NewStatus)
                .NotEmpty().WithMessage("Nauja būsena yra privaloma")
                .Must(BeValidStatus).WithMessage("Neteisinga būsena. Galimos reikšmės: Active, Used, Damaged, Lost");

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
            var validStatuses = new[] { "Active", "Used", "Damaged", "Lost" };
            return validStatuses.Contains(status);
        }
    }
}

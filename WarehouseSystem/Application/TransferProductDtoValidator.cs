using FluentValidation;
using Presentation.DTOs.Products;
using System;

namespace WarehouseSystem.Application.Validators
{
    /// <summary>
    /// Validator for TransferProductDto
    /// </summary>
    public class TransferProductDtoValidator : AbstractValidator<TransferProductDto>
    {
        public TransferProductDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Produkto ID privalo būti teigiamas skaičius");

            RuleFor(x => x.SourceWarehouseId)
                .NotEmpty().WithMessage("Išsiuntimo sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.SourceLocationCode)
                .NotEmpty().WithMessage("Išsiuntimo vietos kodas yra privalomas")
                .MaximumLength(50).WithMessage("Vietos kodas negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x.DestinationWarehouseId)
                .NotEmpty().WithMessage("Gavimo sandėlio ID yra privalomas")
                .MaximumLength(20).WithMessage("Sandėlio ID negali būti ilgesnis nei 20 simbolių");

            RuleFor(x => x.DestinationLocationCode)
                .NotEmpty().WithMessage("Gavimo vietos kodas yra privalomas")
                .MaximumLength(50).WithMessage("Vietos kodas negali būti ilgesnis nei 50 simbolių");

            RuleFor(x => x)
                .Must(x => x.SourceWarehouseId != x.DestinationWarehouseId || 
                          x.SourceLocationCode != x.DestinationLocationCode)
                .WithMessage("Išsiuntimo ir gavimo vietos negali būti tos pačios");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Kiekis turi būti didesnis už 0");

            RuleFor(x => x.QRCodeId)
                .MaximumLength(50).WithMessage("QR kodo ID negali būti ilgesnis nei 50 simbolių")
                .When(x => !string.IsNullOrEmpty(x.QRCodeId));

            RuleFor(x => x.TransferredByUser)
                .NotEmpty().WithMessage("Perkėlusio vartotojo vardas yra privalomas")
                .MaximumLength(100).WithMessage("Vartotojo vardas negali būti ilgesnis nei 100 simbolių");

            RuleFor(x => x.TransferReason)
                .NotEmpty().WithMessage("Perkėlimo priežastis yra privaloma")
                .MaximumLength(500).WithMessage("Perkėlimo priežastis negali būti ilgesnė nei 500 simbolių");

            RuleFor(x => x.Notes)
                .MaximumLength(1000).WithMessage("Pastabos negali būti ilgesnės nei 1000 simbolių")
                .When(x => !string.IsNullOrEmpty(x.Notes));
        }
    }
}

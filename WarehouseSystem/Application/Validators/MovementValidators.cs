using FluentValidation;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Validators
{
    public class ProductMovementValidator : InputValidatorBase<ProductMovementDto>
    {
        public ProductMovementValidator()
        {
            RuleFor(x => x.MovementId)
                .GreaterThan(0).WithMessage("Invalid movement ID");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Invalid product ID");

            RuleFor(x => x.QRCodeId)
                .Matches(@"^[A-Fa-f0-9]{32}$").WithMessage("QR code must be 32 characters hexadecimal string")
                .When(x => !string.IsNullOrEmpty(x.QRCodeId));

            RuleFor(x => x.MovementType)
                .NotEmpty().WithMessage("Movement type is required")
                .Must(type => new[] { "IN", "OUT", "TRANSFER" }.Contains(type))
                .WithMessage("Invalid movement type. Allowed values: IN, OUT, TRANSFER");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Unit of measure is required")
                .Must(uom => new[] { "vnt", "m", "kg", "l" }.Contains(uom.ToLower()))
                .WithMessage("Invalid unit of measure. Allowed values: vnt, m, kg, l");

            RuleFor(x => x.SourceLocation)
                .NotEmpty().WithMessage("Source location is required")
                .Matches(@"^[A-Z]-\d{2}-\d{2}-\d{2}-\d{2}$")
                .WithMessage("Invalid source location format")
                .When(x => x.MovementType != "IN");

            RuleFor(x => x.DestinationLocation)
                .NotEmpty().WithMessage("Destination location is required")
                .Matches(@"^[A-Z]-\d{2}-\d{2}-\d{2}-\d{2}$")
                .WithMessage("Invalid destination location format")
                .When(x => x.MovementType != "OUT");

            RuleFor(x => x.MovementDate)
                .NotEmpty().WithMessage("Movement date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Movement date cannot be in the future");

            RuleFor(x => x.MovedByUser)
                .NotEmpty().WithMessage("Moved by user is required")
                .MaximumLength(100).WithMessage("Moved by user must not exceed 100 characters");

            RuleFor(x => x.ReferenceNumber)
                .MaximumLength(50).WithMessage("Reference number must not exceed 50 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
        }
    }

    public class ProductMovementRegistrationValidator : InputValidatorBase<ProductMovementRegistrationDto>
    {
        public ProductMovementRegistrationValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Invalid product ID");

            RuleFor(x => x.MovementType)
                .NotEmpty().WithMessage("Movement type is required")
                .Must(type => new[] { "IN", "OUT", "TRANSFER" }.Contains(type))
                .WithMessage("Invalid movement type. Allowed values: IN, OUT, TRANSFER");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.SourceLocation)
                .NotEmpty().WithMessage("Source location is required")
                .Matches(@"^[A-Z]-\d{2}-\d{2}-\d{2}-\d{2}$")
                .WithMessage("Invalid source location format")
                .When(x => x.MovementType != "IN");

            RuleFor(x => x.DestinationLocation)
                .NotEmpty().WithMessage("Destination location is required")
                .Matches(@"^[A-Z]-\d{2}-\d{2}-\d{2}-\d{2}$")
                .WithMessage("Invalid destination location format")
                .When(x => x.MovementType != "OUT");

            RuleFor(x => x.QRCodeId)
                .Matches(@"^[A-Fa-f0-9]{32}$").WithMessage("QR code must be 32 characters hexadecimal string")
                .When(x => !string.IsNullOrEmpty(x.QRCodeId));
        }
    }

    public class MovementBusinessRuleValidator : IBusinessRuleValidator<ProductMovementDto>
    {
        private readonly IProductService _productService;
        private readonly IWarehouseService _warehouseService;

        public MovementBusinessRuleValidator(
            IProductService productService,
            IWarehouseService warehouseService)
        {
            _productService = productService;
            _warehouseService = warehouseService;
        }

        public async Task<ValidationResult> ValidateAsync(ProductMovementDto movement, ValidationContext context)
        {
            var result = new ValidationResult();

            try
            {
                // Validate product exists
                var product = await _productService.GetProductByIdAsync(movement.ProductId);
                if (product == null)
                {
                    result.AddError($"Product with ID {movement.ProductId} not found");
                    return result;
                }

                // Validate product is active
                if (!product.IsActive)
                {
                    result.AddError($"Product with ID {movement.ProductId} is not active");
                    return result;
                }

                // Validate locations for movement type
                switch (movement.MovementType)
                {
                    case "IN":
                        if (!await ValidateDestinationLocation(movement, result))
                            return result;
                        break;

                    case "OUT":
                        if (!await ValidateSourceLocation(movement, result))
                            return result;
                        break;

                    case "TRANSFER":
                        if (!await ValidateSourceLocation(movement, result))
                            return result;
                        if (!await ValidateDestinationLocation(movement, result))
                            return result;
                        break;
                }

                // Validate QR code if provided
                if (!string.IsNullOrEmpty(movement.QRCodeId))
                {
                    if (!await ValidateQRCode(movement, result))
                        return result;
                }

                // Validate movement quantity
                if (!await ValidateMovementQuantity(movement, result))
                    return result;
            }
            catch (Exception ex)
            {
                result.AddError($"Validation error: {ex.Message}");
            }

            return result;
        }

        private async Task<bool> ValidateSourceLocation(ProductMovementDto movement, ValidationResult result)
        {
            var sourceLocation = await _warehouseService.GetLocationAsync(
                ExtractWarehouseId(movement.SourceLocation),
                movement.SourceLocation
            );

            if (sourceLocation == null)
            {
                result.AddError($"Source location {movement.SourceLocation} not found");
                return false;
            }

            if (sourceLocation.IsQuarantine)
            {
                result.AddError($"Source location {movement.SourceLocation} is in quarantine");
                return false;
            }

            if (sourceLocation.Quantity < movement.Quantity)
            {
                result.AddError($"Insufficient quantity in source location {movement.SourceLocation}");
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateDestinationLocation(ProductMovementDto movement, ValidationResult result)
        {
            var destinationLocation = await _warehouseService.GetLocationAsync(
                ExtractWarehouseId(movement.DestinationLocation),
                movement.DestinationLocation
            );

            if (destinationLocation == null)
            {
                result.AddError($"Destination location {movement.DestinationLocation} not found");
                return false;
            }

            if (destinationLocation.IsQuarantine && movement.MovementType != "OUT")
            {
                result.AddError($"Destination location {movement.DestinationLocation} is in quarantine");
                return false;
            }

            var availableCapacity = destinationLocation.MaxCapacity - destinationLocation.Quantity;
            if (movement.Quantity > availableCapacity)
            {
                result.AddError($"Insufficient capacity in destination location {movement.DestinationLocation}");
                return false;
            }

            return true;
        }

        private async Task<bool> ValidateQRCode(ProductMovementDto movement, ValidationResult result)
        {
            // Implementation depends on your QR code service
            // This is just a placeholder
            return true;
        }

        private async Task<bool> ValidateMovementQuantity(ProductMovementDto movement, ValidationResult result)
        {
            if (movement.Quantity <= 0)
            {
                result.AddError("Movement quantity must be greater than 0");
                return false;
            }

            // Additional quantity validations can be added here
            return true;
        }

        private string ExtractWarehouseId(string locationCode)
        {
            // Implement your warehouse ID extraction logic
            // This is just a placeholder
            return "MAIN";
        }
    }

    public class MovementCancellationValidator : InputValidatorBase<MovementCancellationDto>
    {
        public MovementCancellationValidator()
        {
            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Cancellation reason is required")
                .MaximumLength(500).WithMessage("Cancellation reason must not exceed 500 characters");

            RuleFor(x => x.CanceledByUser)
                .NotEmpty().WithMessage("Canceled by user is required")
                .MaximumLength(100).WithMessage("Canceled by user must not exceed 100 characters");
        }
    }

    public class MovementQuantityAdjustmentValidator : InputValidatorBase<MovementQuantityAdjustmentDto>
    {
        public MovementQuantityAdjustmentValidator()
        {
            RuleFor(x => x.NewQuantity)
                .GreaterThan(0).WithMessage("New quantity must be greater than 0");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Adjustment reason is required")
                .MaximumLength(500).WithMessage("Adjustment reason must not exceed 500 characters");

            RuleFor(x => x.AdjustedByUser)
                .NotEmpty().WithMessage("Adjusted by user is required")
                .MaximumLength(100).WithMessage("Adjusted by user must not exceed 100 characters");
        }
    }
}

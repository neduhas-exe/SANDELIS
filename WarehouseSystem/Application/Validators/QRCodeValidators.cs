using FluentValidation;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Validators
{
    public class ProductQRCodeValidator : InputValidatorBase<ProductQRCodeDto>
    {
        public ProductQRCodeValidator()
        {
            RuleFor(x => x.QRCodeId)
                .NotEmpty().WithMessage("QR code ID is required")
                .Matches(@"^[A-Fa-f0-9]{32}$").WithMessage("QR code must be 32 characters hexadecimal string");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Invalid product ID");

            RuleFor(x => x.BatchNumber)
                .MaximumLength(50).WithMessage("Batch number must not exceed 50 characters")
                .Matches(@"^[A-Za-z0-9-_]+$").WithMessage("Batch number can only contain letters, numbers, hyphens and underscores")
                .When(x => !string.IsNullOrEmpty(x.BatchNumber));

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0");

            RuleFor(x => x.QRCodeType)
                .NotEmpty().WithMessage("QR code type is required")
                .Must(type => new[] { "Batch", "Individual" }.Contains(type))
                .WithMessage("Invalid QR code type. Allowed values: Batch, Individual");

            RuleFor(x => x.ReceivedDate)
                .NotEmpty().WithMessage("Received date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Received date cannot be in the future");

            RuleFor(x => x.ReceivedByUser)
                .NotEmpty().WithMessage("Received by user is required")
                .MaximumLength(100).WithMessage("Received by user must not exceed 100 characters");

            RuleFor(x => x.WarehouseLocation)
                .NotEmpty().WithMessage("Warehouse location is required")
                .Matches(@"^[A-Z]-\d{2}-\d{2}-\d{2}-\d{2}$")
                .WithMessage("Invalid warehouse location format. Must be in format: Zone-Aisle-Rack-Shelf-Bin (e.g. A-01-02-03-04)");

            RuleFor(x => x.PurchaseInvoice)
                .MaximumLength(50).WithMessage("Purchase invoice number must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.PurchaseInvoice));

            RuleFor(x => x.SupplierName)
                .MaximumLength(255).WithMessage("Supplier name must not exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.SupplierName));

            RuleFor(x => x.SupplierInvoiceNumber)
                .MaximumLength(50).WithMessage("Supplier invoice number must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.SupplierInvoiceNumber));

            RuleFor(x => x.SupplierOrderNumber)
                .MaximumLength(50).WithMessage("Supplier order number must not exceed 50 characters")
                .When(x => !string.IsNullOrEmpty(x.SupplierOrderNumber));

            RuleFor(x => x.BatchPurchasePrice)
                .GreaterThan(0).WithMessage("Batch purchase price must be greater than 0");

            RuleFor(x => x.VATRate)
                .InclusiveBetween(0, 100).WithMessage("VAT rate must be between 0 and 100");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Must(curr => new[] { "EUR", "USD", "GBP" }.Contains(curr))
                .WithMessage("Invalid currency code. Allowed values: EUR, USD, GBP");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(status => new[] { "Active", "Used", "Damaged", "Lost" }.Contains(status))
                .WithMessage("Invalid status. Allowed values: Active, Used, Damaged, Lost");

            RuleFor(x => x.StatusChangedAt)
                .NotEmpty().WithMessage("Status changed date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Status changed date cannot be in the future");

            RuleFor(x => x.StatusChangedBy)
                .NotEmpty().WithMessage("Status changed by user is required")
                .MaximumLength(100).WithMessage("Status changed by user must not exceed 100 characters");
        }
    }

    public class AddProductQRCodeValidator : InputValidatorBase<AddProductQRCodeDto>
    {
        public AddProductQRCodeValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Invalid product ID");

            RuleFor(x => x.BatchNumber)
                .NotEmpty().WithMessage("Batch number is required")
                .MaximumLength(50).WithMessage("Batch number must not exceed 50 characters")
                .Matches(@"^[A-Za-z0-9-_]+$")
                .WithMessage("Batch number can only contain letters, numbers, hyphens and underscores");

            RuleFor(x => x.BatchQuantity)
                .GreaterThan(0).WithMessage("Batch quantity must be greater than 0");

            RuleFor(x => x.QRCodeType)
                .NotEmpty().WithMessage("QR code type is required")
                .Must(type => new[] { "Batch", "Individual" }.Contains(type))
                .WithMessage("Invalid QR code type. Allowed values: Batch, Individual");

            RuleFor(x => x.NumberOfQRCodes)
                .GreaterThan(0).WithMessage("Number of QR codes must be greater than 0")
                .LessThanOrEqualTo(1000).WithMessage("Number of QR codes cannot exceed 1000");

            RuleFor(x => x.PurchaseInvoice)
                .MaximumLength(50).WithMessage("Purchase invoice number must not exceed 50 characters");

            RuleFor(x => x.SupplierName)
                .NotEmpty().WithMessage("Supplier name is required")
                .MaximumLength(255).WithMessage("Supplier name must not exceed 255 characters");

            RuleFor(x => x.PurchasePrice)
                .GreaterThan(0).WithMessage("Purchase price must be greater than 0");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Must(curr => new[] { "EUR", "USD", "GBP" }.Contains(curr))
                .WithMessage("Invalid currency code. Allowed values: EUR, USD, GBP");

            RuleFor(x => x.VATRate)
                .InclusiveBetween(0, 100).WithMessage("VAT rate must be between 0 and 100");

            RuleFor(x => x.Location)
                .NotEmpty().WithMessage("Location is required")
                .Matches(@"^[A-Z]-\d{2}-\d{2}-\d{2}-\d{2}$")
                .WithMessage("Invalid location format. Must be in format: Zone-Aisle-Rack-Shelf-Bin (e.g. A-01-02-03-04)");

            RuleFor(x => x.ReceivedByUser)
                .NotEmpty().WithMessage("Received by user is required")
                .MaximumLength(100).WithMessage("Received by user must not exceed 100 characters");

            RuleFor(x => x.Notes)
                .MaximumLength(500).WithMessage("Notes must not exceed 500 characters");
        }
    }

    public class UpdateQRCodeStatusValidator : InputValidatorBase<UpdateQRCodeStatusDto>
    {
        public UpdateQRCodeStatusValidator()
        {
            RuleFor(x => x.QRCodeId)
                .NotEmpty().WithMessage("QR code ID is required")
                .Matches(@"^[A-Fa-f0-9]{32}$").WithMessage("QR code must be 32 characters hexadecimal string");

            RuleFor(x => x.NewStatus)
                .NotEmpty().WithMessage("New status is required")
                .Must(status => new[] { "Active", "Used", "Damaged", "Lost" }.Contains(status))
                .WithMessage("Invalid status. Allowed values: Active, Used, Damaged, Lost");

            RuleFor(x => x.UpdatedByUser)
                .NotEmpty().WithMessage("Updated by user is required")
                .MaximumLength(100).WithMessage("Updated by user must not exceed 100 characters");

            RuleFor(x => x.UpdateReason)
                .NotEmpty().WithMessage("Update reason is required")
                .MaximumLength(500).WithMessage("Update reason must not exceed 500 characters");

            RuleFor(x => x.UpdatedAt)
                .NotEmpty().WithMessage("Updated date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Updated date cannot be in the future");
        }
    }

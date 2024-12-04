using FluentValidation;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Validators
{
    public class ProductValidator : InputValidatorBase<ProductDto>
    {
        public ProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(255).WithMessage("Product name must not exceed 255 characters");

            RuleFor(x => x.NameEn)
                .MaximumLength(255).WithMessage("English name must not exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.NameEn));

            RuleFor(x => x.EANCode)
                .Matches(@"^\d{13}$").WithMessage("EAN code must be exactly 13 digits")
                .When(x => !string.IsNullOrEmpty(x.EANCode));

            RuleFor(x => x.LegacyCode)
                .Matches(@"^\d{7}$").WithMessage("Legacy code must be exactly 7 digits")
                .When(x => !string.IsNullOrEmpty(x.LegacyCode));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

            RuleFor(x => x.WeightGross)
                .GreaterThan(0).WithMessage("Gross weight must be greater than 0")
                .GreaterThanOrEqualTo(x => x.WeightNet).WithMessage("Gross weight must be greater than or equal to net weight");

            RuleFor(x => x.WeightNet)
                .GreaterThan(0).WithMessage("Net weight must be greater than 0");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Unit of measure is required")
                .Must(uom => new[] { "vnt", "m", "kg", "l" }.Contains(uom.ToLower()))
                .WithMessage("Invalid unit of measure. Allowed values: vnt, m, kg, l");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

            RuleFor(x => x.SubCategory)
                .MaximumLength(100).WithMessage("SubCategory must not exceed 100 characters");

            RuleFor(x => x.Manufacturer)
                .MaximumLength(255).WithMessage("Manufacturer must not exceed 255 characters");

            RuleFor(x => x.MinimumStock)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum stock cannot be negative");

            RuleFor(x => x.PurchasePrice)
                .GreaterThan(0).WithMessage("Purchase price must be greater than 0");

            RuleFor(x => x.RetailPrice)
                .GreaterThan(0).WithMessage("Retail price must be greater than 0")
                .GreaterThanOrEqualTo(x => x.PurchasePrice)
                .WithMessage("Retail price must be greater than or equal to purchase price");

            RuleFor(x => x.WholesalePrice)
                .GreaterThan(0).WithMessage("Wholesale price must be greater than 0")
                .GreaterThanOrEqualTo(x => x.PurchasePrice)
                .WithMessage("Wholesale price must be greater than or equal to purchase price");

            RuleFor(x => x.VATRate)
                .InclusiveBetween(0, 100).WithMessage("VAT rate must be between 0 and 100");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Must(curr => new[] { "EUR", "USD", "GBP" }.Contains(curr))
                .WithMessage("Invalid currency code. Allowed values: EUR, USD, GBP");

            RuleFor(x => x.Margin)
                .GreaterThanOrEqualTo(0).WithMessage("Margin cannot be negative");

            RuleFor(x => x.CreatedAt)
                .NotEmpty().WithMessage("Created date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Created date cannot be in the future");

            RuleFor(x => x.UpdatedAt)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Updated date cannot be in the future")
                .GreaterThanOrEqualTo(x => x.CreatedAt)
                .WithMessage("Updated date must be after created date")
                .When(x => x.UpdatedAt.HasValue);

            RuleFor(x => x.DiscontinuedAt)
                .GreaterThanOrEqualTo(x => x.CreatedAt)
                .WithMessage("Discontinued date must be after created date")
                .When(x => x.DiscontinuedAt.HasValue);

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required")
                .Must(status => new[] { "Active", "Discontinued", "OutOfStock" }.Contains(status))
                .WithMessage("Invalid status. Allowed values: Active, Discontinued, OutOfStock");

            RuleFor(x => x.QRCode)
                .Must(qr => string.IsNullOrEmpty(qr) || 
                           System.Text.RegularExpressions.Regex.IsMatch(qr, "^[A-Fa-f0-9]{32}$"))
                .WithMessage("QR code must be 32 characters hexadecimal string");

            RuleFor(x => x.BatchQRCodes)
                .Must(qrs => qrs == null || qrs.All(qr => 
                    System.Text.RegularExpressions.Regex.IsMatch(qr, "^[A-Fa-f0-9]{32}$")))
                .WithMessage("All batch QR codes must be 32 characters hexadecimal strings");

            RuleFor(x => x.ItemQRCodes)
                .Must(qrs => qrs == null || qrs.All(qr => 
                    System.Text.RegularExpressions.Regex.IsMatch(qr, "^[A-Fa-f0-9]{32}$")))
                .WithMessage("All item QR codes must be 32 characters hexadecimal strings");

            RuleFor(x => x.CountryOfOrigin)
                .MaximumLength(2).WithMessage("Country code must be ISO 2-letter code")
                .Matches(@"^[A-Z]{2}$").WithMessage("Country code must be 2 uppercase letters")
                .When(x => !string.IsNullOrEmpty(x.CountryOfOrigin));

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.Now).WithMessage("Expiry date must be in the future")
                .When(x => x.ExpiryDate.HasValue);

            RuleFor(x => x.QualityGrade)
                .Must(grade => string.IsNullOrEmpty(grade) || 
                              new[] { "A", "B", "C" }.Contains(grade.ToUpper()))
                .WithMessage("Invalid quality grade. Allowed values: A, B, C");
        }
    }

    public class CreateProductValidator : InputValidatorBase<CreateProductDto>
    {
        public CreateProductValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(255).WithMessage("Product name must not exceed 255 characters");

            RuleFor(x => x.EANCode)
                .Matches(@"^\d{13}$").WithMessage("EAN code must be exactly 13 digits")
                .When(x => !string.IsNullOrEmpty(x.EANCode));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters");

            RuleFor(x => x.WeightNet)
                .GreaterThan(0).WithMessage("Net weight must be greater than 0");

            RuleFor(x => x.WeightGross)
                .GreaterThan(0).WithMessage("Gross weight must be greater than 0")
                .GreaterThanOrEqualTo(x => x.WeightNet)
                .WithMessage("Gross weight must be greater than or equal to net weight");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Unit of measure is required")
                .Must(uom => new[] { "vnt", "m", "kg", "l" }.Contains(uom.ToLower()))
                .WithMessage("Invalid unit of measure. Allowed values: vnt, m, kg, l");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Category is required")
                .MaximumLength(100).WithMessage("Category must not exceed 100 characters");

            RuleFor(x => x.PurchasePrice)
                .GreaterThan(0).WithMessage("Purchase price must be greater than 0");

            RuleFor(x => x.RetailPrice)
                .GreaterThan(0).WithMessage("Retail price must be greater than 0")
                .GreaterThanOrEqualTo(x => x.PurchasePrice)
                .WithMessage("Retail price must be greater than or equal to purchase price");

            RuleFor(x => x.VATRate)
                .InclusiveBetween(0, 100).WithMessage("VAT rate must be between 0 and 100");

            RuleFor(x => x.Currency)
                .Must(curr => new[] { "EUR", "USD", "GBP" }.Contains(curr))
                .WithMessage("Invalid currency code. Allowed values: EUR, USD, GBP");

            RuleFor(x => x.CountryOfOrigin)
                .MaximumLength(2).WithMessage("Country code must be ISO 2-letter code")
                .Matches(@"^[A-Z]{2}$").WithMessage("Country code must be 2 uppercase letters")
                .When(x => !string.IsNullOrEmpty(x.CountryOfOrigin));
        }
    }

    public class UpdateProductValidator : InputValidatorBase<UpdateProductDto>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Invalid product ID");

            RuleFor(x => x.Name)
                .MaximumLength(255).WithMessage("Product name must not exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.Name));

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters")
                .When(x => !string.IsNullOrEmpty(x.Description));

            RuleFor(x => x.WeightNet)
                .GreaterThan(0).WithMessage("Net weight must be greater than 0")
                .When(x => x.WeightNet.HasValue);

            RuleFor(x => x.WeightGross)
                .GreaterThan(0).WithMessage("Gross weight must be greater than 0")
                .GreaterThanOrEqualTo(x => x.WeightNet ?? 0)
                .WithMessage("Gross weight must be greater than or equal to net weight")
                .When(x => x.WeightGross.HasValue);

            RuleFor(x => x.RetailPrice)
                .GreaterThan(0).WithMessage("Retail price must be greater than 0")
                .When(x => x.RetailPrice.HasValue);

            RuleFor(x => x.PurchasePrice)
                .GreaterThan(0).WithMessage("Purchase price must be greater than 0")
                .When(x => x.PurchasePrice.HasValue);

            RuleFor(x => x.WholesalePrice)
                .GreaterThan(0).WithMessage("Wholesale price must be greater than 0")
                .When(x => x.WholesalePrice.HasValue);

            RuleFor(x => x.VATRate)
                .InclusiveBetween(0, 100).WithMessage("VAT rate must be between 0 and 100")
                .When(x => x.VATRate.HasValue);

            RuleFor(x => x.Status)
                .Must(status => new[] { "Active", "Discontinued", "OutOfStock" }.Contains(status))
                .WithMessage("Invalid status. Allowed values: Active, Discontinued, OutOfStock")
                .When(x => !string.IsNullOrEmpty(x.Status));

            RuleFor(x => x.UpdateReason)
                .NotEmpty().WithMessage("Update reason is required")
                .MaximumLength(500).WithMessage("Update reason must not exceed 500 characters");

            RuleFor(x => x.UpdatedByUser)
                .NotEmpty().WithMessage("Updated by user is required")
                .MaximumLength(100).WithMessage("Updated by user must not exceed 100 characters");
        }
    }
}

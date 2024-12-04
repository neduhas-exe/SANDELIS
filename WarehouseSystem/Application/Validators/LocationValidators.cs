using FluentValidation;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Validators
{
    public class ProductLocationValidator : InputValidatorBase<ProductLocationDto>
    {
        public ProductLocationValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Warehouse ID is required")
                .MaximumLength(10).WithMessage("Warehouse ID must not exceed 10 characters");

            RuleFor(x => x.Zone)
                .NotEmpty().WithMessage("Zone is required")
                .Matches(@"^[A-Z]$").WithMessage("Zone must be a single uppercase letter");

            RuleFor(x => x.Aisle)
                .NotEmpty().WithMessage("Aisle is required")
                .Matches(@"^\d{2}$").WithMessage("Aisle must be exactly 2 digits");

            RuleFor(x => x.Rack)
                .NotEmpty().WithMessage("Rack is required")
                .Matches(@"^\d{2}$").WithMessage("Rack must be exactly 2 digits");

            RuleFor(x => x.Shelf)
                .NotEmpty().WithMessage("Shelf is required")
                .Matches(@"^\d{2}$").WithMessage("Shelf must be exactly 2 digits");

            RuleFor(x => x.Bin)
                .NotEmpty().WithMessage("Bin is required")
                .Matches(@"^\d{2}$").WithMessage("Bin must be exactly 2 digits");

            RuleFor(x => x.Quantity)
                .GreaterThanOrEqualTo(0).WithMessage("Quantity cannot be negative")
                .LessThanOrEqualTo(x => x.MaxCapacity)
                .WithMessage("Quantity cannot exceed maximum capacity");

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Maximum capacity must be greater than 0");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Unit of measure is required")
                .Must(uom => new[] { "vnt", "m", "kg", "l" }.Contains(uom.ToLower()))
                .WithMessage("Invalid unit of measure. Allowed values: vnt, m, kg, l");

            RuleFor(x => x.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum quantity cannot be negative")
                .LessThanOrEqualTo(x => x.MaxCapacity)
                .WithMessage("Minimum quantity cannot exceed maximum capacity");

            RuleFor(x => x.StorageConditions)
                .MaximumLength(500).WithMessage("Storage conditions must not exceed 500 characters");

            RuleFor(x => x.Temperature)
                .InclusiveBetween(-50, 50).WithMessage("Temperature must be between -50°C and 50°C")
                .When(x => x.Temperature != 0);

            RuleFor(x => x.Humidity)
                .InclusiveBetween(0, 100).WithMessage("Humidity must be between 0% and 100%")
                .When(x => x.Humidity != 0);

            RuleFor(x => x.StockValue)
                .GreaterThanOrEqualTo(0).WithMessage("Stock value cannot be negative");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required")
                .Must(curr => new[] { "EUR", "USD", "GBP" }.Contains(curr))
                .WithMessage("Invalid currency code. Allowed values: EUR, USD, GBP");

            RuleFor(x => x.LastUpdated)
                .NotEmpty().WithMessage("Last updated date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Last updated date cannot be in the future");

            RuleFor(x => x.UpdatedByUser)
                .NotEmpty().WithMessage("Updated by user is required")
                .MaximumLength(100).WithMessage("Updated by user must not exceed 100 characters");

            RuleFor(x => x.LastOperationType)
                .NotEmpty().WithMessage("Last operation type is required")
                .Must(type => new[] { "Receive", "Ship", "Transfer", "Adjust", "Count" }.Contains(type))
                .WithMessage("Invalid operation type. Allowed values: Receive, Ship, Transfer, Adjust, Count");
        }
    }

    public class CreateLocationValidator : InputValidatorBase<CreateLocationDto>
    {
        public CreateLocationValidator()
        {
            RuleFor(x => x.Zone)
                .NotEmpty().WithMessage("Zone is required")
                .Matches(@"^[A-Z]$").WithMessage("Zone must be a single uppercase letter");

            RuleFor(x => x.Aisle)
                .NotEmpty().WithMessage("Aisle is required")
                .Matches(@"^\d{2}$").WithMessage("Aisle must be exactly 2 digits");

            RuleFor(x => x.Rack)
                .NotEmpty().WithMessage("Rack is required")
                .Matches(@"^\d{2}$").WithMessage("Rack must be exactly 2 digits");

            RuleFor(x => x.Shelf)
                .NotEmpty().WithMessage("Shelf is required")
                .Matches(@"^\d{2}$").WithMessage("Shelf must be exactly 2 digits");

            RuleFor(x => x.Bin)
                .NotEmpty().WithMessage("Bin is required")
                .Matches(@"^\d{2}$").WithMessage("Bin must be exactly 2 digits");

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Maximum capacity must be greater than 0");

            RuleFor(x => x.StorageConditions)
                .MaximumLength(500).WithMessage("Storage conditions must not exceed 500 characters");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Unit of measure is required")
                .Must(uom => new[] { "vnt", "m", "kg", "l" }.Contains(uom.ToLower()))
                .WithMessage("Invalid unit of measure. Allowed values: vnt, m, kg, l");

            RuleFor(x => x.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum quantity cannot be negative");
        }
    }

    public class UpdateLocationValidator : InputValidatorBase<UpdateLocationDto>
    {
        public UpdateLocationValidator()
        {
            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Maximum capacity must be greater than 0")
                .When(x => x.MaxCapacity.HasValue);

            RuleFor(x => x.StorageConditions)
                .MaximumLength(500).WithMessage("Storage conditions must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.StorageConditions));

            RuleFor(x => x.MinimumQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Minimum quantity cannot be negative")
                .When(x => x.MinimumQuantity.HasValue);

            RuleFor(x => x.Temperature)
                .InclusiveBetween(-50, 50).WithMessage("Temperature must be between -50°C and 50°C")
                .When(x => x.Temperature.HasValue && x.Temperature != 0);

            RuleFor(x => x.Humidity)
                .InclusiveBetween(0, 100).WithMessage("Humidity must be between 0% and 100%")
                .When(x => x.Humidity.HasValue && x.Humidity != 0);

            RuleFor(x => x.UpdatedByUser)
                .NotEmpty().WithMessage("Updated by user is required")
                .MaximumLength(100).WithMessage("Updated by user must not exceed 100 characters");

            RuleFor(x => x.UpdateReason)
                .NotEmpty().WithMessage("Update reason is required")
                .MaximumLength(500).WithMessage("Update reason must not exceed 500 characters");
        }
    }

    public class LocationStatusHistoryValidator : InputValidatorBase<LocationStatusHistoryDto>
    {
        public LocationStatusHistoryValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Warehouse ID is required")
                .MaximumLength(10).WithMessage("Warehouse ID must not exceed 10 characters");

            RuleFor(x => x.LocationCode)
                .NotEmpty().WithMessage("Location code is required")
                .Matches(@"^[A-Z]-\d{2}-\d{2}-\d{2}-\d{2}$")
                .WithMessage("Invalid location code format. Must be in format: Zone-Aisle-Rack-Shelf-Bin (e.g. A-01-02-03-04)");

            RuleFor(x => x.ChangeType)
                .NotEmpty().WithMessage("Change type is required")
                .Must(type => new[] { "Created", "Updated", "Deleted" }.Contains(type))
                .WithMessage("Invalid change type. Allowed values: Created, Updated, Deleted");

            RuleFor(x => x.NewQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("New quantity cannot be negative")
                .When(x => x.NewQuantity.HasValue);

            RuleFor(x => x.OldQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Old quantity cannot be negative")
                .When(x => x.OldQuantity.HasValue);

            RuleFor(x => x.ChangedByUser)
                .NotEmpty().WithMessage("Changed by user is required")
                .MaximumLength(100).WithMessage("Changed by user must not exceed 100 characters");

            RuleFor(x => x.ChangeReason)
                .NotEmpty().WithMessage("Change reason is required")
                .MaximumLength(500).WithMessage("Change reason must not exceed 500 characters");

            RuleFor(x => x.ChangeDate)
                .NotEmpty().WithMessage("Change date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Change date cannot be in the future");
        }
    }
}

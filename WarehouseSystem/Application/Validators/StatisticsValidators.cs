using FluentValidation;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Validators 
{
    public class LocationUtilizationDtoValidator : InputValidatorBase<LocationUtilizationDto>
    {
        public LocationUtilizationDtoValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Warehouse ID is required")
                .MaximumLength(10).WithMessage("Warehouse ID must not exceed 10 characters");

            RuleFor(x => x.LocationCode)
                .NotEmpty().WithMessage("Location code is required")
                .Matches(@"^[A-Z]-\d{2}-\d{2}-\d{2}-\d{2}$")
                .WithMessage("Invalid location code format. Must be in format: Zone-Aisle-Rack-Shelf-Bin (e.g. A-01-02-03-04)");

            RuleFor(x => x.CurrentQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Current quantity cannot be negative")
                .LessThanOrEqualTo(x => x.MaxCapacity)
                .WithMessage("Current quantity cannot exceed maximum capacity");

            RuleFor(x => x.MaxCapacity)
                .GreaterThan(0).WithMessage("Maximum capacity must be greater than 0");

            RuleFor(x => x.UnitOfMeasure)
                .NotEmpty().WithMessage("Unit of measure is required")
                .Must(uom => new[] { "vnt", "m", "kg", "l" }.Contains(uom.ToLower()))
                .WithMessage("Invalid unit of measure. Allowed values: vnt, m, kg, l");

            RuleFor(x => x.LastUpdated)
                .NotEmpty().WithMessage("Last updated date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Last updated date cannot be in the future");
        }
    }

    public class ZoneStatisticsDtoValidator : InputValidatorBase<ZoneStatisticsDto>
    {
        public ZoneStatisticsDtoValidator()
        {
            RuleFor(x => x.WarehouseId)
                .NotEmpty().WithMessage("Warehouse ID is required")
                .MaximumLength(10).WithMessage("Warehouse ID must not exceed 10 characters");

            RuleFor(x => x.Zone)
                .NotEmpty().WithMessage("Zone is required")
                .Matches(@"^[A-Z]$").WithMessage("Zone must be a single uppercase letter");

            RuleFor(x => x.TotalLocations)
                .GreaterThanOrEqualTo(0).WithMessage("Total locations cannot be negative");

            RuleFor(x => x.EmptyLocations)
                .GreaterThanOrEqualTo(0).WithMessage("Empty locations cannot be negative")
                .LessThanOrEqualTo(x => x.TotalLocations)
                .WithMessage("Empty locations cannot exceed total locations");

            RuleFor(x => x.PartiallyFullLocations)
                .GreaterThanOrEqualTo(0).WithMessage("Partially full locations cannot be negative")
                .LessThanOrEqualTo(x => x.TotalLocations)
                .WithMessage("Partially full locations cannot exceed total locations");

            RuleFor(x => x.FullLocations)
                .GreaterThanOrEqualTo(0).WithMessage("Full locations cannot be negative")
                .LessThanOrEqualTo(x => x.TotalLocations)
                .WithMessage("Full locations cannot exceed total locations");

            RuleFor(x => x)
                .Must(x => x.EmptyLocations + x.PartiallyFullLocations + x.FullLocations == x.TotalLocations)
                .WithMessage("Sum of empty, partially full, and full locations must equal total locations");

            RuleFor(x => x.TotalCapacity)
                .GreaterThan(0).WithMessage("Total capacity must be greater than 0");

            RuleFor(x => x.UsedCapacity)
                .GreaterThanOrEqualTo(0).WithMessage("Used capacity cannot be negative")
                .LessThanOrEqualTo(x => x.TotalCapacity)
                .WithMessage("Used capacity cannot exceed total capacity");

            RuleFor(x => x.LastUpdated)
                .NotEmpty().WithMessage("Last updated date is required")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("Last updated date cannot be in the future");
        }
    }
}

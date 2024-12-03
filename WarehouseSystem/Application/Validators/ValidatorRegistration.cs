using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Application.Validators
{
    public static class ValidatorRegistration
    {
        public static IServiceCollection AddValidators(this IServiceCollection services)
        {
            // Product validators
            services.AddScoped<IValidator<CreateProductDto>, CreateProductDtoValidator>();
            services.AddScoped<IValidator<UpdateProductDto>, UpdateProductDtoValidator>();
            services.AddScoped<IValidator<ProductDto>, ProductDtoValidator>();

            // QR code validators
            services.AddScoped<IValidator<AddProductQRCodeDto>, AddProductQRCodeDtoValidator>();
            services.AddScoped<IValidator<UpdateQRCodeStatusDto>, UpdateQRCodeStatusDtoValidator>();
            services.AddScoped<IValidator<ProductQRCodeDto>, ProductQRCodeDtoValidator>();

            // Location validators
            services.AddScoped<IValidator<CreateLocationDto>, CreateLocationDtoValidator>();
            services.AddScoped<IValidator<UpdateLocationDto>, UpdateLocationDtoValidator>();
            services.AddScoped<IValidator<ProductLocationDto>, ProductLocationDtoValidator>();

            // Movement validators
            services.AddScoped<IValidator<ProductMovementDto>, ProductMovementDtoValidator>();
            services.AddScoped<IValidator<ProductMovementRegistrationDto>, ProductMovementRegistrationDtoValidator>();
            services.AddScoped<IValidator<MovementCancellationDto>, MovementCancellationDtoValidator>();
            services.AddScoped<IValidator<MovementQuantityAdjustmentDto>, MovementQuantityAdjustmentDtoValidator>();

            // Statistics validators
            services.AddScoped<IValidator<LocationUtilizationDto>, LocationUtilizationDtoValidator>();
            services.AddScoped<IValidator<ZoneStatisticsDto>, ZoneStatisticsDtoValidator>();
            services.AddScoped<IValidator<LocationStatusHistoryDto>, LocationStatusHistoryDtoValidator>();

            // Stock validators
            services.AddScoped<IValidator<StockCountDto>, StockCountDtoValidator>();
            services.AddScoped<IValidator<StockAdjustmentDto>, StockAdjustmentDtoValidator>();

            return services;
        }
    }
}

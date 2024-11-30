using Application.Services;
using Application.Services.Interfaces;
using Infrastructure.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void BootstrapApplication(this IServiceCollection services)
        {
            services.AddTransient<IProductsService, ProductsService>();
            services.AddInfrastructure();
        }
    }
}

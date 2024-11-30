using Infrastructure.Repositories;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services)
        {
            services.AddTransient<IProductsRepository, ProductsRepository>();
        }
    }
}

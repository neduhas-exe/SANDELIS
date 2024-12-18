using Infrastructure.Repositories;
using Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;
//NOTE: Register repositories here.
public static class ServiceCollectionExtensions
{
    public static void AddInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IProductsRepository, ProductsRepository>();
        services.AddSingleton<CsvFileService>();  // Singleton because we want to maintain _lastId
        services.AddTransient<ISiteRepository, CsvSiteRepository>();
    }
}

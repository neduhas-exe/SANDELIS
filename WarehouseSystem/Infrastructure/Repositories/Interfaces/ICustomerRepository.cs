// Path: WarehouseSystem/Infrastructure/Repositories/Interfaces/ICustomerRepository.cs
namespace Infrastructure.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Customer Get(long id);
        List<Customer> List();
        Customer Create(Customer customer);
    }
}

// Path: WarehouseSystem/Infrastructure/Repositories/Interfaces/ISiteRepository.cs
namespace Infrastructure.Repositories.Interfaces
{
    public interface ISiteRepository
    {
        Site Get(long id);
        List<Site> List();
        Site Create(Site site);
    }
}

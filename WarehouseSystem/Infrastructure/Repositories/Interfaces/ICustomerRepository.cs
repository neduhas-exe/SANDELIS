// Path: WarehouseSystem/Infrastructure/Repositories/Interfaces/ICustomerRepository.cs
using Domain.Models;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Customer Get(long id);
        List<Customer> List();
        Customer Create(Customer customer);
    }
}


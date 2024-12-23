using Domain.Models;

namespace Infrastructure.Repositories.Interfaces;

public interface ICustomersRepository
{
    public Customer Get(long id);
    public List<Customer> List();
    public Customer Create(Customer customer);
}
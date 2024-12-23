using Domain.Models;

namespace Application.Services.Interfaces;

public interface ICustomersService
{
    public Customer Get(long id);
    public List<Customer> List();
    public Customer Create(Customer customer);
}

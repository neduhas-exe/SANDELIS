using Application.Services.Interfaces;
using Domain.Models;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services;

public class CustomersService(ICustomersRepository customersRepository) : ICustomersService
{
    private readonly ICustomersRepository _customersRepository = customersRepository;

    public Customer Get(long id) => _customersRepository.Get(id);

    public List<Customer> List() => _customersRepository.List();

    public Customer Create(Customer customer) => _customersRepository.Create(customer);
}

// Path: WarehouseSystem/Application/Services/CustomersService.cs
namespace Application.Services
{
    public class CustomersService : ICustomersService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomersService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public Customer Get(long id)
        {
            return _customerRepository.Get(id);
        }

        public List<Customer> List()
        {
            return _customerRepository.List();
        }

        public Customer Create(Customer customer)
        {
            return _customerRepository.Create(customer);
        }
    }
}

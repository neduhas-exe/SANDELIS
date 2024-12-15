// Application/Services/Interfaces/ICustomersService.cs
public interface ICustomersService
{
    Customer Get(long id);
    List<Customer> List();
    Customer Create(Customer customer);
}

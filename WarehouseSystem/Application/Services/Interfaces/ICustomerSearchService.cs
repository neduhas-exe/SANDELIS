using Domain.Models;

namespace Application.Services.Interfaces;

public interface ICustomerSearchService
{
    public List<Customer> Search(string searchTerm);
    public List<Customer> SearchByField(string fieldName, string searchTerm);
}
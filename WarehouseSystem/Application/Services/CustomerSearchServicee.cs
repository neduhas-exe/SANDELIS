using Domain.Models;
using Application.Services.Interfaces;

namespace Application.Services;

public class CustomerSearchService : ICustomerSearchService
{
    private readonly ICustomersService _customersService;

    public CustomerSearchService(ICustomersService customersService)
    {
        _customersService = customersService;
    }

    public List<Customer> Search(string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return _customersService.List();

        searchTerm = searchTerm.ToLower();
        return _customersService.List()
            .Where(c =>
                (c.Name?.ToLower().Contains(searchTerm) ?? false) ||
                (c.LastName?.ToLower().Contains(searchTerm) ?? false) ||
                (c.Email?.ToLower().Contains(searchTerm) ?? false) ||
                (c.Phone?.ToLower().Contains(searchTerm) ?? false) ||
                (c.Address?.ToLower().Contains(searchTerm) ?? false) ||
                (c.Barcode?.ToLower().Contains(searchTerm) ?? false)
            ).ToList();
    }

    public List<Customer> SearchByField(string fieldName, string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return _customersService.List();

        searchTerm = searchTerm.ToLower();
        var customers = _customersService.List();

        return fieldName.ToLower() switch
        {
            "name" => customers.Where(c => c.Name?.ToLower().Contains(searchTerm) ?? false).ToList(),
            "lastname" => customers.Where(c => c.LastName?.ToLower().Contains(searchTerm) ?? false).ToList(),
            "email" => customers.Where(c => c.Email?.ToLower().Contains(searchTerm) ?? false).ToList(),
            "phone" => customers.Where(c => c.Phone?.ToLower().Contains(searchTerm) ?? false).ToList(),
            "address" => customers.Where(c => c.Address?.ToLower().Contains(searchTerm) ?? false).ToList(),
            "barcode" => customers.Where(c => c.Barcode?.ToLower().Contains(searchTerm) ?? false).ToList(),
            _ => new List<Customer>()
        };
    }
}
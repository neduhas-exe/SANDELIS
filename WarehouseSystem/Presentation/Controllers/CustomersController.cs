using Application.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController : Controller
{
    private readonly ICustomersService _customersService;
    private readonly ICustomerSearchService _searchService;

    public CustomersController(ICustomersService customersService, ICustomerSearchService searchService)
    {
        _customersService = customersService;
        _searchService = searchService;
    }

    [HttpGet("{id}")]
    public IActionResult Get(long id)
    {
        var customer = _customersService.Get(id);
        return Ok(customer);
    }

    [HttpGet()]
    public IActionResult List()
    {
        var customers = _customersService.List();
        return Ok(customers);
    }

    [HttpPost()]
    public IActionResult Create(Customer customer)
    {
        var newCustomer = _customersService.Create(customer);
        return Ok(newCustomer);
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string searchTerm)
    {
        var results = _searchService.Search(searchTerm);
        return Ok(results);
    }

    [HttpGet("search/{fieldName}")]
    public IActionResult SearchByField(string fieldName, [FromQuery] string searchTerm)
    {
        var results = _searchService.SearchByField(fieldName, searchTerm);
        return Ok(results);
    }
}
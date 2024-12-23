using Application.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

[ApiController]
[Route("customers")]
public class CustomersController(ICustomersService customersService) : Controller
{
    private readonly ICustomersService _customersService = customersService;

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
}

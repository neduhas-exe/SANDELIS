// Path: WarehouseSystem/Presentation/Controllers/CustomersController.cs

using Application.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    /// <summary>
    /// Kontroleris skirtas klientų (Customer) valdymui
    /// </summary>
    [ApiController]  // Žymi, kad tai yra API kontroleris
    [Route("customers")]  // API endpoint pradžia, pvz: /customers
    public class CustomersController : Controller
    {
        private readonly ICustomersService _customersService;

        /// <summary>
        /// Konstruktorius su dependency injection
        /// </summary>
        /// <param name="customersService">Klientų serviso implementacija</param>
        public CustomersController(ICustomersService customersService)
        {
            _customersService = customersService;
        }

        /// <summary>
        /// Gauti konkretų klientą pagal ID
        /// </summary>
        /// <param name="id">Kliento ID</param>
        /// <returns>Kliento informacija</returns>
        [HttpGet("{id}")]  // GET /customers/{id}
        public IActionResult Get(long id)
        {
            var customer = _customersService.Get(id);
            return Ok(customer);  // Grąžina 200 OK su kliento duomenimis
        }

        /// <summary>
        /// Gauti visų klientų sąrašą
        /// </summary>
        /// <returns>Klientų sąrašas</returns>
        [HttpGet]  // GET /customers
        public IActionResult List()
        {
            var customers = _customersService.List();
            return Ok(customers);  // Grąžina 200 OK su klientų sąrašu
        }

        /// <summary>
        /// Sukurti naują klientą
        /// </summary>
        /// <param name="customer">Naujo kliento duomenys</param>
        /// <returns>Sukurto kliento informacija</returns>
        [HttpPost]  // POST /customers
        public IActionResult Create(Customer customer)
        {
            var newCustomer = _customersService.Create(customer);
            return Ok(newCustomer);  // Grąžina 200 OK su sukurto kliento duomenimis
        }
    }
}

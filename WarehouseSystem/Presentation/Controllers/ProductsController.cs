using Application.Services.Interfaces;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    //Temporary controller used to test functionality until front end is developed.
    [ApiController]
    [Route("products")]
    public class ProductsController(IProductsService productsService) : Controller
    {
        private readonly IProductsService _productsService = productsService;

        [HttpGet("{id}")]
        public IActionResult Get(long id)
        {
            var product = _productsService.Get(id);
            return Ok(product);
        }

        [HttpGet()]
        public IActionResult List()
        {
            var products = _productsService.List();
            return Ok(products);
        }

        [HttpPost()]
        public IActionResult Create(Product product)
        {
            var newProduct = _productsService.Create(product);
            return Ok(newProduct);
        }
    }
}

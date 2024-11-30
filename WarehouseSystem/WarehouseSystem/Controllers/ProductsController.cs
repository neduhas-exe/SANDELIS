using Application.Services.Interfaces;
using Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;

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
            return Ok(product.Adapt<ProductDto>());
        }

        [HttpGet()]
        public IActionResult List()
        {
            var products = _productsService.List();
            return Ok(products.Adapt<List<ProductDto>>());
        }

        [HttpPost()]
        public IActionResult Create(ProductDto productDto)
        {
            var product = _productsService.Create(productDto.Adapt<Product>());
            return Ok(product);
        }
    }
}

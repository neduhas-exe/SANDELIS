using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using CsvHelper;
using System.Globalization;
using Domain.Models;
using System.Linq;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly string _csvFilePath = "Data/products.csv";

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            using var reader = new StreamReader(_csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var products = csv.GetRecords<Product>().ToList();
            return Ok(products);
        }
        catch (FileNotFoundException)
        {
            return NotFound("CSV file not found");
        }
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        try
        {
            using var reader = new StreamReader(_csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var product = csv.GetRecords<Product>()
                            .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound($"Product with ID {id} not found");

            return Ok(product);
        }
        catch (FileNotFoundException)
        {
            return NotFound("CSV file not found");
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] Product product)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var products = new List<Product>();

            // Read existing products
            if (System.IO.File.Exists(_csvFilePath))
            {
                using (var reader = new StreamReader(_csvFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    products = csv.GetRecords<Product>().ToList();
                }
            }

            // Set new ID
            product.Id = products.Any() ? products.Max(p => p.Id) + 1 : 1;
            products.Add(product);

            // Write all products back to CSV
            using (var writer = new StreamWriter(_csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(products);
            }

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Product product)
    {
        if (id != product.Id)
            return BadRequest("ID mismatch");

        try
        {
            var products = new List<Product>();
            using (var reader = new StreamReader(_csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                products = csv.GetRecords<Product>().ToList();
            }

            var existingProduct = products.FirstOrDefault(p => p.Id == id);
            if (existingProduct == null)
                return NotFound($"Product with ID {id} not found");

            products.Remove(existingProduct);
            products.Add(product);
            products = products.OrderBy(p => p.Id).ToList();

            using (var writer = new StreamWriter(_csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(products);
            }

            return NoContent();
        }
        catch (FileNotFoundException)
        {
            return NotFound("CSV file not found");
        }
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        try
        {
            var products = new List<Product>();
            using (var reader = new StreamReader(_csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                products = csv.GetRecords<Product>().ToList();
            }

            var productToDelete = products.FirstOrDefault(p => p.Id == id);
            if (productToDelete == null)
                return NotFound($"Product with ID {id} not found");

            products.Remove(productToDelete);

            using (var writer = new StreamWriter(_csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(products);
            }

            return NoContent();
        }
        catch (FileNotFoundException)
        {
            return NotFound("CSV file not found");
        }
    }

    [HttpGet("search")]
    public IActionResult Search([FromQuery] string searchTerm)
    {
        try
        {
            using var reader = new StreamReader(_csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var products = csv.GetRecords<Product>()
                            .Where(p => p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();

            return Ok(products);
        }
        catch (FileNotFoundException)
        {
            return NotFound("CSV file not found");
        }
    }
}
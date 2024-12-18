using CsvHelper;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;

[ApiController]
[Route("api/[controller]")]
public class CustomersController : ControllerBase
{
    private readonly string _csvFilePath = "Data/customers.csv";

    [HttpGet]
    public IActionResult GetAll()
    {
        try
        {
            using var reader = new StreamReader(_csvFilePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var customers = csv.GetRecords<Customer>().ToList();
            return Ok(customers);
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
            var customer = csv.GetRecords<Customer>()
                            .FirstOrDefault(c => c.Id == id);

            if (customer == null)
                return NotFound($"Customer with ID {id} not found");

            return Ok(customer);
        }
        catch (FileNotFoundException)
        {
            return NotFound("CSV file not found");
        }
    }

    [HttpPost]
    public IActionResult Create([FromBody] Customer customer)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var customers = new List<Customer>();

            if (System.IO.File.Exists(_csvFilePath))
            {
                using (var reader = new StreamReader(_csvFilePath))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    customers = csv.GetRecords<Customer>().ToList();
                }
            }

            customer.Id = customers.Any() ? customers.Max(c => c.Id) + 1 : 1;
            customers.Add(customer);

            using (var writer = new StreamWriter(_csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(customers);
            }

            return CreatedAtAction(nameof(GetById), new { id = customer.Id }, customer);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Internal server error: {ex.Message}");
        }
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] Customer customer)
    {
        if (id != customer.Id)
            return BadRequest("ID mismatch");

        try
        {
            var customers = new List<Customer>();
            using (var reader = new StreamReader(_csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                customers = csv.GetRecords<Customer>().ToList();
            }

            var existingCustomer = customers.FirstOrDefault(c => c.Id == id);
            if (existingCustomer == null)
                return NotFound($"Customer with ID {id} not found");

            customers.Remove(existingCustomer);
            customers.Add(customer);
            customers = customers.OrderBy(c => c.Id).ToList();

            using (var writer = new StreamWriter(_csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(customers);
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
            var customers = new List<Customer>();
            using (var reader = new StreamReader(_csvFilePath))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                customers = csv.GetRecords<Customer>().ToList();
            }

            var customerToDelete = customers.FirstOrDefault(c => c.Id == id);
            if (customerToDelete == null)
                return NotFound($"Customer with ID {id} not found");

            customers.Remove(customerToDelete);

            using (var writer = new StreamWriter(_csvFilePath))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecords(customers);
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
            var customers = csv.GetRecords<Customer>()
                            .Where(c => c.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase))
                            .ToList();

            return Ok(customers);
        }
        catch (FileNotFoundException)
        {
            return NotFound("CSV file not found");
        }
    }
}
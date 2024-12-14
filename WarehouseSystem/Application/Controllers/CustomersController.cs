using Microsoft.AspNetCore.Mvc;
using WarehouseSystem.Application.DTOs;
using WarehouseSystem.Application.Services.Interfaces;
using Mapster;
using System.Text;

namespace WarehouseSystem.Application.Controllers
{
    /// <summary>
    /// Klientų valdymo kontroleris
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly ICustomersService _customersService;
        private readonly ILogger<CustomersController> _logger;

        /// <summary>
        /// Kontrolerio konstruktorius
        /// </summary>
        public CustomersController(ICustomersService customersService, 
                                 ILogger<CustomersController> logger)
        {
            _customersService = customersService;
            _logger = logger;
        }

        /// <summary>
        /// Gauti visus klientus
        /// </summary>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("Gaunamas visų klientų sąrašas");
                var customers = await _customersService.GetAllAsync();
                return Ok(customers.Adapt<List<CustomerDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant klientų sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Gauti klientą pagal ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                _logger.LogInformation("Gaunamas klientas {CustomerId}", id);
                var customer = await _customersService.GetAsync(id);
                
                if (customer == null)
                {
                    return NotFound($"Klientas {id} nerastas");
                }

                return Ok(customer.Adapt<CustomerDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant klientą {CustomerId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Sukurti naują klientą
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CustomerDto customerDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Papildoma validacija įmonėms
                if (customerDto.CustomerType == "Company")
                {
                    if (string.IsNullOrEmpty(customerDto.CompanyName))
                    {
                        return BadRequest("Įmonės pavadinimas yra privalomas įmonės tipo klientams");
                    }
                    if (string.IsNullOrEmpty(customerDto.CompanyCode))
                    {
                        return BadRequest("Įmonės kodas yra privalomas įmonės tipo klientams");
                    }
                }
                // Papildoma validacija privatiems asmenims
                else if (customerDto.CustomerType == "Private")
                {
                    if (string.IsNullOrEmpty(customerDto.FirstName) || string.IsNullOrEmpty(customerDto.LastName))
                    {
                        return BadRequest("Vardas ir pavardė yra privalomi privataus asmens tipo klientams");
                    }
                }

                _logger.LogInformation("Kuriamas naujas klientas {CustomerName}", 
                    customerDto.CustomerType == "Company" ? customerDto.CompanyName : $"{customerDto.FirstName} {customerDto.LastName}");

                var createdCustomer = await _customersService.CreateAsync(customerDto.Adapt<Customer>());
                return CreatedAtAction(nameof(Get), new { id = createdCustomer.Id }, createdCustomer.Adapt<CustomerDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant klientą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Atnaujinti kliento informaciją
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(long id, [FromBody] CustomerDto customerDto)
        {
            try
            {
                if (id != customerDto.Id)
                {
                    return BadRequest("Nesutampa kliento ID");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Papildoma validacija kaip ir Create metode
                if (customerDto.CustomerType == "Company")
                {
                    if (string.IsNullOrEmpty(customerDto.CompanyName))
                    {
                        return BadRequest("Įmonės pavadinimas yra privalomas įmonės tipo klientams");
                    }
                    if (string.IsNullOrEmpty(customerDto.CompanyCode))
                    {
                        return BadRequest("Įmonės kodas yra privalomas įmonės tipo klientams");
                    }
                }
                else if (customerDto.CustomerType == "Private")
                {
                    if (string.IsNullOrEmpty(customerDto.FirstName) || string.IsNullOrEmpty(customerDto.LastName))
                    {
                        return BadRequest("Vardas ir pavardė yra privalomi privataus asmens tipo klientams");
                    }
                }

                _logger.LogInformation("Atnaujinamas klientas {CustomerId}", id);
                
                var existingCustomer = await _customersService.GetAsync(id);
                if (existingCustomer == null)
                {
                    return NotFound($"Klientas {id} nerastas");
                }

                var updatedCustomer = await _customersService.UpdateAsync(customerDto.Adapt<Customer>());
                return Ok(updatedCustomer.Adapt<CustomerDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant klientą {CustomerId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Ištrinti klientą
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                _logger.LogInformation("Trinamas klientas {CustomerId}", id);
                
                var existingCustomer = await _customersService.GetAsync(id);
                if (existingCustomer == null)
                {
                    return NotFound($"Klientas {id} nerastas");
                }

                await _customersService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant klientą {CustomerId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Gauti įmonių tipo klientus
        /// </summary>
        [HttpGet("companies")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCompanies()
        {
            try
            {
                _logger.LogInformation("Gaunamas įmonių sąrašas");
                var companies = await _customersService.GetCompanieAsync();
                return Ok(companies.Adapt<List<CustomerDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant įmonių sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Gauti privačių asmenų tipo klientus
        /// </summary>
        [HttpGet("private")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPrivateCustomers()
        {
            try
            {
                _logger.LogInformation("Gaunamas privačių klientų sąrašas");
                var privateCustomers = await _customersService.GetPrivateCustomersAsync();
                return Ok(privateCustomers.Adapt<List<CustomerDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant privačių klientų sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Gauti aktyvius klientus
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveCustomers()
        {
            try
            {
                _logger.LogInformation("Gaunamas aktyvių klientų sąrašas");
                var activeCustomers = await _customersService.GetActiveCustomersAsync();
                return Ok(activeCustomers.Adapt<List<CustomerDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant aktyvių klientų sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Eksportuoti klientų sąrašą į CSV
        /// </summary>
        [HttpGet("export/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportToCsv()
        {
            try
            {
                _logger.LogInformation("Eksportuojamas klientų sąrašas į CSV");
                var customers = await _customersService.GetAllAsync();
                
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("CustomerID,CustomerType,CompanyName,CompanyCode,VATCode," +
                                    "FirstName,LastName,Email,Phone,Address,City,PostalCode," +
                                    "IsGuest,CustomerStatus,DiscountLevel,CreatedBy,CreatedDate," +
                                    "ModifiedBy,ModifiedDate");

                foreach (var customer in customers)
                {
                    csvBuilder.AppendLine(
                        $"{customer.Id},{customer.CustomerType}," +
                        $"{EscapeCsvField(customer.CompanyName)},{customer.CompanyCode}," +
                        $"{customer.VATCode},{EscapeCsvField(customer.FirstName)}," +
                        $"{EscapeCsvField(customer.LastName)},{customer.Email}," +
                        $"{customer.Phone},{EscapeCsvField(customer.Address)}," +
                        $"{EscapeCsvField(customer.City)},{customer.PostalCode}," +
                        $"{customer.IsGuest},{customer.CustomerStatus}," +
                        $"{customer.DiscountLevel},{customer.CreatedBy}," +
                        $"{customer.CreatedDate:yyyy-MM-dd},{customer.ModifiedBy}," +
                        $"{customer.ModifiedDate:yyyy-MM-dd}");
                }

                var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                return File(bytes, "text/csv", $"klientai_{DateTime.Now:yyyy-MM-dd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida eksportuojant klientus į CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// CSV lauko apsauga
        /// </summary>
        private static string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            if (field.Contains(",") || field.Contains("\"") || field.Contains("\n"))
            {
                field = field.Replace("\"", "\"\"");
                field = $"\"{field}\"";
            }
            return field;
        }
    }
}

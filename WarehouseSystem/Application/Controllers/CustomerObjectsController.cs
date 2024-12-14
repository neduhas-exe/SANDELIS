using Microsoft.AspNetCore.Mvc;
using WarehouseSystem.Application.DTOs;
using WarehouseSystem.Application.Services.Interfaces;
using Mapster;

namespace WarehouseSystem.Application.Controllers
{
    /// <summary>
    /// Klientų objektų valdymo kontroleris
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CustomerObjectsController : ControllerBase
    {
        private readonly ICustomerObjectsService _customerObjectsService;
        private readonly ILogger<CustomerObjectsController> _logger;

        /// <summary>
        /// Kontrolerio konstruktorius
        /// </summary>
        public CustomerObjectsController(ICustomerObjectsService customerObjectsService, 
                                      ILogger<CustomerObjectsController> logger)
        {
            _customerObjectsService = customerObjectsService;
            _logger = logger;
        }

        /// <summary>
        /// Gauti visus kliento objektus
        /// </summary>
        /// <param name="customerId">Kliento ID</param>
        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCustomerObjects(long customerId)
        {
            try
            {
                _logger.LogInformation("Gaunami kliento {CustomerId} objektai", customerId);
                var objects = await _customerObjectsService.GetCustomerObjectsAsync(customerId);
                
                if (objects == null || !objects.Any())
                {
                    return NotFound($"Klientui {customerId} objektų nerasta");
                }

                return Ok(objects.Adapt<List<CustomerObjectDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant kliento {CustomerId} objektus", customerId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Gauti konkretų objektą pagal ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetObject(long id)
        {
            try
            {
                _logger.LogInformation("Gaunamas objektas {ObjectId}", id);
                var obj = await _customerObjectsService.GetObjectAsync(id);
                
                if (obj == null)
                {
                    return NotFound($"Objektas {id} nerastas");
                }

                return Ok(obj.Adapt<CustomerObjectDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant objektą {ObjectId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Sukurti naują objektą
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateObject([FromBody] CustomerObjectDto objectDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Papildoma validacija
                if (!objectDto.IsValidObjectType())
                {
                    return BadRequest("Neteisingas objekto tipas");
                }
                if (!objectDto.IsValidStatus())
                {
                    return BadRequest("Neteisingas objekto statusas");
                }
                if (!objectDto.IsValidProjectPhase())
                {
                    return BadRequest("Neteisinga projekto fazė");
                }

                _logger.LogInformation("Kuriamas naujas objektas {ObjectName}", objectDto.ObjectName);
                var createdObject = await _customerObjectsService.CreateObjectAsync(objectDto.Adapt<CustomerObject>());
                
                return CreatedAtAction(nameof(GetObject), 
                                     new { id = createdObject.Id }, 
                                     createdObject.Adapt<CustomerObjectDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant objektą {ObjectName}", objectDto.ObjectName);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Atnaujinti objekto informaciją
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateObject(long id, [FromBody] CustomerObjectDto objectDto)
        {
            try
            {
                if (id != objectDto.Id)
                {
                    return BadRequest("Nesutampa objekto ID");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Papildoma validacija
                if (!objectDto.IsValidObjectType())
                {
                    return BadRequest("Neteisingas objekto tipas");
                }
                if (!objectDto.IsValidStatus())
                {
                    return BadRequest("Neteisingas objekto statusas");
                }
                if (!objectDto.IsValidProjectPhase())
                {
                    return BadRequest("Neteisinga projekto fazė");
                }

                _logger.LogInformation("Atnaujinamas objektas {ObjectId}", id);
                
                var existingObject = await _customerObjectsService.GetObjectAsync(id);
                if (existingObject == null)
                {
                    return NotFound($"Objektas {id} nerastas");
                }

                var updatedObject = await _customerObjectsService.UpdateObjectAsync(objectDto.Adapt<CustomerObject>());
                return Ok(updatedObject.Adapt<CustomerObjectDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant objektą {ObjectId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Ištrinti objektą
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteObject(long id)
        {
            try
            {
                _logger.LogInformation("Trinamas objektas {ObjectId}", id);
                
                var existingObject = await _customerObjectsService.GetObjectAsync(id);
                if (existingObject == null)
                {
                    return NotFound($"Objektas {id} nerastas");
                }

                await _customerObjectsService.DeleteObjectAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant objektą {ObjectId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Gauti objektus pagal tipą
        /// </summary>
        [HttpGet("type/{objectType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetObjectsByType(string objectType)
        {
            try
            {
                _logger.LogInformation("Gaunami {ObjectType} tipo objektai", objectType);
                var objects = await _customerObjectsService.GetObjectsByTypeAsync(objectType);
                return Ok(objects.Adapt<List<CustomerObjectDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant {ObjectType} tipo objektus", objectType);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Gauti objektus pagal projekto fazę
        /// </summary>
        [HttpGet("phase/{phase}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetObjectsByPhase(string phase)
        {
            try
            {
                _logger.LogInformation("Gaunami objektai {Phase} fazėje", phase);
                var objects = await _customerObjectsService.GetObjectsByPhaseAsync(phase);
                return Ok(objects.Adapt<List<CustomerObjectDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant objektus {Phase} fazėje", phase);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Gauti aktyvius objektus
        /// </summary>
        [HttpGet("active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActiveObjects()
        {
            try
            {
                _logger.LogInformation("Gaunami aktyvūs objektai");
                var objects = await _customerObjectsService.GetActiveObjectsAsync();
                return Ok(objects.Adapt<List<CustomerObjectDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant aktyvius objektus");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Eksportuoti objektų sąrašą į CSV
        /// </summary>
        [HttpGet("export/csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ExportToCsv()
        {
            try
            {
                _logger.LogInformation("Eksportuojamas objektų sąrašas į CSV");
                var objects = await _customerObjectsService.GetAllObjectsAsync();
                
                var csvBuilder = new StringBuilder();
                csvBuilder.AppendLine("ObjectID,CustomerID,ObjectName,ObjectType,Address,City,PostalCode," +
                                    "ContactPerson,ContactPhone,Status,ProjectPhase,CreatedBy,CreatedDate," +
                                    "ModifiedBy,ModifiedDate");

                foreach (var obj in objects)
                {
                    csvBuilder.AppendLine(
                        $"{obj.Id},{obj.CustomerID},{EscapeCsvField(obj.ObjectName)}," +
                        $"{obj.ObjectType},{EscapeCsvField(obj.Address)},{EscapeCsvField(obj.City)}," +
                        $"{obj.PostalCode},{EscapeCsvField(obj.ContactPerson)},{obj.ContactPhone}," +
                        $"{obj.Status},{obj.ProjectPhase},{obj.CreatedBy}," +
                        $"{obj.CreatedDate:yyyy-MM-dd},{obj.ModifiedBy},{obj.ModifiedDate:yyyy-MM-dd}");
                }

                var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                return File(bytes, "text/csv", $"objektai_{DateTime.Now:yyyy-MM-dd}.csv");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida eksportuojant objektus į CSV");
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

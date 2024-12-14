using Application.Services.Interfaces;
using Domain.Models;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using Presentation.DTOs;
using System.Text;

namespace WarehouseSystem.Application.Controllers
{
    /// <summary>
    /// Produktų valdymo kontroleris
    /// Atsakingas už visas produktų CRUD operacijas ir papildomas funkcijas
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductsService _productsService;
        private readonly ILogger<ProductsController> _logger;

        /// <summary>
        /// Kontrolerio konstruktorius
        /// </summary>
        /// <param name="productsService">Produktų serviso interfeisas</param>
        /// <param name="logger">Loginimo servisas</param>
        public ProductsController(IProductsService productsService, ILogger<ProductsController> logger)
        {
            _productsService = productsService;
            _logger = logger;
        }

        /// <summary>
        /// Gauti produktą pagal ID
        /// </summary>
        /// <param name="id">Produkto identifikatorius</param>
        /// <returns>Produkto informacija</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                _logger.LogInformation("Gaunamas produktas su ID: {Id}", id);
                var product = await _productsService.GetAsync(id);

                if (product == null)
                {
                    _logger.LogWarning("Produktas su ID {Id} nerastas", id);
                    return NotFound($"Produktas su ID {id} nerastas");
                }

                return Ok(product.Adapt<ProductDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produktą su ID {Id}", id);
                return StatusCode(500, "Vidinė serverio klaida gaunant produktą");
            }
        }

        /// <summary>
        /// Gauti visų produktų sąrašą
        /// </summary>
        /// <returns>Produktų sąrašas</returns>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> List()
        {
            try
            {
                _logger.LogInformation("Gaunamas visų produktų sąrašas");
                var products = await _productsService.ListAsync();
                return Ok(products.Adapt<List<ProductDto>>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produktų sąrašą");
                return StatusCode(500, "Vidinė serverio klaida gaunant produktų sąrašą");
            }
        }

        /// <summary>
        /// Sukurti naują produktą
        /// </summary>
        /// <param name="productDto">Naujo produkto informacija</param>
        /// <returns>Sukurto produkto informacija</returns>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Create([FromBody] ProductDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Kuriamas naujas produktas: {ProductName}", productDto.Name);
                
                var product = await _productsService.CreateAsync(productDto.Adapt<Product>());
                var createdProduct = product.Adapt<ProductDto>();
                
                return CreatedAtAction(nameof(Get), new { id = createdProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant produktą: {ProductName}", productDto.Name);
                return StatusCode(500, "Vidinė serverio klaida kuriant produktą");
            }
        }

        /// <summary>
        /// Atnaujinti produkto informaciją
        /// </summary>
        /// <param name="id">Produkto identifikatorius</param>
        /// <param name="productDto">Atnaujinta produkto informacija</param>
        /// <returns>Atnaujinto produkto informacija</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ProductDto))]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Update(long id, [FromBody] ProductDto productDto)
        {
            try
            {
                if (id != productDto.Id)
                {
                    return BadRequest("Nesutampa produkto ID");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                _logger.LogInformation("Atnaujinamas produktas su ID: {Id}", id);
                
                var existingProduct = await _productsService.GetAsync(id);
                if (existingProduct == null)
                {
                    return NotFound($"Produktas su ID {id} nerastas");
                }

                var updatedProduct = await _productsService.UpdateAsync(productDto.Adapt<Product>());
                return Ok(updatedProduct.Adapt<ProductDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant produktą su ID {Id}", id);
                return StatusCode(500, "Vidinė serverio klaida atnaujinant produktą");
            }
        }

        /// <summary>
        /// Ištrinti produktą
        /// </summary>
        /// <param name="id">Produkto identifikatorius</param>
        /// <returns>Operacijos rezultatas</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                _logger.LogInformation("Trinamas produktas su ID: {Id}", id);
                
                var existingProduct = await _productsService.GetAsync(id);
                if (existingProduct == null)
                {
                    return NotFound($"Produktas su ID {id} nerastas");
                }

                await _productsService.DeleteAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant produktą su ID {Id}", id);
                return StatusCode(500, "Vidinė serverio klaida trinant produktą");
            }
        }

        /// <summary>
        /// Eksportuoti produktų sąrašą į CSV
        /// </summary>
        /// <returns>CSV failas</returns>
        [HttpGet("export-csv")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ExportToCsv()
        {
            try
            {
                _logger.LogInformation("Eksportuojamas produktų sąrašas į CSV");
                
                var products = await _productsService.ListAsync();
                var csvBuilder = new StringBuilder();

                // CSV antraštės
                csvBuilder.AppendLine("ProductID,LegacyCode,Name,Barcode,QRCode,Description,Category,SubCategory," +
                                   "PurchasePriceExVAT,SalePriceExVAT,VATRate,LastInvoiceNumber,LastPurchaseDate," +
                                   "LastPurchaseSupplier,LastReceivedBy,QuantityInStock,MinimumStockLevel,LastRestockDate," +
                                   "SupplierID,CreatedBy,CreatedDate,ModifiedBy,ModifiedDate,Status");

                // Produktų duomenys
                foreach (var product in products)
                {
                    var productDto = product.Adapt<ProductDto>();
                    csvBuilder.AppendLine(
                        $"{productDto.Id},{productDto.LegacyCode},{EscapeCsvField(productDto.Name)}," +
                        $"{productDto.Barcode},{productDto.QRCode},{EscapeCsvField(productDto.Description)}," +
                        $"{EscapeCsvField(productDto.Category)},{EscapeCsvField(productDto.SubCategory)}," +
                        $"{productDto.PurchasePriceExVAT},{productDto.SalePriceExVAT},{productDto.VATRate}," +
                        $"{productDto.LastInvoiceNumber},{productDto.LastPurchaseDate:yyyy-MM-dd}," +
                        $"{EscapeCsvField(productDto.LastPurchaseSupplier)},{EscapeCsvField(productDto.LastReceivedBy)}," +
                        $"{productDto.QuantityInStock},{productDto.MinimumStockLevel}," +
                        $"{productDto.LastRestockDate:yyyy-MM-dd},{productDto.SupplierID}," +
                        $"{productDto.CreatedBy},{productDto.CreatedDate:yyyy-MM-dd}," +
                        $"{productDto.ModifiedBy},{productDto.ModifiedDate:yyyy-MM-dd},{productDto.Status}");
                }

                var buffer = Encoding.UTF8.GetBytes(csvBuilder.ToString());
                var fileName = $"produktai_{DateTime.Now:yyyy-MM-dd}.csv";
                
                return File(buffer, "text/csv", fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida eksportuojant produktus į CSV");
                return StatusCode(500, "Vidinė serverio klaida eksportuojant produktus");
            }
        }

        /// <summary>
        /// Gauti produktus, kuriems reikia papildymo
        /// </summary>
        /// <returns>Produktų, kuriems reikia papildymo, sąrašas</returns>
        [HttpGet("needs-restock")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<ProductDto>))]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetNeedsRestock()
        {
            try
            {
                _logger.LogInformation("Gaunamas produktų, kuriems reikia papildymo, sąrašas");
                
                var products = await _productsService.ListAsync();
                var needsRestock = products
                    .Where(p => p.QuantityInStock <= p.MinimumStockLevel)
                    .Adapt<List<ProductDto>>();

                return Ok(needsRestock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produktų, kuriems reikia papildymo, sąrašą");
                return StatusCode(500, "Vidinė serverio klaida gaunant produktų sąrašą");
            }
        }

        /// <summary>
        /// Apsaugo CSV lauko reikšmę
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

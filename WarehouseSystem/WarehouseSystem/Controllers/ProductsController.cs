using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Controllers
{
    /// <summary>
    /// Kontroleris skirtas produktų API endpoint'ų valdymui
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        // Serviso ir logerio injekcijos per konstruktorių
        // IProductService - pagrindinis produktų valdymo servisas
        // ILogger - klaidų ir veiksmų registravimui
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(
            IProductService productService,
            ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        #region Pagrindinės CRUD Operacijos

        /// <summary>
        /// Grąžina visų produktų sąrašą
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts()
        {
            try
            {
                var products = await _productService.GetAllProductsAsync();
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produktų sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Grąžina produktą pagal ID
        /// </summary>
        /// <param name="id">Produkto ID</param>
        [HttpGet("{id}")]
        public async Task<ActionResult<ProductDto>> GetProduct(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound($"Produktas su ID {id} nerastas");
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produktą {ProductId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Grąžina produktą pagal EAN kodą
        /// </summary>
        /// <param name="eanCode">Produkto EAN kodas</param>
        [HttpGet("ean/{eanCode}")]
        public async Task<ActionResult<ProductDto>> GetProductByEAN(string eanCode)
        {
            try
            {
                var product = await _productService.GetProductByEANAsync(eanCode);
                if (product == null)
                {
                    return NotFound($"Produktas su EAN {eanCode} nerastas");
                }
                return Ok(product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produktą pagal EAN {EanCode}", eanCode);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Sukuria naują produktą
        /// </summary>
        /// <param name="productDto">Naujo produkto duomenys</param>
        [HttpPost]
        public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto productDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var createdProduct = await _productService.CreateProductAsync(productDto);
                return CreatedAtAction(
                    nameof(GetProduct),
                    new { id = createdProduct.Id },
                    createdProduct
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant naują produktą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Atnaujina esamą produktą
        /// </summary>
        /// <param name="id">Produkto ID</param>
        /// <param name="productDto">Atnaujinti produkto duomenys</param>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] UpdateProductDto productDto)
        {
            try
            {
                if (id != productDto.Id)
                {
                    return BadRequest("Produkto ID nesutampa su URL nurodytu ID");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var updatedProduct = await _productService.UpdateProductAsync(productDto);
                return Ok(updatedProduct);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant produktą {ProductId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region QR Kodų Operacijos

        /// <summary>
        /// Grąžina visus produkto QR kodus
        /// </summary>
        /// <param name="id">Produkto ID</param>
        [HttpGet("{id}/qrcodes")]
        public async Task<ActionResult<IEnumerable<ProductQRCodeDto>>> GetProductQRCodes(int id)
        {
            try
            {
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null)
                {
                    return NotFound($"Produktas su ID {id} nerastas");
                }

                var qrCodes = await _productService.GetProductQRCodesAsync(id);
                return Ok(qrCodes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produkto {ProductId} QR kodus", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Prideda naują QR kodą produktui
        /// </summary>
        /// <param name="qrCodeDto">Naujo QR kodo duomenys</param>
        [HttpPost("qrcodes")]
        public async Task<ActionResult<ProductQRCodeDto>> AddQRCode([FromBody] AddProductQRCodeDto qrCodeDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var product = await _productService.GetProductByIdAsync(qrCodeDto.ProductId);
                if (product == null)
                {
                    return NotFound($"Produktas su ID {qrCodeDto.ProductId} nerastas");
                }

                var qrCode = await _productService.AddQRCodeAsync(qrCodeDto);
                return CreatedAtAction(
                    nameof(GetProductQRCodes),
                    new { id = qrCodeDto.ProductId },
                    qrCode
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida pridedant QR kodą produktui {ProductId}", qrCodeDto.ProductId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Atnaujina QR kodo būseną
        /// </summary>
        /// <param name="statusDto">Naujos būsenos duomenys</param>
        [HttpPut("qrcodes/status")]
        public async Task<IActionResult> UpdateQRCodeStatus([FromBody] UpdateQRCodeStatusDto statusDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var success = await _productService.UpdateQRCodeStatusAsync(statusDto);
                if (!success)
                {
                    return NotFound($"QR kodas {statusDto.QRCodeId} nerastas");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant QR kodo {QRCodeId} būseną", statusDto.QRCodeId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Paieška ir Filtravimas

        /// <summary>
        /// Ieško produktų pagal pateiktą terminą
        /// </summary>
        /// <param name="term">Paieškos terminas</param>
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> SearchProducts([FromQuery] string term)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(term))
                {
                    return BadRequest("Paieškos terminas negali būti tuščias");
                }

                var products = await _productService.SearchProductsAsync(term);
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida ieškant produktų su terminu: {SearchTerm}", term);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        /// <summary>
        /// Grąžina produktus pagal kategoriją
        /// </summary>
        /// <param name="category">Kategorijos pavadinimas</param>
        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<ProductDto>>> Get

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly ILogger<WarehouseController> _logger;

        public WarehouseController(
            IWarehouseService warehouseService,
            ILogger<WarehouseController> logger)
        {
            _warehouseService = warehouseService;
            _logger = logger;
        }

        #region Lokacijų valdymas

        [HttpGet("locations/{warehouseId}")]
        public async Task<ActionResult<IEnumerable<ProductLocationDto>>> GetLocations(string warehouseId)
        {
            try
            {
                var locations = await _warehouseService.GetAllLocationsAsync(warehouseId);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant sandėlio {WarehouseId} lokacijas", warehouseId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("locations/{warehouseId}/{locationCode}")]
        public async Task<ActionResult<ProductLocationDto>> GetLocation(
            string warehouseId,
            string locationCode)
        {
            try
            {
                var location = await _warehouseService.GetLocationAsync(warehouseId, locationCode);
                if (location == null)
                {
                    return NotFound($"Lokacija {locationCode} nerasta sandėlyje {warehouseId}");
                }
                return Ok(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant lokaciją {LocationCode}", locationCode);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("locations/{warehouseId}")]
        public async Task<IActionResult> CreateLocation(
            string warehouseId,
            [FromBody] CreateLocationDto locationDto)
        {
            try
            {
                var success = await _warehouseService.CreateLocationAsync(
                    warehouseId,
                    locationDto.Zone,
                    locationDto.Aisle,
                    locationDto.Rack,
                    locationDto.Shelf,
                    locationDto.Bin,
                    locationDto.MaxCapacity,
                    locationDto.StorageConditions
                );

                if (!success)
                {
                    return BadRequest("Nepavyko sukurti lokacijos");
                }

                return CreatedAtAction(
                    nameof(GetLocation),
                    new { warehouseId, locationCode = $"{locationDto.Zone}-{locationDto.Aisle}-{locationDto.Rack}-{locationDto.Shelf}-{locationDto.Bin}" },
                    locationDto
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant naują lokaciją");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPut("locations/{warehouseId}/{locationCode}")]
        public async Task<IActionResult> UpdateLocation(
            string warehouseId,
            string locationCode,
            [FromBody] UpdateLocationDto locationDto)
        {
            try
            {
                var success = await _warehouseService.UpdateLocationAsync(
                    warehouseId,
                    locationCode,
                    locationDto.MaxCapacity,
                    locationDto.StorageConditions,
                    locationDto.IsQuarantine
                );

                if (!success)
                {
                    return NotFound($"Lokacija {locationCode} nerasta");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant lokaciją {LocationCode}", locationCode);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpDelete("locations/{warehouseId}/{locationCode}")]
        public async Task<IActionResult> DeleteLocation(string warehouseId, string locationCode)
        {
            try
            {
                var success = await _warehouseService.DeleteLocationAsync(warehouseId, locationCode);
                if (!success)
                {
                    return NotFound($"Lokacija {locationCode} nerasta arba negali būti ištrinta");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant lokaciją {LocationCode}", locationCode);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Produktų operacijos

        [HttpGet("products/{productId}/locations")]
        public async Task<ActionResult<IEnumerable<ProductLocationDto>>> GetProductLocations(int productId)
        {
            try
            {
                var locations = await _warehouseService.GetProductLocationsAsync(productId);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produkto {ProductId} lokacijas", productId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("products/receive")]
        public async Task<IActionResult> ReceiveProduct([FromBody] ReceiveProductDto receiveDto)
        {
            try
            {
                var success = await _warehouseService.ReceiveProductAsync(
                    receiveDto.ProductId,
                    receiveDto.WarehouseId,
                    receiveDto.LocationCode,
                    receiveDto.Quantity,
                    receiveDto.QRCodeId
                );

                if (!success)
                {
                    return BadRequest("Nepavyko priimti produkto");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida priimant produktą {ProductId}", receiveDto.ProductId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("products/transfer")]
        public async Task<IActionResult> TransferProduct([FromBody] TransferProductDto transferDto)
        {
            try
            {
                var success = await _warehouseService.TransferProductAsync(
                    transferDto.ProductId,
                    transferDto.SourceWarehouseId,
                    transferDto.SourceLocationCode,
                    transferDto.DestinationWarehouseId,
                    transferDto.DestinationLocationCode,
                    transferDto.Quantity,
                    transferDto.QRCodeId
                );

                if (!success)
                {
                    return BadRequest("Nepavyko perkelti produkto");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida perkeliant produktą {ProductId}", transferDto.ProductId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("products/ship")]
        public async Task<IActionResult> ShipProduct([FromBody] ShipProductDto shipDto)
        {
            try
            {
                var success = await _warehouseService.ShipProductAsync(
                    shipDto.ProductId,
                    shipDto.WarehouseId,
                    shipDto.LocationCode,
                    shipDto.Quantity,
                    shipDto.ReferenceNumber,
                    shipDto.QRCodeId
                );

                if (!success)
                {
                    return BadRequest("Nepavyko išsiųsti produkto");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida išsiunčiant produktą {ProductId}", shipDto.ProductId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Inventorizacija

        [HttpPost("inventory/count")]
        public async Task<ActionResult<ProductLocationDto>> CountStock(
            [FromBody] StockCountDto countDto)
        {
            try
            {
                var result = await _warehouseService.CountStockAsync(
                    countDto.ProductId,
                    countDto.WarehouseId,
                    countDto.LocationCode,
                    countDto.CountedQuantity,
                    countDto.CountedByUser
                );

                if (result == null)
                {
                    return BadRequest("Nepavyko atlikti inventorizacijos");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atliekant inventorizaciją");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("inventory/adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] StockAdjustmentDto adjustmentDto)
        {
            try
            {
                var success = await _warehouseService.AdjustStockAsync(
                    adjustmentDto.ProductId,
                    adjustmentDto.WarehouseId,
                    adjustmentDto.LocationCode,
                    adjustmentDto.NewQuantity,
                    adjustmentDto.Reason,
                    adjustmentDto.AdjustedByUser
                );

                if (!success)
                {
                    return BadRequest("Nepavyko atlikti kiekio koregavimo");
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida koreguojant kiekį");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Statistika ir ataskaitos

        [HttpGet("statistics/stock-levels")]
        public async Task<ActionResult<IDictionary<string, decimal>>> GetStockLevels(
            [FromQuery] string warehouseId)
        {
            try
            {
                var levels = await _warehouseService.GetStockLevelsAsync(warehouseId);
                return Ok(levels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant atsargų lygius");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("statistics/low-stock")]
        public async Task<ActionResult<IEnumerable<ProductLocationDto>>> GetLowStockLocations(
            [FromQuery] string warehouseId)
        {
            try
            {
                var locations = await _warehouseService.GetLowStockLocationsAsync(warehouseId);
                return Ok(locations);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant žemo likučio lokacijas");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("report")]
        public async Task<ActionResult<string>> GenerateStockReport(
            [FromQuery] string warehouseId,
            [FromQuery] DateTime reportDate)
        {
            try
            {
                var report = await _warehouseService.GenerateStockReportAsync(
                    warehouseId,
                    reportDate
                );
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida generuojant atsargų ataskaitą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region CSV operacijos

        [HttpGet("export")]
        public async Task<IActionResult> ExportLocations(
            [FromQuery] string warehouseId = null)
        {
            try
            {
                var fileName = $"warehouse_locations_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var tempPath = Path.Combine(Path.GetTempPath(), fileName);

                await _warehouseService.ExportLocationsToCsvAsync(tempPath, warehouseId);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
                System.IO.File.Delete(tempPath);

                return File(
                    fileBytes,
                    "text/csv",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida eksportuojant lokacijas į CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportLocations(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nepateiktas CSV failas");
                }

                var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);

                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _warehouseService.ImportLocationsFromCsvAsync(tempPath);
                System.IO.File.Delete(tempPath);

                return Ok("CSV importuotas sėkmingai");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida importuojant lokacijas iš CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion
    }
}

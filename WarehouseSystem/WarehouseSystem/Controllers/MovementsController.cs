using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Products;

namespace WarehouseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovementsController : ControllerBase
    {
        private readonly IMovementService _movementService;
        private readonly ILogger<MovementsController> _logger;

        public MovementsController(
            IMovementService movementService,
            ILogger<MovementsController> logger)
        {
            _movementService = movementService;
            _logger = logger;
        }

        #region Judėjimų gavimas ir paieška

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductMovementDto>>> GetMovements(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var movements = await _movementService.GetMovementsAsync(startDate, endDate);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant judėjimų sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductMovementDto>> GetMovement(int id)
        {
            try
            {
                var movement = await _movementService.GetMovementByIdAsync(id);
                if (movement == null)
                {
                    return NotFound($"Judėjimas su ID {id} nerastas");
                }
                return Ok(movement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant judėjimą {MovementId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<ProductMovementDto>>> SearchMovements(
            [FromQuery] string searchTerm,
            [FromQuery] string movementType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return BadRequest("Paieškos terminas negali būti tuščias");
                }

                var movements = await _movementService.SearchMovementsAsync(
                    searchTerm,
                    movementType,
                    startDate,
                    endDate
                );
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida ieškant judėjimų su terminu: {SearchTerm}", searchTerm);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Produkto judėjimai

        [HttpGet("product/{productId}")]
        public async Task<ActionResult<IEnumerable<ProductMovementDto>>> GetProductMovements(
            int productId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var movements = await _movementService.GetProductMovementsAsync(
                    productId,
                    startDate,
                    endDate
                );
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produkto {ProductId} judėjimus", productId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("product/{productId}/totals")]
        public async Task<ActionResult<object>> GetProductMovementTotals(
            int productId,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var incoming = await _movementService.GetProductTotalIncomingAsync(
                    productId,
                    startDate,
                    endDate
                );
                var outgoing = await _movementService.GetProductTotalOutgoingAsync(
                    productId,
                    startDate,
                    endDate
                );

                return Ok(new
                {
                    TotalIncoming = incoming,
                    TotalOutgoing = outgoing,
                    Balance = incoming - outgoing
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant produkto {ProductId} judėjimų sumas", productId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region QR kodų judėjimai

        [HttpGet("qrcode/{qrCodeId}")]
        public async Task<ActionResult<IEnumerable<ProductMovementDto>>> GetQRCodeMovements(string qrCodeId)
        {
            try
            {
                var movements = await _movementService.GetQRCodeMovementsAsync(qrCodeId);
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant QR kodo {QRCodeId} judėjimus", qrCodeId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("qrcode/{qrCodeId}/location")]
        public async Task<ActionResult<string>> GetQRCodeCurrentLocation(string qrCodeId)
        {
            try
            {
                var location = await _movementService.GetQRCodeCurrentLocationAsync(qrCodeId);
                if (location == null)
                {
                    return NotFound($"QR kodas {qrCodeId} neturi aktyvios lokacijos");
                }
                return Ok(location);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant QR kodo {QRCodeId} lokaciją", qrCodeId);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Lokacijos judėjimai

        [HttpGet("location/{locationCode}")]
        public async Task<ActionResult<IEnumerable<ProductMovementDto>>> GetLocationMovements(
            string locationCode,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var movements = await _movementService.GetLocationMovementsAsync(
                    locationCode,
                    startDate,
                    endDate
                );
                return Ok(movements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant lokacijos {LocationCode} judėjimus", locationCode);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("location/{locationCode}/totals")]
        public async Task<ActionResult<object>> GetLocationMovementTotals(
            string locationCode,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var incoming = await _movementService.GetLocationTotalIncomingAsync(
                    locationCode,
                    startDate,
                    endDate
                );
                var outgoing = await _movementService.GetLocationTotalOutgoingAsync(
                    locationCode,
                    startDate,
                    endDate
                );

                return Ok(new
                {
                    TotalIncoming = incoming,
                    TotalOutgoing = outgoing,
                    Balance = incoming - outgoing
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant lokacijos {LocationCode} judėjimų sumas", locationCode);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Judėjimų valdymas

        [HttpPost]
        public async Task<ActionResult<ProductMovementDto>> RegisterMovement(
            [FromBody] ProductMovementRegistrationDto registrationDto)
        {
            try
            {
                var movement = await _movementService.RegisterMovementAsync(
                    registrationDto.ProductId,
                    registrationDto.MovementType,
                    registrationDto.Quantity,
                    registrationDto.SourceLocation,
                    registrationDto.DestinationLocation,
                    registrationDto.ReferenceNumber,
                    registrationDto.QRCodeId
                );

                if (movement == null)
                {
                    return BadRequest("Neteisingi judėjimo duomenys");
                }

                return CreatedAtAction(
                    nameof(GetMovement),
                    new { id = movement.MovementId },
                    movement
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida registruojant naują judėjimą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelMovement(
            int id,
            [FromBody] MovementCancellationDto cancellationDto)
        {
            try
            {
                var success = await _movementService.CancelMovementAsync(
                    id,
                    cancellationDto.Reason,
                    cancellationDto.CanceledByUser
                );

                if (!success)
                {
                    return NotFound($"Judėjimas su ID {id} nerastas");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atšaukiant judėjimą {MovementId}", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPut("{id}/quantity")]
        public async Task<IActionResult> AdjustMovementQuantity(
            int id,
            [FromBody] MovementQuantityAdjustmentDto adjustmentDto)
        {
            try
            {
                var success = await _movementService.AdjustMovementAsync(
                    id,
                    adjustmentDto.NewQuantity,
                    adjustmentDto.Reason,
                    adjustmentDto.AdjustedByUser
                );

                if (!success)
                {
                    return NotFound($"Judėjimas su ID {id} nerastas");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida koreguojant judėjimo {MovementId} kiekį", id);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Statistika ir ataskaitos

        [HttpGet("statistics")]
        public async Task<ActionResult<object>> GetMovementStatistics(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var countsByType = await _movementService.GetMovementCountsByTypeAsync(startDate, endDate);
                
                return Ok(new
                {
                    MovementCounts = countsByType,
                    DateRange = new
                    {
                        StartDate = startDate,
                        EndDate = endDate
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant judėjimų statistiką");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("report")]
        public async Task<ActionResult<string>> GenerateMovementReport(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime endDate,
            [FromQuery] string movementType = null)
        {
            try
            {
                var report = await _movementService.GenerateMovementReportAsync(
                    startDate,
                    endDate,
                    movementType
                );
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida generuojant judėjimų ataskaitą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("topmoving")]
        public async Task<ActionResult<IEnumerable<ProductMovementDto>>> GetTopMovingProducts(
            [FromQuery] int count = 10,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var products = await _movementService.GetTopMovingProductsAsync(
                    count,
                    startDate,
                    endDate
                );
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant daugiausiai judančių produktų sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region CSV operacijos

        [HttpGet("export")]
        public async Task<IActionResult> ExportMovements(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var fileName = $"movements_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var tempPath = Path.Combine(Path.GetTempPath(), fileName);

                await _movementService.ExportMovementsToCsvAsync(tempPath, startDate, endDate);

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
                _logger.LogError(ex, "Klaida eksportuojant judėjimus į CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportMovements(IFormFile file)
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

                await _movementService.ImportMovementsFromCsvAsync(tempPath);
                System.IO.File.Delete(tempPath);

                return Ok("CSV importuotas sėkmingai");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida importuojant judėjimus iš CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion
    }

    public class ProductMovementRegistrationDto
    {
        public int ProductId { get; set; }
        public string MovementType { get; set; }
        public decimal Quantity { get; set; }
        public string SourceLocation { get; set; }
        public string DestinationLocation { get; set; }
        public string ReferenceNumber { get; set; }
        public string QRCodeId { get; set; }
    }

    public class MovementCancellationDto
    {
        public string Reason { get; set; }
        public string CanceledByUser { get; set; }
    }

    public class MovementQuantityAdjustmentDto
    {
        public decimal NewQuantity { get; set; }
        public string Reason { get; set; }
        public string AdjustedByUser { get; set; }
    }
}

namespace Presentation.DTOs.Products
{
    /// <summary>
    /// Naujos sandėlio vietos sukūrimo DTO
    /// </summary>
    public class CreateLocationDto
    {
        public string Zone { get; set; }
        public string Aisle { get; set; }
        public string Rack { get; set; }
        public string Shelf { get; set; }
        public string Bin { get; set; }
        public decimal MaxCapacity { get; set; }
        public string StorageConditions { get; set; }
        public string UnitOfMeasure { get; set; } = "VNT";
        public decimal MinimumQuantity { get; set; } = 0;
    }

    /// <summary>
    /// Sandėlio vietos atnaujinimo DTO
    /// </summary>
    public class UpdateLocationDto
    {
        public decimal? MaxCapacity { get; set; }
        public string StorageConditions { get; set; }
        public bool? IsQuarantine { get; set; }
        public decimal? MinimumQuantity { get; set; }
        public decimal? Temperature { get; set; }
        public decimal? Humidity { get; set; }
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }

    /// <summary>
    /// Produkto priėmimo į sandėlį DTO
    /// </summary>
    public class ReceiveProductDto
    {
        public int ProductId { get; set; }
        public string WarehouseId { get; set; }
        public string LocationCode { get; set; }
        public decimal Quantity { get; set; }
        public string QRCodeId { get; set; }
        public string ReceivedByUser { get; set; }
        public string PurchaseInvoice { get; set; }
        public string SupplierName { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Produkto perkėlimo tarp lokacijų DTO
    /// </summary>
    public class TransferProductDto
    {
        public int ProductId { get; set; }
        public string SourceWarehouseId { get; set; }
        public string SourceLocationCode { get; set; }
        public string DestinationWarehouseId { get; set; }
        public string DestinationLocationCode { get; set; }
        public decimal Quantity { get; set; }
        public string QRCodeId { get; set; }
        public string TransferredByUser { get; set; }
        public string TransferReason { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Produkto išsiuntimo iš sandėlio DTO
    /// </summary>
    public class ShipProductDto
    {
        public int ProductId { get; set; }
        public string WarehouseId { get; set; }
        public string LocationCode { get; set; }
        public decimal Quantity { get; set; }
        public string QRCodeId { get; set; }
        public string ReferenceNumber { get; set; }
        public string ShippedByUser { get; set; }
        public string ShipmentType { get; set; }  // Sales, Return, Transfer, etc.
        public string CustomerInfo { get; set; }
        public string Notes { get; set; }
    }

    /// <summary>
    /// Inventorizacijos skaičiavimo DTO
    /// </summary>
    public class StockCountDto
    {
        public int ProductId { get; set; }
        public string WarehouseId { get; set; }
        public string LocationCode { get; set; }
        public decimal CountedQuantity { get; set; }
        public string CountedByUser { get; set; }
        public DateTime CountDate { get; set; } = DateTime.Now;
        public string Notes { get; set; }
    }

    /// <summary>
    /// Atsargų kiekio koregavimo DTO
    /// </summary>
    public class StockAdjustmentDto
    {
        public int ProductId { get; set; }
        public string WarehouseId { get; set; }
        public string LocationCode { get; set; }
        public decimal NewQuantity { get; set; }
        public string Reason { get; set; }
        public string AdjustedByUser { get; set; }
        public string ApprovedByUser { get; set; }
        public DateTime AdjustmentDate { get; set; } = DateTime.Now;
        public string Notes { get; set; }
    }

    /// <summary>
    /// Sandėlio vietų užimtumo statistikos DTO
    /// </summary>
    public class LocationUtilizationDto
    {
        public string WarehouseId { get; set; }
        public string LocationCode { get; set; }
        public decimal CurrentQuantity { get; set; }
        public decimal MaxCapacity { get; set; }
        public decimal UtilizationPercentage => MaxCapacity > 0 
            ? (CurrentQuantity / MaxCapacity) * 100 
            : 0;
        public string UnitOfMeasure { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Sandėlio zonos statistikos DTO
    /// </summary>
    public class ZoneStatisticsDto
    {
        public string WarehouseId { get; set; }
        public string Zone { get; set; }
        public int TotalLocations { get; set; }
        public int EmptyLocations { get; set; }
        public int PartiallyFullLocations { get; set; }
        public int FullLocations { get; set; }
        public decimal TotalCapacity { get; set; }
        public decimal UsedCapacity { get; set; }
        public decimal UtilizationPercentage => TotalCapacity > 0 
            ? (UsedCapacity / TotalCapacity) * 100 
            : 0;
        public DateTime LastUpdated { get; set; }
    }

    /// <summary>
    /// Sandėlio būsenos istorijos įrašo DTO
    /// </summary>
    public class LocationStatusHistoryDto
    {
        public string WarehouseId { get; set; }
        public string LocationCode { get; set; }
        public string ChangeType { get; set; }  // Created, Updated, Deleted, etc.
        public decimal? OldQuantity { get; set; }
        public decimal? NewQuantity { get; set; }
        public string ChangedByUser { get; set; }
        public string ChangeReason { get; set; }
        public DateTime ChangeDate { get; set; }

        public string ToCsvLine()
        {
            return $"{WarehouseId}," +
                   $"{LocationCode}," +
                   $"\"{ChangeType}\"," +
                   $"{OldQuantity}," +
                   $"{NewQuantity}," +
                   $"\"{ChangedByUser}\"," +
                   $"\"{EscapeCsvField(ChangeReason)}\"," +
                   $"{ChangeDate:yyyy-MM-dd HH:mm:ss}";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }
}

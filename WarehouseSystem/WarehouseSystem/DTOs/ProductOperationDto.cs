namespace Presentation.DTOs.Products
{
    /// <summary>
    /// DTO naujo produkto sukūrimui
    /// </summary>
    public class CreateProductDto
    {
        // Pagrindiniai laukai
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string EANCode { get; set; }
        public string LegacyCode { get; set; }
        public string Description { get; set; }

        // QR kodo informacija
        public bool HasBatchQR { get; set; }            // Ar naudos partijai skirtą QR
        public bool HasUniqueItemQR { get; set; }       // Ar kiekvienas vienetas turės unikalų QR
        public string InitialQRCode { get; set; }       // Pradinis produkto QR kodas
        
        // Fizinės savybės
        public decimal WeightNet { get; set; }
        public decimal WeightGross { get; set; }
        public string UnitOfMeasure { get; set; }
        
        // Klasifikacija
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string Manufacturer { get; set; }
        
        // Produkto tipas
        public bool IsBatchProduct { get; set; }
        public decimal MinimumStock { get; set; }

        // Kainos ir PVM
        public decimal PurchasePrice { get; set; }          // Pirkimo kaina be PVM
        public decimal RetailPrice { get; set; }            // Pardavimo kaina be PVM
        public decimal WholesalePrice { get; set; }         // Didmeninė kaina be PVM
        public decimal VATRate { get; set; }                // PVM tarifas (default 21)
        public string Currency { get; set; }                // Valiuta (default EUR)

        // Papildoma informacija
        public string CountryOfOrigin { get; set; }
        public List<string> Certificates { get; set; }
        public string CreatedByUser { get; set; }

        public CreateProductDto()
        {
            VATRate = 21;                                   // Numatytasis PVM
            Currency = "EUR";                               // Numatytoji valiuta
            Certificates = new List<string>();
        }

        public string ToCsvLine()
        {
            var purchasePriceVAT = PurchasePrice * (1 + VATRate / 100);
            var retailPriceVAT = RetailPrice * (1 + VATRate / 100);
            var wholesalePriceVAT = WholesalePrice * (1 + VATRate / 100);
            var margin = PurchasePrice > 0 ? ((RetailPrice - PurchasePrice) / PurchasePrice) * 100 : 0;

            return $"\"{EscapeCsvField(Name)}\"," +
                   $"\"{EscapeCsvField(NameEn)}\"," +
                   $"{EANCode}," +
                   $"{LegacyCode}," +
                   $"\"{EscapeCsvField(Description)}\"," +
                   $"{HasBatchQR}," +
                   $"{HasUniqueItemQR}," +
                   $"\"{InitialQRCode}\"," +
                   $"{WeightNet}," +
                   $"{WeightGross}," +
                   $"\"{UnitOfMeasure}\"," +
                   $"\"{EscapeCsvField(Category)}\"," +
                   $"\"{EscapeCsvField(SubCategory)}\"," +
                   $"\"{EscapeCsvField(Manufacturer)}\"," +
                   $"{IsBatchProduct}," +
                   $"{MinimumStock}," +
                   $"{PurchasePrice}," +
                   $"{purchasePriceVAT}," +
                   $"{RetailPrice}," +
                   $"{retailPriceVAT}," +
                   $"{WholesalePrice}," +
                   $"{wholesalePriceVAT}," +
                   $"{VATRate}," +
                   $"{Currency}," +
                   $"{margin}," +
                   $"\"{CountryOfOrigin}\"," +
                   $"\"{string.Join("|", Certificates)}\"," +
                   $"\"{CreatedByUser}\"," +
                   $"{DateTime.Now:yyyy-MM-dd HH:mm:ss}";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }

    /// <summary>
    /// DTO produkto atnaujinimui
    /// </summary>
    public class UpdateProductDto
    {
        public int Id { get; set; }
        
        // Pagrindinė informacija
        public string Name { get; set; }
        public string NameEn { get; set; }
        public string Description { get; set; }
        
        // QR kodo atnaujinimas
        public string NewQRCode { get; set; }              // Naujas QR kodas
        public bool? HasBatchQR { get; set; }              // Keisti QR tipo požymį
        public bool? HasUniqueItemQR { get; set; }         // Keisti individualių QR požymį
        
        // Fizinės savybės
        public decimal? WeightNet { get; set; }
        public decimal? WeightGross { get; set; }
        
        // Kainos
        public decimal? RetailPrice { get; set; }
        public decimal? PurchasePrice { get; set; }
        public decimal? WholesalePrice { get; set; }
        public decimal? VATRate { get; set; }
        
        // Būsena
        public string Status { get; set; }
        public bool? IsActive { get; set; }
        
        // Audito informacija
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string ToCsvLine()
        {
            return $"{Id}," +
                   $"\"{EscapeCsvField(Name)}\"," +
                   $"\"{EscapeCsvField(NameEn)}\"," +
                   $"\"{EscapeCsvField(Description)}\"," +
                   $"\"{NewQRCode}\"," +
                   $"{HasBatchQR}," +
                   $"{HasUniqueItemQR}," +
                   $"{WeightNet}," +
                   $"{WeightGross}," +
                   $"{RetailPrice}," +
                   $"{PurchasePrice}," +
                   $"{WholesalePrice}," +
                   $"{VATRate}," +
                   $"\"{Status}\"," +
                   $"{IsActive}," +
                   $"\"{EscapeCsvField(UpdatedByUser)}\"," +
                   $"\"{EscapeCsvField(UpdateReason)}\"," +
                   $"{UpdatedAt:yyyy-MM-dd HH:mm:ss}";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }

    /// <summary>
    /// DTO naujo QR kodo pridėjimui
    /// </summary>
    public class AddProductQRCodeDto
    {
        public int ProductId { get; set; }
        public string BatchNumber { get; set; }            // Partijos numeris
        public decimal BatchQuantity { get; set; }         // Partijos kiekis
        public string QRCodeType { get; set; }             // "Batch" arba "Individual"
        public int NumberOfQRCodes { get; set; }           // Kiek QR kodų sugeneruoti
        
        // Pirkimo informacija
        public string PurchaseInvoice { get; set; }        // Pirkimo sąskaitos numeris
        public string SupplierName { get; set; }           // Tiekėjo pavadinimas
        public decimal PurchasePrice { get; set; }         // Pirkimo kaina
        public string Currency { get; set; }               // Valiuta
        public decimal VATRate { get; set; }               // PVM tarifas

        // Papildoma informacija
        public string Location { get; set; }               // Sandėlio vieta
        public string ReceivedByUser { get; set; }         // Kas priėmė
        public string Notes { get; set; }                  // Pastabos
        public DateTime ReceivedDate { get; set; } = DateTime.Now;

        public AddProductQRCodeDto()
        {
            VATRate = 21;                                  // Numatytasis PVM
            Currency = "EUR";                              // Numatytoji valiuta
            QRCodeType = "Batch";                          // Numatytasis tipas
            NumberOfQRCodes = 1;                           // Numatytasis kiekis
        }

        public string ToCsvLine()
        {
            var purchasePriceVAT = PurchasePrice * (1 + VATRate / 100);
            
            return $"{ProductId}," +
                   $"{BatchNumber}," +
                   $"{BatchQuantity}," +
                   $"\"{QRCodeType}\"," +
                   $"{NumberOfQRCodes}," +
                   $"\"{PurchaseInvoice}\"," +
                   $"\"{EscapeCsvField(SupplierName)}\"," +
                   $"{PurchasePrice}," +
                   $"{purchasePriceVAT}," +
                   $"{VATRate}," +
                   $"{Currency}," +
                   $"\"{Location}\"," +
                   $"\"{EscapeCsvField(ReceivedByUser)}\"," +
                   $"\"{EscapeCsvField(Notes)}\"," +
                   $"{ReceivedDate:yyyy-MM-dd HH:mm:ss}";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }

    /// <summary>
    /// DTO QR kodo būsenos atnaujinimui
    /// </summary>
    public class UpdateQRCodeStatusDto
    {
        public string QRCodeId { get; set; }
        public string NewStatus { get; set; }              // Active, Used, Damaged, Lost
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public string ToCsvLine()
        {
            return $"{QRCodeId}," +
                   $"\"{NewStatus}\"," +
                   $"\"{EscapeCsvField(UpdatedByUser)}\"," +
                   $"\"{EscapeCsvField(UpdateReason)}\"," +
                   $"{UpdatedAt:yyyy-MM-dd HH:mm:ss}";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }
}

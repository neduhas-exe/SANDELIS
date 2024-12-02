namespace Presentation.DTOs.Products
{
    /// <summary>
    /// DTO klasė produktų sekimui su QR kodais
    /// </summary>
    public class ProductQRCodeDto
    {
        // Identifikacija
        public string QRCodeId { get; set; }               // Unikalus QR kodo identifikatorius
        public int ProductId { get; set; }                 // Produkto ID kuriam priklauso QR
        
        // Partijos informacija
        public string BatchNumber { get; set; }            // Partijos numeris
        public decimal Quantity { get; set; }              // Kiekis
        public string QRCodeType { get; set; }             // Batch arba Individual
        
        // Priėmimo informacija
        public DateTime ReceivedDate { get; set; }         // Gavimo data
        public string ReceivedByUser { get; set; }         // Kas priėmė
        public string WarehouseLocation { get; set; }      // Kur padėta

        // Tiekėjo informacija
        public string PurchaseInvoice { get; set; }        // Pirkimo sąskaitos numeris
        public string SupplierName { get; set; }           // Tiekėjo pavadinimas
        public string SupplierInvoiceNumber { get; set; }  // Tiekėjo sąskaitos numeris
        public string SupplierOrderNumber { get; set; }    // Tiekėjo užsakymo numeris

        // Finansinė informacija
        public decimal BatchPurchasePrice { get; set; }    // Šios partijos pirkimo kaina be PVM
        public decimal BatchPurchasePriceVAT { get; set; } // Šios partijos pirkimo kaina su PVM
        public decimal VATRate { get; set; }               // PVM tarifas šiai partijai
        public string Currency { get; set; }               // Valiuta
        public DateTime PurchaseDate { get; set; }         // Pirkimo data

        // Būsena
        public string Status { get; set; }                 // Active, Used, Damaged, Lost
        public DateTime StatusChangedAt { get; set; }      // Paskutinio statuso pakeitimo data
        public string StatusChangedBy { get; set; }        // Kas pakeitė statusą

        public static string GetCsvHeader()
        {
            return "QRCodeId,ProductId,BatchNumber,Quantity,QRCodeType,ReceivedDate,ReceivedByUser," +
                   "WarehouseLocation,PurchaseInvoice,SupplierName,SupplierInvoiceNumber," +
                   "SupplierOrderNumber,BatchPurchasePrice,BatchPurchasePriceVAT,VATRate," +
                   "Currency,PurchaseDate,Status,StatusChangedAt,StatusChangedBy";
        }

        public string ToCsvLine()
        {
            return $"{QRCodeId}," +
                   $"{ProductId}," +
                   $"{BatchNumber}," +
                   $"{Quantity}," +
                   $"\"{QRCodeType}\"," +
                   $"{ReceivedDate:yyyy-MM-dd}," +
                   $"\"{EscapeCsvField(ReceivedByUser)}\"," +
                   $"\"{EscapeCsvField(WarehouseLocation)}\"," +
                   $"\"{PurchaseInvoice}\"," +
                   $"\"{EscapeCsvField(SupplierName)}\"," +
                   $"\"{SupplierInvoiceNumber}\"," +
                   $"\"{SupplierOrderNumber}\"," +
                   $"{BatchPurchasePrice}," +
                   $"{BatchPurchasePriceVAT}," +
                   $"{VATRate}," +
                   $"{Currency}," +
                   $"{PurchaseDate:yyyy-MM-dd}," +
                   $"\"{Status}\"," +
                   $"{StatusChangedAt:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{EscapeCsvField(StatusChangedBy)}\"";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }

    /// <summary>
    /// DTO klasė produkto vietos sandėlyje sekimui
    /// </summary>
    public class ProductLocationDto
    {
        // Sandėlio informacija
        public string WarehouseId { get; set; }            // Sandėlio ID
        public string Zone { get; set; }                   // Zona sandėlyje
        public string Aisle { get; set; }                  // Praėjimas
        public string Rack { get; set; }                   // Lentynų blokas
        public string Shelf { get; set; }                  // Lentyna
        public string Bin { get; set; }                    // Dėžė/vieta

        // Kiekiai ir talpa
        public decimal Quantity { get; set; }              // Kiekis toje vietoje
        public decimal MaxCapacity { get; set; }           // Maksimali talpa
        public string UnitOfMeasure { get; set; }          // Matavimo vienetas
        public decimal MinimumQuantity { get; set; }       // Minimalus kiekis šioje vietoje

        // Būsena ir sąlygos
        public bool IsQuarantine { get; set; }             // Ar tai karantino zona
        public string StorageConditions { get; set; }      // Saugojimo sąlygos
        public decimal Temperature { get; set; }           // Temperatūra (jei aktualu)
        public decimal Humidity { get; set; }              // Drėgmė (jei aktualu)

        // Finansinė informacija
        public decimal StockValue { get; set; }            // Prekių vertė šioje lokacijoje
        public decimal StockValueVAT { get; set; }         // Prekių vertė su PVM
        public string Currency { get; set; }               // Valiuta

        // Audito informacija
        public DateTime LastUpdated { get; set; }          // Paskutinio atnaujinimo data
        public string UpdatedByUser { get; set; }          // Kas atliko paskutinį atnaujinimą
        public string LastOperationType { get; set; }      // Paskutinės operacijos tipas

        public static string GetCsvHeader()
        {
            return "WarehouseId,Zone,Aisle,Rack,Shelf,Bin,Quantity,MaxCapacity," +
                   "UnitOfMeasure,MinimumQuantity,IsQuarantine,StorageConditions," +
                   "Temperature,Humidity,StockValue,StockValueVAT,Currency," +
                   "LastUpdated,UpdatedByUser,LastOperationType";
        }

        public string ToCsvLine()
        {
            return $"{WarehouseId}," +
                   $"\"{Zone}\"," +
                   $"\"{Aisle}\"," +
                   $"\"{Rack}\"," +
                   $"\"{Shelf}\"," +
                   $"\"{Bin}\"," +
                   $"{Quantity}," +
                   $"{MaxCapacity}," +
                   $"\"{UnitOfMeasure}\"," +
                   $"{MinimumQuantity}," +
                   $"{IsQuarantine}," +
                   $"\"{EscapeCsvField(StorageConditions)}\"," +
                   $"{Temperature}," +
                   $"{Humidity}," +
                   $"{StockValue}," +
                   $"{StockValueVAT}," +
                   $"{Currency}," +
                   $"{LastUpdated:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{EscapeCsvField(UpdatedByUser)}\"," +
                   $"\"{LastOperationType}\"";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }

    /// <summary>
    /// DTO klasė produkto judėjimo istorijai sekti
    /// </summary>
    public class ProductMovementDto
    {
        public int MovementId { get; set; }
        public int ProductId { get; set; }
        public string QRCodeId { get; set; }               // Jei judėjimas susijęs su QR kodu
        
        // Judėjimo informacija
        public string MovementType { get; set; }           // IN, OUT, TRANSFER
        public decimal Quantity { get; set; }
        public string UnitOfMeasure { get; set; }
        
        // Lokacijos
        public string SourceLocation { get; set; }         // Iš kur
        public string DestinationLocation { get; set; }    // Į kur
        
        // Operacijos detalės
        public DateTime MovementDate { get; set; }
        public string MovedByUser { get; set; }
        public string ReferenceNumber { get; set; }        // Susijusios operacijos numeris
        public string Notes { get; set; }

        public static string GetCsvHeader()
        {
            return "MovementId,ProductId,QRCodeId,MovementType,Quantity,UnitOfMeasure," +
                   "SourceLocation,DestinationLocation,MovementDate,MovedByUser," +
                   "ReferenceNumber,Notes";
        }

        public string ToCsvLine()
        {
            return $"{MovementId}," +
                   $"{ProductId}," +
                   $"{QRCodeId}," +
                   $"\"{MovementType}\"," +
                   $"{Quantity}," +
                   $"\"{UnitOfMeasure}\"," +
                   $"\"{EscapeCsvField(SourceLocation)}\"," +
                   $"\"{EscapeCsvField(DestinationLocation)}\"," +
                   $"{MovementDate:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{EscapeCsvField(MovedByUser)}\"," +
                   $"\"{ReferenceNumber}\"," +
                   $"\"{EscapeCsvField(Notes)}\"";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }
    }
}

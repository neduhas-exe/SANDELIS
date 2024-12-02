namespace Presentation.DTOs.Products
{
    /// <summary>
    /// Pagrindinis produkto DTO skirtas darbui su CSV
    /// </summary>
    public class ProductDto
    {
        // Unikalus produkto ID CSV faile
        public int Id { get; set; }

        // Pagrindinio pavadinimo laukai
        public string Name { get; set; }                    // Pagrindinis pavadinimas
        public string NameEn { get; set; }                  // Angliškas pavadinimas eksportui

        // Produkto kodų laukai
        public string EANCode { get; set; }                 // Tarptautinis barkodas
        public string LegacyCode { get; set; }              // Senasis sistemos kodas (0010006 ir pan.)
        public string Barcode { get; set; }                 // Vidinis barkodas jei skiriasi nuo EAN
        public string QRCode { get; set; }                  // Bendras produkto QR kodas

        // QR kodo tipai
        public bool HasBatchQR { get; set; }                // Ar naudoja partijai skirtą QR
        public bool HasUniqueItemQR { get; set; }           // Ar kiekvienas vienetas turi unikalų QR

        // QR susiejimas su produktu
        public List<string> BatchQRCodes { get; set; }      // Partijų QR kodų sąrašas
        public List<string> ItemQRCodes { get; set; }       // Individualių vienetų QR kodų sąrašas
        public DateTime QRGeneratedDate { get; set; }       // QR kodo sugeneravimo data

        // Aprašymo laukai
        public string Description { get; set; }             // Produkto aprašymas
        public string TechnicalSpecs { get; set; }          // Techninės charakteristikos

        // Fizinės savybės
        public decimal WeightGross { get; set; }            // Svoris su pakuote
        public decimal WeightNet { get; set; }              // Svoris be pakuotės
        public string UnitOfMeasure { get; set; }           // Matavimo vienetas (vnt, m, kg)

        // Klasifikatoriai
        public string Category { get; set; }                // Pagrindinė kategorija
        public string SubCategory { get; set; }             // Subkategorija
        public string Manufacturer { get; set; }            // Gamintojas

        // Sandėliavimo informacija
        public bool IsBatchProduct { get; set; }            // Ar tai yra birinis/matuojamas ilgiu produktas
        public decimal MinimumStock { get; set; }           // Minimalus kiekis sandėlyje

        // Kainos ir finansinė informacija
        public decimal PurchasePrice { get; set; }          // Pirkimo kaina be PVM
        public decimal PurchasePriceVAT { get; set; }       // Pirkimo kaina su PVM
        public decimal RetailPrice { get; set; }            // Pardavimo kaina be PVM
        public decimal RetailPriceVAT { get; set; }         // Pardavimo kaina su PVM
        public decimal WholesalePrice { get; set; }         // Didmeninė kaina be PVM
        public decimal WholesalePriceVAT { get; set; }      // Didmeninė kaina su PVM
        public decimal VATRate { get; set; }                // PVM tarifas procentais (pvz., 21)
        public string Currency { get; set; }                // Valiuta (EUR, USD)
        public decimal Margin { get; set; }                 // Marža procentais

        // Laiko žymės
        public DateTime CreatedAt { get; set; }             // Sukūrimo data
        public DateTime? UpdatedAt { get; set; }            // Paskutinio atnaujinimo data
        public DateTime? DiscontinuedAt { get; set; }       // Prekės išėmimo iš prekybos data

        // Būsenos
        public bool IsActive { get; set; }                  // Ar produktas aktyvus
        public bool IsDiscontinued { get; set; }            // Ar produktas nebegaminamas
        public string Status { get; set; }                  // Produkto būsena (Active, Discontinued, OutOfStock)

        // Susiję produktai
        public List<int> RelatedProductIds { get; set; }    // Susijusių produktų ID sąrašas
        public List<int> SubstituteProductIds { get; set; } // Pakaitinių produktų ID sąrašas

        // Kokybė ir atitiktis
        public List<string> Certificates { get; set; }      // Sertifikatų sąrašas (CE, RoHS ir t.t.)
        public string CountryOfOrigin { get; set; }         // Kilmės šalis
        public DateTime? ExpiryDate { get; set; }           // Galiojimo data
        public string QualityGrade { get; set; }            // Kokybės laipsnis

        // Konstruktorius
        public ProductDto()
        {
            BatchQRCodes = new List<string>();
            ItemQRCodes = new List<string>();
            VATRate = 21;                                   // Numatytasis PVM
            Currency = "EUR";                               // Numatytoji valiuta
            RelatedProductIds = new List<int>();
            SubstituteProductIds = new List<int>();
            Certificates = new List<string>();
            CreatedAt = DateTime.Now;
            IsActive = true;
        }

        // Metodai PVM skaičiavimams
        public decimal CalculateVAT(decimal price)
        {
            return price * (VATRate / 100);
        }

        public decimal CalculatePriceWithVAT(decimal priceWithoutVAT)
        {
            return priceWithoutVAT * (1 + VATRate / 100);
        }

        public decimal CalculateMargin()
        {
            if (PurchasePrice > 0)
            {
                return ((RetailPrice - PurchasePrice) / PurchasePrice) * 100;
            }
            return 0;
        }

        // Metodai darbui su CSV
        public static string GetCsvHeader()
        {
            return "Id,Name,NameEn,EANCode,LegacyCode,QRCode,HasBatchQR,HasUniqueItemQR," +
                   "Description,WeightNet,WeightGross,UnitOfMeasure,Category,SubCategory," +
                   "Manufacturer,IsBatchProduct,MinimumStock,PurchasePrice,PurchasePriceVAT," +
                   "RetailPrice,RetailPriceVAT,WholesalePrice,WholesalePriceVAT,VATRate," +
                   "Currency,Margin,CreatedAt,UpdatedAt,IsActive,Status,CountryOfOrigin";
        }

        public string ToCsvLine()
        {
            return $"{Id}," +
                   $"\"{EscapeCsvField(Name)}\"," +
                   $"\"{EscapeCsvField(NameEn)}\"," +
                   $"{EANCode}," +
                   $"{LegacyCode}," +
                   $"{QRCode}," +
                   $"{HasBatchQR}," +
                   $"{HasUniqueItemQR}," +
                   $"\"{EscapeCsvField(Description)}\"," +
                   $"{WeightNet}," +
                   $"{WeightGross}," +
                   $"{UnitOfMeasure}," +
                   $"\"{EscapeCsvField(Category)}\"," +
                   $"\"{EscapeCsvField(SubCategory)}\"," +
                   $"\"{EscapeCsvField(Manufacturer)}\"," +
                   $"{IsBatchProduct}," +
                   $"{MinimumStock}," +
                   $"{PurchasePrice}," +
                   $"{PurchasePriceVAT}," +
                   $"{RetailPrice}," +
                   $"{RetailPriceVAT}," +
                   $"{WholesalePrice}," +
                   $"{WholesalePriceVAT}," +
                   $"{VATRate}," +
                   $"{Currency}," +
                   $"{Margin}," +
                   $"{CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                   $"{(UpdatedAt.HasValue ? UpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "")}," +
                   $"{IsActive}," +
                   $"\"{Status}\"," +
                   $"\"{CountryOfOrigin}\"";
        }

        private string EscapeCsvField(string field)
        {
            if (string.IsNullOrEmpty(field)) return "";
            return field.Replace("\"", "\"\"");
        }

        private string ConvertQRListToCsv(List<string> qrCodes)
        {
            return string.Join("|", qrCodes);
        }
    }
}

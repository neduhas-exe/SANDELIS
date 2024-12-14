using System.ComponentModel.DataAnnotations;

namespace WarehouseSystem.Application.DTOs
{
    /// <summary>
    /// Produkto duomenų perdavimo objektas (DTO)
    /// Naudojamas duomenų apsikeitimui tarp kliento ir serverio
    /// </summary>
    public class ProductDto
    {
        /// <summary>
        /// Unikalus produkto identifikatorius
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Senasis produkto kodas (legacy)
        /// Gali būti nuo 6 iki 12 skaitmenų ilgio
        /// </summary>
        [Required(ErrorMessage = "Senasis produkto kodas yra privalomas")]
        [RegularExpression(@"^[0-9]{6,12}$", 
            ErrorMessage = "Senasis produkto kodas turi būti nuo 6 iki 12 skaitmenų")]
        public string LegacyCode { get; set; }

        /// <summary>
        /// Produkto pavadinimas
        /// </summary>
        [Required(ErrorMessage = "Produkto pavadinimas yra privalomas")]
        [StringLength(100, MinimumLength = 3, 
            ErrorMessage = "Pavadinimas turi būti nuo 3 iki 100 simbolių")]
        public string Name { get; set; }

        /// <summary>
        /// Produkto barkodas EAN-13 formatu
        /// </summary>
        [Required(ErrorMessage = "Barkodas yra privalomas")]
        [RegularExpression(@"^\d{13}$", 
            ErrorMessage = "Barkodas turi būti sudarytas iš 13 skaitmenų")]
        public string Barcode { get; set; }

        /// <summary>
        /// Produkto QR kodas
        /// Generuojamas automatiškai
        /// </summary>
        public string QRCode { get; set; }

        /// <summary>
        /// Išsamus produkto aprašymas
        /// </summary>
        [StringLength(500, ErrorMessage = "Aprašymas negali viršyti 500 simbolių")]
        public string Description { get; set; }

        /// <summary>
        /// Produkto kategorija
        /// </summary>
        [Required(ErrorMessage = "Kategorija yra privaloma")]
        [StringLength(50, ErrorMessage = "Kategorijos pavadinimas negali viršyti 50 simbolių")]
        public string Category { get; set; }

        /// <summary>
        /// Produkto subkategorija
        /// </summary>
        [StringLength(50, ErrorMessage = "Subkategorijos pavadinimas negali viršyti 50 simbolių")]
        public string SubCategory { get; set; }

        /// <summary>
        /// Pirkimo kaina be PVM
        /// </summary>
        [Required(ErrorMessage = "Pirkimo kaina be PVM yra privaloma")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Pirkimo kaina turi būti didesnė už 0")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", 
            ErrorMessage = "Kaina turi turėti ne daugiau kaip du skaičius po kablelio")]
        public decimal PurchasePriceExVAT { get; set; }

        /// <summary>
        /// Pardavimo kaina be PVM
        /// </summary>
        [Required(ErrorMessage = "Pardavimo kaina be PVM yra privaloma")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Pardavimo kaina turi būti didesnė už 0")]
        [RegularExpression(@"^\d+(\.\d{1,2})?$", 
            ErrorMessage = "Kaina turi turėti ne daugiau kaip du skaičius po kablelio")]
        public decimal SalePriceExVAT { get; set; }

        /// <summary>
        /// PVM tarifas procentais
        /// </summary>
        [Required(ErrorMessage = "PVM tarifas yra privalomas")]
        [Range(0, 100, ErrorMessage = "PVM tarifas turi būti tarp 0 ir 100")]
        public decimal VATRate { get; set; } = 21; // Numatytasis Lietuvos PVM tarifas

        /// <summary>
        /// Paskutinės pirkimo sąskaitos numeris
        /// </summary>
        [StringLength(20, ErrorMessage = "Sąskaitos numeris negali viršyti 20 simbolių")]
        public string LastInvoiceNumber { get; set; }

        /// <summary>
        /// Paskutinio pirkimo data
        /// </summary>
        public DateTime LastPurchaseDate { get; set; }

        /// <summary>
        /// Paskutinis tiekėjas
        /// </summary>
        [StringLength(100, ErrorMessage = "Tiekėjo pavadinimas negali viršyti 100 simbolių")]
        public string LastPurchaseSupplier { get; set; }

        /// <summary>
        /// Prekių priėmėjas
        /// </summary>
        [StringLength(50, ErrorMessage = "Priėmėjo vardas negali viršyti 50 simbolių")]
        public string LastReceivedBy { get; set; }

        /// <summary>
        /// Esamas kiekis sandėlyje
        /// </summary>
        [Required(ErrorMessage = "Kiekis sandėlyje yra privalomas")]
        [Range(0, int.MaxValue, ErrorMessage = "Kiekis negali būti neigiamas")]
        public int QuantityInStock { get; set; }

        /// <summary>
        /// Minimalus kiekis sandėlyje
        /// </summary>
        [Required(ErrorMessage = "Minimalus kiekis sandėlyje yra privalomas")]
        [Range(0, int.MaxValue, ErrorMessage = "Minimalus kiekis negali būti neigiamas")]
        public int MinimumStockLevel { get; set; }

        /// <summary>
        /// Paskutinio papildymo data
        /// </summary>
        public DateTime LastRestockDate { get; set; }

        /// <summary>
        /// Pagrindinio tiekėjo ID
        /// </summary>
        [Required(ErrorMessage = "Tiekėjo ID yra privalomas")]
        public string SupplierID { get; set; }

        /// <summary>
        /// Įrašo kūrėjas
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Įrašo sukūrimo data
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Paskutinis redaguotojas
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Paskutinio redagavimo data
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// Produkto būsena
        /// </summary>
        [Required(ErrorMessage = "Produkto būsena yra privaloma")]
        public string Status { get; set; }

        #region Apskaičiuojami laukai

        /// <summary>
        /// Pardavimo kaina su PVM
        /// </summary>
        public decimal SalePriceWithVAT => Math.Round(SalePriceExVAT * (1 + (VATRate / 100)), 2);

        /// <summary>
        /// Pirkimo kaina su PVM
        /// </summary>
        public decimal PurchasePriceWithVAT => Math.Round(PurchasePriceExVAT * (1 + (VATRate / 100)), 2);

        /// <summary>
        /// Pelno marža procentais
        /// </summary>
        public decimal ProfitMarginPercentage => Math.Round(((SalePriceExVAT - PurchasePriceExVAT) / PurchasePriceExVAT) * 100, 2);

        /// <summary>
        /// Ar reikia papildyti sandėlį
        /// </summary>
        public bool NeedsRestock => QuantityInStock <= MinimumStockLevel;

        /// <summary>
        /// Kiek vienetų trūksta iki minimalaus kiekio
        /// </summary>
        public int UnitsToRestock => NeedsRestock ? MinimumStockLevel - QuantityInStock : 0;

        #endregion
    }
}

using Domain.Enums;

namespace Domain.Models;

//TODO: Change SubCategories from string to enum;
//TODO: Add Customer model, Site model;
//TODO: Add controller with Get (list)/Get (by ID)/Post (create) methods for Customer and Site models;
//TODO: Add Services and Repositories for both Site and Customer models, based on ProductService/Repository;
//TODO: Add validators with FluentValidations for each model;
//TODO: Add exception handling middleware;

public class Product
{
    /// <summary>
    /// PVM tarifas procentais
    /// </summary>
    private decimal VATRate { get; set; } = 21; // Numatytasis Lietuvos PVM tarifas

    /// <summary>
    /// Unikalus produkto identifikatorius
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Senasis produkto kodas (legacy)
    /// Gali būti nuo 6 iki 12 skaitmenų ilgio
    /// </summary>
    public string LegacyCode { get; set; }

    /// <summary>
    /// Produkto pavadinimas
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Produkto barkodas EAN-13 formatu
    /// </summary>
    public string Barcode { get; set; }

    /// <summary>
    /// Produkto QR kodas
    /// Generuojamas automatiškai
    /// </summary>
    public string QRCode { get; set; }

    /// <summary>
    /// Išsamus produkto aprašymas
    /// </summary>
    public string Description { get; set; }

    /// <summary>
    /// Produkto kategorija
    /// </summary>
    public Categories Category { get; set; }

    /// <summary>
    /// Produkto subkategorija
    /// </summary>
    public string SubCategory { get; set; }

    /// <summary>
    /// Pirkimo kaina be PVM
    /// </summary>
    public decimal PurchasePriceExVAT { get; set; }

    /// <summary>
    /// Pardavimo kaina be PVM
    /// </summary>
    public decimal SalePriceExVAT { get; set; }

    /// <summary>
    /// Paskutinės pirkimo sąskaitos numeris
    /// </summary>
    public string LastInvoiceNumber { get; set; }

    /// <summary>
    /// Paskutinio pirkimo data
    /// </summary>
    public DateTime LastPurchaseDate { get; set; }

    /// <summary>
    /// Paskutinis tiekėjas
    /// </summary>
    public string LastPurchaseSupplier { get; set; }

    /// <summary>
    /// Prekių priėmėjas
    /// </summary>
    public string LastReceivedBy { get; set; }

    /// <summary>
    /// Esamas kiekis sandėlyje
    /// </summary>
    public int QuantityInStock { get; set; }

    /// <summary>
    /// Minimalus kiekis sandėlyje
    /// </summary>
    public int MinimumStockLevel { get; set; }

    /// <summary>
    /// Paskutinio papildymo data
    /// </summary>
    public DateTime LastRestockDate { get; set; }

    /// <summary>
    /// Pagrindinio tiekėjo ID
    /// </summary>
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
    public string Status { get; set; }

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
}

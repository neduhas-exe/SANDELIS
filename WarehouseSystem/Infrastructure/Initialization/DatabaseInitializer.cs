// WarehouseSystem/Infrastructure/Initialization/DatabaseInitializer.cs
using CsvHelper;
using Domain.Models;
using Domain.Enums;
using Infrastructure.Config;
using System.Globalization;

namespace Infrastructure.Initialization
{
    public static class DatabaseInitializer
    {
        private static readonly DateTime CurrentTime = DateTime.UtcNow;

        public static void InitializeDatabase()
        {
            InitializeCustomers();
            InitializeSites();
            InitializeUsers();
            InitializeProducts();
        }

        private static void InitializeCustomers()
        {
            var filePath = CsvConfig.Paths.Customers;
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                var sampleCustomers = new List<Customer>
                {
                    new Customer
                    {
                        Id = 1,
                        CustomerType = CustomerType.Company,
                        Name = "UAB \"Statybų Ekspertai\"",
                        CompanyCode = "123456789",
                        VATCode = "LT123456789",
                        LegalAddress = "Verkių g. 1, Vilnius",
                        ContactPersonName = "Jonas Jonaitis",
                        ContactEmail = "jonas@statybuekspertai.lt",
                        ContactPhone = "+37061234567",
                        CreditLimit = 10000.00m,
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedDate = CurrentTime,
                        ModifiedBy = "system",
                        ModifiedDate = CurrentTime
                    },
                    new Customer
                    {
                        Id = 2,
                        CustomerType = CustomerType.Individual,
                        Name = "Petras Petraitis",
                        CompanyCode = null,
                        VATCode = null,
                        LegalAddress = "Gedimino pr. 15, Vilnius",
                        ContactPersonName = "Petras Petraitis",
                        ContactEmail = "petras@gmail.com",
                        ContactPhone = "+37062345678",
                        CreditLimit = 1000.00m,
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedDate = CurrentTime,
                        ModifiedBy = "system",
                        ModifiedDate = CurrentTime
                    }
                };

                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteHeader<Customer>();
                csv.NextRecord();
                csv.WriteRecords(sampleCustomers);
            }
        }

        private static void InitializeSites()
        {
            var filePath = CsvConfig.Paths.Sites;
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                var sampleSites = new List<Site>
                {
                    new Site
                    {
                        Id = 1,
                        CustomerId = 1,
                        Name = "Statybvietė Vilnius",
                        Address = "Kalvarijų g. 100, Vilnius",
                        ContactPerson = "Marius Mariukas",
                        ContactPhone = "+37061111111",
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedDate = CurrentTime,
                        ModifiedBy = "system",
                        ModifiedDate = CurrentTime
                    },
                    new Site
                    {
                        Id = 2,
                        CustomerId = 1,
                        Name = "Statybvietė Kaunas",
                        Address = "Savanorių pr. 50, Kaunas",
                        ContactPerson = "Tomas Tomukas",
                        ContactPhone = "+37062222222",
                        IsActive = true,
                        CreatedBy = "system",
                        CreatedDate = CurrentTime,
                        ModifiedBy = "system",
                        ModifiedDate = CurrentTime
                    }
                };

                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteHeader<Site>();
                csv.NextRecord();
                csv.WriteRecords(sampleSites);
            }
        }

        private static void InitializeUsers()
        {
            var filePath = CsvConfig.Paths.Users;
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                var sampleUsers = new List<User>
                {
                    new User
                    {
                        Id = 1,
                        UserName = "admin",
                        FirstName = "Administratorius",
                        LastName = "Sisteminis",
                        Email = "admin@sandelis.lt",
                        IsActive = true,
                        CreatedDate = CurrentTime,
                        LastLoginDate = CurrentTime
                    },
                    new User
                    {
                        Id = 2,
                        UserName = "sandelininkas",
                        FirstName = "Jonas",
                        LastName = "Sandėlininkas",
                        Email = "jonas@sandelis.lt",
                        IsActive = true,
                        CreatedDate = CurrentTime,
                        LastLoginDate = CurrentTime
                    }
                };

                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteHeader<User>();
                csv.NextRecord();
                csv.WriteRecords(sampleUsers);
            }
        }

        private static void InitializeProducts()
        {
            var filePath = CsvConfig.Paths.Products;
            if (!File.Exists(filePath) || new FileInfo(filePath).Length == 0)
            {
                var sampleProducts = new List<Product>
                {
                    new Product
                    {
                        Id = 1,
                        LegacyCode = "123456",
                        Name = "Elektrinis Perforatorius PRO-X",
                        Barcode = "4771234567890",
                        QRCode = "PRO-X-001",
                        Description = "Profesionalus elektrinis perforatorius su SDS+ sistema",
                        Category = Categories.ElectricTools,
                        SubCategory = SubCategories.PowerTools,
                        PurchasePriceExVAT = 250.00m,
                        SalePriceExVAT = 375.00m,
                        LastInvoiceNumber = "INV-2024-001",
                        LastPurchaseDate = CurrentTime.AddDays(-30),
                        LastPurchaseSupplier = "UAB Įrankių Tiekėjas",
                        LastReceivedBy = "Jonas Sandėlininkas",
                        QuantityInStock = 15,
                        MinimumStockLevel = 5,
                        LastRestockDate = CurrentTime.AddDays(-30),
                        SupplierID = "SUPP001",
                        CreatedBy = "system",
                        CreatedDate = CurrentTime,
                        ModifiedBy = "system",
                        ModifiedDate = CurrentTime,
                        Status = "Active"
                    },
                    new Product
                    {
                        Id = 2,
                        LegacyCode = "234567",
                        Name = "LED Prožektorius 50W",
                        Barcode = "4771234567891",
                        QRCode = "LED-50W-001",
                        Description = "Profesionalus LED prožektorius, 50W, IP65",
                        Category = Categories.Lighting,
                        SubCategory = SubCategories.OutdoorLighting,
                        PurchasePriceExVAT = 45.00m,
                        SalePriceExVAT = 89.00m,
                        LastInvoiceNumber = "INV-2024-002",
                        LastPurchaseDate = CurrentTime.AddDays(-15),
                        LastPurchaseSupplier = "UAB Šviesos Sprendimai",
                        LastReceivedBy = "Jonas Sandėlininkas",
                        QuantityInStock = 30,
                        MinimumStockLevel = 10,
                        LastRestockDate = CurrentTime.AddDays(-15),
                        SupplierID = "SUPP002",
                        CreatedBy = "system",
                        CreatedDate = CurrentTime,
                        ModifiedBy = "system",
                        ModifiedDate = CurrentTime,
                        Status = "Active"
                    }
                };

                using var writer = new StreamWriter(filePath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteHeader<Product>();
                csv.NextRecord();
                csv.WriteRecords(sampleProducts);
            }
        }
    }
}
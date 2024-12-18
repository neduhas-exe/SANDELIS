// WarehouseSystem/Infrastructure/Repositories/ProductsRepository.cs
using CsvHelper;
using Domain.Models;
using Infrastructure.Config;
using Infrastructure.Repositories.Interfaces;
using System.Globalization;

namespace Infrastructure.Repositories
{
    public class ProductsRepository : IProductsRepository
    {
        private readonly string _filePath;

        public ProductsRepository()
        {
            _filePath = CsvConfig.Paths.Products;
            InitializeFile();
        }

        private void InitializeFile()
        {
            if (new FileInfo(_filePath).Length == 0)
            {
                using var writer = new StreamWriter(_filePath);
                using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
                csv.WriteHeader<Product>();
                csv.NextRecord();
            }
        }

        public Product Get(long id)
        {
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                var products = csv.GetRecords<Product>().ToList();

                return products.FirstOrDefault(product => product.Id == id);
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                InitializeFile();
                return null;
            }
        }

        public List<Product> List()
        {
            try
            {
                using var reader = new StreamReader(_filePath);
                using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
                return csv.GetRecords<Product>().ToList();
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                InitializeFile();
                return new List<Product>();
            }
        }

        public Product Create(Product product)
        {
            var products = List();
            products.Add(product);

            using var writer = new StreamWriter(_filePath);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            csv.WriteHeader<Product>();
            csv.NextRecord();
            csv.WriteRecords(products);

            return product;
        }
    }
}
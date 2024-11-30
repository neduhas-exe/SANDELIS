using CsvHelper;
using Domain.Models;
using Infrastructure.Repositories.Interfaces;
using System.Globalization;

namespace Infrastructure.Repositories
{
    //CSV DB repository. Later should be replaced with a regular DB.
    public class ProductsRepository : IProductsRepository
    {
        private const string _filePath = "C:/Test/products.csv";

        public Product Get(long id)
        {
            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var products = csv.GetRecords<Product>().ToList();

            return products.FirstOrDefault(product => product.Id == id);
        }

        public List<Product> List()
        {
            using var reader = new StreamReader(_filePath);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var products = csv.GetRecords<Product>().ToList();

            return products;
        }

        public Product Create(Product product)
        {
            using var writer = new StreamWriter(_filePath, append: true);
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            if (new FileInfo(_filePath).Length == 0)
            {
                csv.WriteHeader<Product>();
                csv.NextRecord();
            }

            csv.WriteRecord(product);
            csv.NextRecord();

            return product;
        }
    }
}

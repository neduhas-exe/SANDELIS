using CsvHelper;
using Domain.Models;
using Infrastructure.Repositories.Interfaces;
using System.Globalization;

namespace Infrastructure.Repositories;

public class CustomersRepository : ICustomersRepository
{
    private const string _filePath = "C:\\Test\\customers.csv";

    public Customer Get(long id)
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var customers = csv.GetRecords<Customer>().ToList();

        return customers.FirstOrDefault(customer => customer.Id == id);
    }

    public List<Customer> List()
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var customers = csv.GetRecords<Customer>().ToList();

        return customers;
    }

    public Customer Create(Customer customer)
    {
        using var writer = new StreamWriter(_filePath, append: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        if (new FileInfo(_filePath).Length == 0)
        {
            csv.WriteHeader<Customer>();
            csv.NextRecord();
        }

        csv.WriteRecord(customer);
        csv.NextRecord();

        return customer;
    }
}

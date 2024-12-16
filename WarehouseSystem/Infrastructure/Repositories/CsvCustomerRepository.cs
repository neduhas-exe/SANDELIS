// Path: WarehouseSystem/Infrastructure/Repositories/CsvCustomerRepository.cs
public class CsvCustomerRepository : ICustomerRepository
{
    private readonly CsvFileService _csvService;
    private const string FileName = "customers.csv";
    private long _lastId = 0;

    public CsvCustomerRepository(CsvFileService csvService)
    {
        _csvService = csvService;
        // Nustatome paskutinį ID iš esamų įrašų
        var customers = _csvService.ReadCsv<Customer>(FileName);
        if (customers.Any())
            _lastId = customers.Max(c => c.Id);
    }

    public Customer Get(long id)
    {
        var customers = _csvService.ReadCsv<Customer>(FileName);
        return customers.FirstOrDefault(c => c.Id == id);
    }

    public List<Customer> List()
    {
        return _csvService.ReadCsv<Customer>(FileName);
    }

    public Customer Create(Customer customer)
    {
        var customers = _csvService.ReadCsv<Customer>(FileName);
        customer.Id = ++_lastId;
        customers.Add(customer);
        _csvService.WriteCsv(FileName, customers);
        return customer;
    }
}


// Path: WarehouseSystem/Infrastructure/Services/CsvFileService.cs
using System.Globalization;
using CsvHelper;

public class CsvFileService
{
    private readonly string _basePath;

    public CsvFileService(string basePath = "Data/CSV")
    {
        _basePath = basePath;
        // Sukuriame Data/CSV direktoriją jei jos nėra
        Directory.CreateDirectory(_basePath);
    }

    public List<T> ReadCsv<T>(string fileName)
    {
        var path = Path.Combine(_basePath, fileName);
        
        if (!File.Exists(path))
            return new List<T>();

        using var reader = new StreamReader(path);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<T>().ToList();
    }

    public void WriteCsv<T>(string fileName, IEnumerable<T> records)
    {
        var path = Path.Combine(_basePath, fileName);
        using var writer = new StreamWriter(path);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
    }
}

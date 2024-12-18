// Path: WarehouseSystem/Infrastructure/Services/CsvFileService.cs
using CsvHelper;
using System.Globalization;

public class CsvFileService
{
    private readonly string _basePath;

    public CsvFileService(string basePath = "Data")
    {
        _basePath = basePath;
    }

    public List<T> ReadCsv<T>(string fileName)
    {
        var filePath = Path.Combine(_basePath, fileName);

        if (!File.Exists(filePath))
            return new List<T>();

        using var reader = new StreamReader(filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        return csv.GetRecords<T>().ToList();
    }

    public void WriteCsv<T>(string fileName, List<T> records)
    {
        var filePath = Path.Combine(_basePath, fileName);

        // Ensure directory exists
        Directory.CreateDirectory(_basePath);

        using var writer = new StreamWriter(filePath);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);
        csv.WriteRecords(records);
    }
}
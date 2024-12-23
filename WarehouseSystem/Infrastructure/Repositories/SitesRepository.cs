using CsvHelper;
using Domain.Models;
using Infrastructure.Repositories.Interfaces;
using System.Globalization;

namespace Infrastructure.Repositories;

public class SitesRepository : ISitesRepository
{
    private const string _filePath = "C:\\Test\\sites.csv";

    public Site Get(long id)
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var sites = csv.GetRecords<Site>().ToList();

        return sites.FirstOrDefault(site => site.Id == id);
    }

    public List<Site> List()
    {
        using var reader = new StreamReader(_filePath);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        var sites = csv.GetRecords<Site>().ToList();

        return sites;
    }

    public Site Create(Site site)
    {
        using var writer = new StreamWriter(_filePath, append: true);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        if (new FileInfo(_filePath).Length == 0)
        {
            csv.WriteHeader<Site>();
            csv.NextRecord();
        }

        csv.WriteRecord(site);
        csv.NextRecord();

        return site;
    }
}
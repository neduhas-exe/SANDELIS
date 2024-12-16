// Path: WarehouseSystem/Infrastructure/Repositories/CsvSiteRepository.cs
public class CsvSiteRepository : ISiteRepository
{
    private readonly CsvFileService _csvService;
    private const string FileName = "sites.csv";
    private long _lastId = 0;

    public CsvSiteRepository(CsvFileService csvService)
    {
        _csvService = csvService;
        var sites = _csvService.ReadCsv<Site>(FileName);
        if (sites.Any())
            _lastId = sites.Max(s => s.Id);
    }

    public Site Get(long id)
    {
        var sites = _csvService.ReadCsv<Site>(FileName);
        return sites.FirstOrDefault(s => s.Id == id);
    }

    public List<Site> List()
    {
        return _csvService.ReadCsv<Site>(FileName);
    }

    public Site Create(Site site)
    {
        var sites = _csvService.ReadCsv<Site>(FileName);
        site.Id = ++_lastId;
        sites.Add(site);
        _csvService.WriteCsv(FileName, sites);
        return site;
    }
}

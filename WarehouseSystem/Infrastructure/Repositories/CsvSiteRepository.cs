using Domain.Models;
using Infrastructure.Repositories.Interfaces;

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

    public Site Update(Site site)
    {
        var sites = _csvService.ReadCsv<Site>(FileName);
        var existingSite = sites.FirstOrDefault(s => s.Id == site.Id);

        if (existingSite != null)
        {
            sites.Remove(existingSite);
            sites.Add(site);
            sites = sites.OrderBy(s => s.Id).ToList();
            _csvService.WriteCsv(FileName, sites);
        }

        return site;
    }

    public List<Site> GetByCustomerId(long customerId)
    {
        var sites = _csvService.ReadCsv<Site>(FileName);
        return sites.Where(s => s.CustomerId == customerId).ToList();
    }

    public List<Site> Search(
        long? customerId = null,
        string name = null,
        string address = null,
        bool? isActive = null,
        bool? hasComments = null)
    {
        var sites = _csvService.ReadCsv<Site>(FileName);
        var query = sites.AsQueryable();

        if (customerId.HasValue)
            query = query.Where(s => s.CustomerId == customerId.Value);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(s => s.Name.Contains(name, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(address))
            query = query.Where(s => s.Address.Contains(address, StringComparison.OrdinalIgnoreCase));

        if (isActive.HasValue)
            query = query.Where(s => s.IsActive == isActive.Value);

        if (hasComments.HasValue)
            query = query.Where(s => hasComments.Value ?
                !string.IsNullOrWhiteSpace(s.Comments) :
                string.IsNullOrWhiteSpace(s.Comments));

        return query.ToList();
    }

    public List<Site> GetSitesWithProductTotals()
    {
        var sites = _csvService.ReadCsv<Site>(FileName);
        return sites.Where(s =>
            s.TotalProductCount.HasValue &&
            s.TotalProductValue.HasValue)
            .ToList();
    }

    public List<Site> GetRecentlyModified(DateTime since)
    {
        var sites = _csvService.ReadCsv<Site>(FileName);
        return sites.Where(s =>
            s.ModifiedDate.HasValue &&
            s.ModifiedDate.Value >= since)
            .OrderByDescending(s => s.ModifiedDate)
            .ToList();
    }

    public List<Site> GetSitesWithRecentComments(DateTime since)
    {
        var sites = _csvService.ReadCsv<Site>(FileName);
        return sites.Where(s =>
            s.LastCommentDate.HasValue &&
            s.LastCommentDate.Value >= since)
            .OrderByDescending(s => s.LastCommentDate)
            .ToList();
    }

    public bool ValidateCustomerSite(long customerId, long siteId)
    {
        var site = Get(siteId);
        return site != null && site.CustomerId == customerId;
    }
}
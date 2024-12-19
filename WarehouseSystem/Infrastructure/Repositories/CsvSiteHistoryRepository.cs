// Infrastructure/Repositories/CsvSiteHistoryRepository.cs
using Domain.Models;
using Infrastructure.Repositories.Interfaces;

namespace Infrastructure.Repositories;

public class CsvSiteHistoryRepository : ISiteHistoryRepository
{
    private readonly CsvFileService _csvService;
    private const string FileName = "site_history.csv";
    private long _lastId = 0;

    public CsvSiteHistoryRepository(CsvFileService csvService)
    {
        _csvService = csvService;
        var history = _csvService.ReadCsv<SiteHistory>(FileName);
        if (history.Any())
            _lastId = history.Max(h => h.Id);
    }

    public List<SiteHistory> GetBySiteId(long siteId)
    {
        var history = _csvService.ReadCsv<SiteHistory>(FileName);
        return history.Where(h => h.SiteId == siteId)
                     .OrderByDescending(h => h.ChangeDate)
                     .ToList();
    }

    public SiteHistory Create(SiteHistory history)
    {
        var allHistory = _csvService.ReadCsv<SiteHistory>(FileName);
        history.Id = ++_lastId;
        allHistory.Add(history);
        _csvService.WriteCsv(FileName, allHistory);
        return history;
    }

    public List<SiteHistory> GetRecentHistory(DateTime since)
    {
        var history = _csvService.ReadCsv<SiteHistory>(FileName);
        return history.Where(h => h.ChangeDate >= since)
                     .OrderByDescending(h => h.ChangeDate)
                     .ToList();
    }

    public List<SiteHistory> GetByUser(string userName)
    {
        var history = _csvService.ReadCsv<SiteHistory>(FileName);
        return history.Where(h => h.UserName == userName)
                     .OrderByDescending(h => h.ChangeDate)
                     .ToList();
    }

    public List<SiteHistory> GetByChangeType(string changeType)
    {
        var history = _csvService.ReadCsv<SiteHistory>(FileName);
        return history.Where(h => h.ChangeType == changeType)
                     .OrderByDescending(h => h.ChangeDate)
                     .ToList();
    }
}
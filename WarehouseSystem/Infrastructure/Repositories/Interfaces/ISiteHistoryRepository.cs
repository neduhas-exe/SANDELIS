// Infrastructure/Repositories/Interfaces/ISiteHistoryRepository.cs
using Domain.Models;

namespace Infrastructure.Repositories.Interfaces;

public interface ISiteHistoryRepository
{
    List<SiteHistory> GetBySiteId(long siteId);
    SiteHistory Create(SiteHistory history);
    List<SiteHistory> GetRecentHistory(DateTime since);
    List<SiteHistory> GetByUser(string userName);
    List<SiteHistory> GetByChangeType(string changeType);
}
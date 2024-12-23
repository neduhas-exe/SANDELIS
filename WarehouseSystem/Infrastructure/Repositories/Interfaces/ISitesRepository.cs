using Domain.Models;

namespace Infrastructure.Repositories.Interfaces;

public interface ISitesRepository
{
    public Site Get(long id);
    public List<Site> List();
    public Site Create(Site site);
}
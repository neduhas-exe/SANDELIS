using Domain.Models;

namespace Infrastructure.Repositories.Interfaces
{
    public interface ISiteRepository
    {
        Site Get(long id);
        List<Site> List();
        Site Create(Site site);
    }
}

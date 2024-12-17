using Domain.Models;

public interface ISitesService
{
    Site Get(long id);
    List<Site> List();
    Site Create(Site site);
}
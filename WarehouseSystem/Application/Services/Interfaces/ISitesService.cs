using Domain.Models;

namespace Application.Services.Interfaces;

public interface ISitesService
{
    public Site Get(long id);
    public List<Site> List();
    public Site Create(Site site);
}
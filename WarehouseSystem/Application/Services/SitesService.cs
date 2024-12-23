using Application.Services.Interfaces;
using Domain.Models;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services;

public class SitesService(ISitesRepository sitesRepository) : ISitesService
{
    private readonly ISitesRepository _sitesRepository = sitesRepository;

    public Site Get(long id) => _sitesRepository.Get(id);

    public List<Site> List() => _sitesRepository.List();

    public Site Create(Site site) => _sitesRepository.Create(site);
}

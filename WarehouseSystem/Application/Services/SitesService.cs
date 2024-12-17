using Application.Services.Interfaces;
using Domain.Models;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services
{
    public class SitesService : ISitesService
    {
        private readonly ISiteRepository _siteRepository;

        public SitesService(ISiteRepository siteRepository)
        {
            _siteRepository = siteRepository;
        }

        public Site Get(long id)
        {
            return _siteRepository.Get(id);
        }

        public List<Site> List()
        {
            return _siteRepository.List();
        }

        public Site Create(Site site)
        {
            return _siteRepository.Create(site);
        }

        // Remove the duplicate interface implementations
        // These were causing conflicts:
        // Site ISitesService.Get(long id)
        // List<Site> ISitesService.List()
    }
}
// Path: WarehouseSystem/Application/Services/SitesService.cs
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
    }
}

using Domain.Models;

public interface ISiteRepository
{
    /// <summary>
    /// Gauti site pagal ID
    /// </summary>
    Site Get(long id);

    /// <summary>
    /// Gauti visø sites sàraðà
    /// </summary>
    List<Site> List();

    /// <summary>
    /// Sukurti naujà site
    /// </summary>
    Site Create(Site site);

    /// <summary>
    /// Atnaujinti esamà site
    /// </summary>
    Site Update(Site site);

    /// <summary>
    /// Gauti sites pagal customer ID
    /// </summary>
    List<Site> GetByCustomerId(long customerId);

    /// <summary>
    /// Ieðkoti sites pagal ávairius kriterijus
    /// </summary>
    List<Site> Search(
        long? customerId = null,
        string name = null,
        string address = null,
        bool? isActive = null,
        bool? hasComments = null);

    /// <summary>
    /// Gauti sites su produktø suma ir kiekiu
    /// </summary>
    List<Site> GetSitesWithProductTotals();

    /// <summary>
    /// Gauti sites pagal modifikavimo datà
    /// </summary>
    List<Site> GetRecentlyModified(DateTime since);

    /// <summary>
    /// Gauti sites pagal paskutiná komentarà
    /// </summary>
    List<Site> GetSitesWithRecentComments(DateTime since);

    /// <summary>
    /// Patikrinti ar site priklauso customer
    /// </summary>
    bool ValidateCustomerSite(long customerId, long siteId);
}
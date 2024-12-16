namespace Application.Services.Interfaces
{
    /// <summary>
    /// Sąsaja apibrėžianti objektų serviso funkcionalumą
    /// </summary>
    public interface ISitesService
    {
        /// <summary>
        /// Gauti objektą pagal ID
        /// </summary>
        /// <param name="id">Objekto ID</param>
        /// <returns>Objekto duomenys</returns>
        Site Get(long id);

        /// <summary>
        /// Gauti visų objektų sąrašą
        /// </summary>
        /// <returns>Objektų sąrašas</returns>
        List<Site> List();

        /// <summary>
        /// Sukurti naują objektą
        /// </summary>
        /// <param name="site">Naujo objekto duomenys</param>
        /// <returns>Sukurtas objekto duomenys</returns>
        Site Create(Site site);
    }
}

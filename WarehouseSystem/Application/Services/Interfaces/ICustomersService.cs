// Path: WarehouseSystem/Application/Services/Interfaces/ICustomersService.cs

namespace Application.Services.Interfaces
{
    /// <summary>
    /// Sąsaja apibrėžianti klientų serviso funkcionalumą
    /// </summary>
    public interface ICustomersService
    {
        /// <summary>
        /// Gauti klientą pagal ID
        /// </summary>
        /// <param name="id">Kliento ID</param>
        /// <returns>Kliento objektas</returns>
        Customer Get(long id);

        /// <summary>
        /// Gauti visų klientų sąrašą
        /// </summary>
        /// <returns>Klientų sąrašas</returns>
        List<Customer> List();

        /// <summary>
        /// Sukurti naują klientą
        /// </summary>
        /// <param name="customer">Naujo kliento duomenys</param>
        /// <returns>Sukurtas kliento objektas</returns>
        Customer Create(Customer customer);
    }
}

using WarehouseSystem.Domain.Models;

namespace WarehouseSystem.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Klientų objektų repozitorijos interfeisas
    /// </summary>
    public interface ICustomerObjectsRepository
    {
        /// <summary>
        /// Gauti visus objektus
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetAllAsync();

        /// <summary>
        /// Gauti objektą pagal ID
        /// </summary>
        Task<CustomerObject> GetByIdAsync(long id);

        /// <summary>
        /// Gauti visus kliento objektus
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetByCustomerIdAsync(long customerId);

        /// <summary>
        /// Sukurti naują objektą
        /// </summary>
        Task<CustomerObject> CreateAsync(CustomerObject customerObject);

        /// <summary>
        /// Atnaujinti objekto informaciją
        /// </summary>
        Task<CustomerObject> UpdateAsync(CustomerObject customerObject);

        /// <summary>
        /// Ištrinti objektą
        /// </summary>
        Task DeleteAsync(long id);

        /// <summary>
        /// Gauti objektus pagal tipą
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetByTypeAsync(string objectType);

        /// <summary>
        /// Gauti objektus pagal statusą
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetByStatusAsync(string status);

        /// <summary>
        /// Gauti objektus pagal projekto fazę
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetByPhaseAsync(string phase);

        /// <summary>
        /// Gauti objektus pagal miestą
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetByCityAsync(string city);

        /// <summary>
        /// Patikrinti ar objektas priklauso klientui
        /// </summary>
        Task<bool> BelongsToCustomerAsync(long objectId, long customerId);

        /// <summary>
        /// Gauti objektus pagal pašto kodą
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetByPostalCodeAsync(string postalCode);

        /// <summary>
        /// Atnaujinti kontaktinę informaciją
        /// </summary>
        Task UpdateContactInfoAsync(long objectId, string contactPerson, string contactPhone);

        /// <summary>
        /// Išsaugoti CSV failo pakeitimus
        /// </summary>
        Task SaveChangesToCsvAsync();
    }
}

using WarehouseSystem.Domain.Models;

namespace WarehouseSystem.Application.Services.Interfaces
{
    /// <summary>
    /// Klientų objektų serviso interfeisas
    /// </summary>
    public interface ICustomerObjectsService
    {
        /// <summary>
        /// Gauti visus objektus
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetAllObjectsAsync();

        /// <summary>
        /// Gauti objektą pagal ID
        /// </summary>
        Task<CustomerObject> GetObjectAsync(long id);

        /// <summary>
        /// Gauti visus kliento objektus
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetCustomerObjectsAsync(long customerId);

        /// <summary>
        /// Sukurti naują objektą
        /// </summary>
        Task<CustomerObject> CreateObjectAsync(CustomerObject customerObject);

        /// <summary>
        /// Atnaujinti objekto informaciją
        /// </summary>
        Task<CustomerObject> UpdateObjectAsync(CustomerObject customerObject);

        /// <summary>
        /// Ištrinti objektą
        /// </summary>
        Task DeleteObjectAsync(long id);

        /// <summary>
        /// Gauti objektus pagal tipą
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetObjectsByTypeAsync(string objectType);

        /// <summary>
        /// Gauti objektus pagal projekto fazę
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetObjectsByPhaseAsync(string phase);

        /// <summary>
        /// Gauti aktyvius objektus
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetActiveObjectsAsync();

        /// <summary>
        /// Gauti objektus pagal miestą
        /// </summary>
        Task<IEnumerable<CustomerObject>> GetObjectsByCityAsync(string city);

        /// <summary>
        /// Atnaujinti objekto statusą
        /// </summary>
        Task UpdateObjectStatusAsync(long objectId, string status);

        /// <summary>
        /// Atnaujinti objekto projekto fazę
        /// </summary>
        Task UpdateObjectPhaseAsync(long objectId, string phase);

        /// <summary>
        /// Pridėti pastabą prie objekto
        /// </summary>
        Task AddObjectNoteAsync(long objectId, string note);
    }
}

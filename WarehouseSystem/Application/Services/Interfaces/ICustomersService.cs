using WarehouseSystem.Domain.Models;

namespace WarehouseSystem.Application.Services.Interfaces
{
    /// <summary>
    /// Klientų serviso interfeisas
    /// </summary>
    public interface ICustomersService
    {
        /// <summary>
        /// Gauti visus klientus
        /// </summary>
        Task<IEnumerable<Customer>> GetAllAsync();

        /// <summary>
        /// Gauti klientą pagal ID
        /// </summary>
        Task<Customer> GetAsync(long id);

        /// <summary>
        /// Sukurti naują klientą
        /// </summary>
        Task<Customer> CreateAsync(Customer customer);

        /// <summary>
        /// Atnaujinti kliento informaciją
        /// </summary>
        Task<Customer> UpdateAsync(Customer customer);

        /// <summary>
        /// Ištrinti klientą
        /// </summary>
        Task DeleteAsync(long id);

        /// <summary>
        /// Gauti tik įmonių tipo klientus
        /// </summary>
        Task<IEnumerable<Customer>> GetCompanieAsync();

        /// <summary>
        /// Gauti tik privačių asmenų tipo klientus
        /// </summary>
        Task<IEnumerable<Customer>> GetPrivateCustomersAsync();

        /// <summary>
        /// Gauti aktyvius klientus
        /// </summary>
        Task<IEnumerable<Customer>> GetActiveCustomersAsync();

        /// <summary>
        /// Gauti klientus pagal vadybininką
        /// </summary>
        Task<IEnumerable<Customer>> GetCustomersByManagerAsync(string managerId);

        /// <summary>
        /// Gauti klientus, su kuriais reikia susisiekti
        /// </summary>
        Task<IEnumerable<Customer>> GetCustomersNeedingContactAsync();

        /// <summary>
        /// Atnaujinti kliento kontakto informaciją
        /// </summary>
        Task UpdateCustomerContactAsync(long customerId, DateTime contactDate, string notes);

        /// <summary>
        /// Priskirti vadybininką klientui
        /// </summary>
        Task AssignManagerAsync(long customerId, string managerId, string managerName);
    }
}

using WarehouseSystem.Domain.Models;

namespace WarehouseSystem.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Klientų repozitorijos interfeisas
    /// </summary>
    public interface ICustomersRepository
    {
        /// <summary>
        /// Gauti visus klientus
        /// </summary>
        Task<IEnumerable<Customer>> GetAllAsync();

        /// <summary>
        /// Gauti klientą pagal ID
        /// </summary>
        Task<Customer> GetByIdAsync(long id);

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
        /// Gauti klientus pagal tipą
        /// </summary>
        Task<IEnumerable<Customer>> GetByTypeAsync(string customerType);

        /// <summary>
        /// Gauti klientus pagal statusą
        /// </summary>
        Task<IEnumerable<Customer>> GetByStatusAsync(string status);

        /// <summary>
        /// Gauti klientus pagal vadybininką
        /// </summary>
        Task<IEnumerable<Customer>> GetByManagerAsync(string managerId);

        /// <summary>
        /// Gauti klientus pagal miestą
        /// </summary>
        Task<IEnumerable<Customer>> GetByCityAsync(string city);

        /// <summary>
        /// Gauti klientus pagal nuolaidos lygį
        /// </summary>
        Task<IEnumerable<Customer>> GetByDiscountLevelAsync(string discountLevel);

        /// <summary>
        /// Patikrinti ar egzistuoja klientas su nurodytu el. paštu
        /// </summary>
        Task<bool> ExistsByEmailAsync(string email);

        /// <summary>
        /// Patikrinti ar egzistuoja įmonė su nurodytu kodu
        /// </summary>
        Task<bool> ExistsByCompanyCodeAsync(string companyCode);

        /// <summary>
        /// Išsaugoti CSV failo pakeitimus
        /// </summary>
        Task SaveChangesToCsvAsync();
    }
}

// Path: WarehouseSystem/Presentation/Controllers/CustomersController.cs

using Application.Services.Interfaces;
using Domain.Enums;
using Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers
{
    /// <summary>
    /// Kontroleris skirtas klientų (Customer) valdymui
    /// </summary>
    [ApiController]  // Žymi, kad tai yra API kontroleris
    [Route("customers")]  // API endpoint pradžia, pvz: /customers
    public class CustomersController : ControllerBase
    {
        // Existing code...
    }
    public class Customer : AuditableEntity
    {
        public long Id { get; set; }
        public CustomerType CustomerType { get; set; }
        public string Name { get; set; }
        public string? CompanyCode { get; set; }
        public string? VATCode { get; set; }
        public string LegalAddress { get; set; }
        public string ContactPersonName { get; set; }
        public string ContactEmail { get; set; }
        public string ContactPhone { get; set; }
        public decimal CreditLimit { get; set; }
        public bool IsActive { get; set; } = true;
        public ICollection<Site> Sites { get; set; }
    }

    public class Site : AuditableEntity
    {
        public long Id { get; set; }
        public long CustomerId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public bool IsActive { get; set; } = true;
        public Customer Customer { get; set; }
    }
}

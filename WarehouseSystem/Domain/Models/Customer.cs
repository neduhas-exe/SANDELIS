namespace WarehouseSystem.Domain.Models
{
    /// <summary>
    /// Kliento domeno modelis
    /// </summary>
    public class Customer
    {
        /// <summary>
        /// Kliento unikalus identifikatorius
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Kliento tipas (Company/Private/Guest)
        /// </summary>
        public string CustomerType { get; set; }

        #region Įmonės duomenys
        public string CompanyName { get; set; }
        public string CompanyCode { get; set; }
        public string VATCode { get; set; }
        #endregion

        #region Asmens duomenys
        public string FirstName { get; set; }
        public string LastName { get; set; }
        #endregion

        #region Kontaktinė informacija
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string PreferredContactMethod { get; set; }
        #endregion

        #region Vadybininko informacija
        public string AssignedManagerID { get; set; }
        public string AssignedManagerName { get; set; }
        public DateTime? LastContactDate { get; set; }
        public DateTime? NextContactDate { get; set; }
        public string CustomerNotes { get; set; }
        #endregion

        #region Kliento statusas ir nuolaidos
        public bool IsGuest { get; set; }
        public string CustomerStatus { get; set; }
        public string DiscountLevel { get; set; }
        #endregion

        #region Sisteminė informacija
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion

        #region Metodai
        /// <summary>
        /// Gauti pilną kliento pavadinimą
        /// </summary>
        public string GetFullName()
        {
            if (!string.IsNullOrEmpty(CompanyName))
                return CompanyName;
            return $"{FirstName} {LastName}".Trim();
        }

        /// <summary>
        /// Gauti pilną adresą
        /// </summary>
        public string GetFullAddress()
        {
            return $"{Address}, {City}, {PostalCode}";
        }

        /// <summary>
        /// Patikrinti ar reikia susisiekti su klientu
        /// </summary>
        public bool NeedsContact()
        {
            return NextContactDate.HasValue && NextContactDate.Value.Date <= DateTime.Today;
        }

        /// <summary>
        /// Atnaujinti kontakto informaciją
        /// </summary>
        public void UpdateContactInfo(DateTime contactDate, string notes)
        {
            LastContactDate = contactDate;
            NextContactDate = contactDate.AddDays(30); // Numatytasis sekantis kontaktas po 30 dienų
            CustomerNotes = notes;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Priskirti vadybininką
        /// </summary>
        public void AssignManager(string managerId, string managerName)
        {
            AssignedManagerID = managerId;
            AssignedManagerName = managerName;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Patikrinti ar klientas yra aktyvus
        /// </summary>
        public bool IsActive()
        {
            return CustomerStatus?.Equals("Active", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Gauti nuolaidos procentą pagal lygį
        /// </summary>
        public decimal GetDiscountPercentage()
        {
            return DiscountLevel switch
            {
                "A" => 15.0m,
                "B" => 10.0m,
                "C" => 5.0m,
                "D" => 0.0m,
                _ => 0.0m
            };
        }
        #endregion
    }
}

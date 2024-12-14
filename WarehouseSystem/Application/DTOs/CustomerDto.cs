using System.ComponentModel.DataAnnotations;

namespace WarehouseSystem.Application.DTOs
{
    /// <summary>
    /// Kliento duomenų perdavimo objektas
    /// </summary>
    public class CustomerDto
    {
        /// <summary>
        /// Kliento unikalus identifikatorius
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Kliento tipas (Company/Private/Guest)
        /// </summary>
        [Required(ErrorMessage = "Kliento tipas yra privalomas")]
        public string CustomerType { get; set; }

        #region Įmonės duomenys

        /// <summary>
        /// Įmonės pavadinimas (jei klientas - įmonė)
        /// </summary>
        [StringLength(200, ErrorMessage = "Įmonės pavadinimas negali viršyti 200 simbolių")]
        public string CompanyName { get; set; }

        /// <summary>
        /// Įmonės kodas
        /// </summary>
        [RegularExpression(@"^\d{9}$", ErrorMessage = "Įmonės kodas turi būti 9 skaitmenų")]
        public string CompanyCode { get; set; }

        /// <summary>
        /// PVM mokėtojo kodas
        /// </summary>
        [RegularExpression(@"^LT\d{11}$", ErrorMessage = "PVM kodas turi būti formato LT ir 11 skaitmenų")]
        public string VATCode { get; set; }

        #endregion

        #region Asmens duomenys

        /// <summary>
        /// Kliento vardas
        /// </summary>
        [StringLength(50, ErrorMessage = "Vardas negali viršyti 50 simbolių")]
        public string FirstName { get; set; }

        /// <summary>
        /// Kliento pavardė
        /// </summary>
        [StringLength(50, ErrorMessage = "Pavardė negali viršyti 50 simbolių")]
        public string LastName { get; set; }

        #endregion

        #region Kontaktinė informacija

        /// <summary>
        /// El. pašto adresas
        /// </summary>
        [Required(ErrorMessage = "El. paštas yra privalomas")]
        [EmailAddress(ErrorMessage = "Neteisingas el. pašto formatas")]
        public string Email { get; set; }

        /// <summary>
        /// Telefono numeris
        /// </summary>
        [Required(ErrorMessage = "Telefono numeris yra privalomas")]
        [RegularExpression(@"^\+370\d{8}$", ErrorMessage = "Telefono numeris turi būti formato +370XXXXXXXX")]
        public string Phone { get; set; }

        /// <summary>
        /// Adresas
        /// </summary>
        [Required(ErrorMessage = "Adresas yra privalomas")]
        [StringLength(200, ErrorMessage = "Adresas negali viršyti 200 simbolių")]
        public string Address { get; set; }

        /// <summary>
        /// Miestas
        /// </summary>
        [Required(ErrorMessage = "Miestas yra privalomas")]
        [StringLength(50, ErrorMessage = "Miestas negali viršyti 50 simbolių")]
        public string City { get; set; }

        /// <summary>
        /// Pašto kodas
        /// </summary>
        [Required(ErrorMessage = "Pašto kodas yra privalomas")]
        [RegularExpression(@"^LT-\d{5}$", ErrorMessage = "Pašto kodas turi būti formato LT-XXXXX")]
        public string PostalCode { get; set; }

        /// <summary>
        /// Pageidaujamas kontakto būdas (Email/Phone)
        /// </summary>
        [Required(ErrorMessage = "Pageidaujamas kontakto būdas yra privalomas")]
        public string PreferredContactMethod { get; set; }

        #endregion

        #region Vadybininko informacija

        /// <summary>
        /// Priskirto vadybininko ID
        /// </summary>
        public string AssignedManagerID { get; set; }

        /// <summary>
        /// Priskirto vadybininko vardas ir pavardė
        /// </summary>
        public string AssignedManagerName { get; set; }

        /// <summary>
        /// Paskutinio kontakto data
        /// </summary>
        public DateTime? LastContactDate { get; set; }

        /// <summary>
        /// Sekančio planuojamo kontakto data
        /// </summary>
        public DateTime? NextContactDate { get; set; }

        /// <summary>
        /// Pastabos apie klientą
        /// </summary>
        [StringLength(1000, ErrorMessage = "Pastabos negali viršyti 1000 simbolių")]
        public string CustomerNotes { get; set; }

        #endregion

        #region Kliento statusas ir nuolaidos

        /// <summary>
        /// Ar svečias
        /// </summary>
        public bool IsGuest { get; set; }

        /// <summary>
        /// Kliento statusas
        /// </summary>
        [Required(ErrorMessage = "Kliento statusas yra privalomas")]
        public string CustomerStatus { get; set; } = "Active";

        /// <summary>
        /// Nuolaidos lygis (A, B, C, D)
        /// </summary>
        [Required(ErrorMessage = "Nuolaidos lygis yra privalomas")]
        [RegularExpression(@"^[A-D]$", ErrorMessage = "Nuolaidos lygis turi būti A, B, C arba D")]
        public string DiscountLevel { get; set; }

        #endregion

        #region Sisteminė informacija

        /// <summary>
        /// Įrašo kūrėjas
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// Sukūrimo data
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// Paskutinis redaguotojas
        /// </summary>
        public string ModifiedBy { get; set; }

        /// <summary>
        /// Paskutinio redagavimo data
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        #endregion

        #region Apskaičiuojamos savybės

        /// <summary>
        /// Pilnas kliento vardas
        /// </summary>
        public string FullName
        {
            get
            {
                if (!string.IsNullOrEmpty(CompanyName))
                    return CompanyName;
                return $"{FirstName} {LastName}".Trim();
            }
        }

        /// <summary>
        /// Ar klientas yra įmonė
        /// </summary>
        public bool IsCompany => CustomerType?.Equals("Company", StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// Ar klientas yra privatus asmuo
        /// </summary>
        public bool IsPrivate => CustomerType?.Equals("Private", StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// Ar klientui priskirtas vadybininkas
        /// </summary>
        public bool HasAssignedManager => !string.IsNullOrEmpty(AssignedManagerID);

        /// <summary>
        /// Ar reikia susisiekti su klientu (praėjo NextContactDate)
        /// </summary>
        public bool NeedsContact => NextContactDate.HasValue && NextContactDate.Value.Date <= DateTime.Today;

        /// <summary>
        /// Dienų skaičius nuo paskutinio kontakto
        /// </summary>
        public int? DaysSinceLastContact => LastContactDate.HasValue 
            ? (int)(DateTime.Today - LastContactDate.Value).TotalDays 
            : null;

        #endregion
    }
}

using System.ComponentModel.DataAnnotations;

namespace WarehouseSystem.Application.DTOs
{
    /// <summary>
    /// Kliento objekto duomenų perdavimo objektas
    /// </summary>
    public class CustomerObjectDto
    {
        /// <summary>
        /// Objekto unikalus identifikatorius
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Kliento ID kuriam priklauso objektas
        /// </summary>
        [Required(ErrorMessage = "Kliento ID yra privalomas")]
        public long CustomerID { get; set; }

        /// <summary>
        /// Objekto pavadinimas
        /// </summary>
        [Required(ErrorMessage = "Objekto pavadinimas yra privalomas")]
        [StringLength(200, ErrorMessage = "Objekto pavadinimas negali viršyti 200 simbolių")]
        public string ObjectName { get; set; }

        /// <summary>
        /// Objekto tipas (Commercial/Residential/Apartment/House/Industrial)
        /// </summary>
        [Required(ErrorMessage = "Objekto tipas yra privalomas")]
        public string ObjectType { get; set; }

        /// <summary>
        /// Objekto adresas
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
        /// Kontaktinis asmuo objekte
        /// </summary>
        [Required(ErrorMessage = "Kontaktinis asmuo yra privalomas")]
        [StringLength(100, ErrorMessage = "Kontaktinio asmens vardas negali viršyti 100 simbolių")]
        public string ContactPerson { get; set; }

        /// <summary>
        /// Kontaktinio asmens telefono numeris
        /// </summary>
        [Required(ErrorMessage = "Kontaktinio asmens telefono numeris yra privalomas")]
        [RegularExpression(@"^\+370\d{8}$", ErrorMessage = "Telefono numeris turi būti formato +370XXXXXXXX")]
        public string ContactPhone { get; set; }

        /// <summary>
        /// Objekto statusas (Active/Inactive/Planned/Finished)
        /// </summary>
        [Required(ErrorMessage = "Objekto statusas yra privalomas")]
        public string Status { get; set; }

        /// <summary>
        /// Projekto fazė (Planning/Construction/Installation/Renovation/Maintenance/Finished)
        /// </summary>
        [Required(ErrorMessage = "Projekto fazė yra privaloma")]
        public string ProjectPhase { get; set; }

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

        #region Papildomos savybės

        /// <summary>
        /// Ar objektas aktyvus
        /// </summary>
        public bool IsActive => Status?.Equals("Active", StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// Ar objektas yra planavimo fazėje
        /// </summary>
        public bool IsPlanning => ProjectPhase?.Equals("Planning", StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// Ar objektas yra statybų/renovacijos fazėje
        /// </summary>
        public bool IsUnderConstruction => ProjectPhase?.Equals("Construction", StringComparison.OrdinalIgnoreCase) 
                                         || ProjectPhase?.Equals("Renovation", StringComparison.OrdinalIgnoreCase) ?? false;

        /// <summary>
        /// Pilnas objekto adresas
        /// </summary>
        public string FullAddress => $"{Address}, {City}, {PostalCode}";

        /// <summary>
        /// Objekto tipo kategorija (Commercial/Residential/Industrial)
        /// </summary>
        public string Category
        {
            get
            {
                return ObjectType switch
                {
                    "Commercial" => "Commercial",
                    "Apartment" or "House" or "Residential" => "Residential",
                    "Industrial" => "Industrial",
                    _ => "Other"
                };
            }
        }

        #endregion

        #region Validacija

        /// <summary>
        /// Patikrinti ar objekto tipas yra leistinas
        /// </summary>
        public bool IsValidObjectType()
        {
            string[] validTypes = { "Commercial", "Residential", "Apartment", "House", "Industrial" };
            return validTypes.Contains(ObjectType, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Patikrinti ar objekto statusas yra leistinas
        /// </summary>
        public bool IsValidStatus()
        {
            string[] validStatuses = { "Active", "Inactive", "Planned", "Finished" };
            return validStatuses.Contains(Status, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Patikrinti ar projekto fazė yra leistina
        /// </summary>
        public bool IsValidProjectPhase()
        {
            string[] validPhases = { "Planning", "Construction", "Installation", 
                                   "Renovation", "Maintenance", "Finished" };
            return validPhases.Contains(ProjectPhase, StringComparer.OrdinalIgnoreCase);
        }

        #endregion
    }
}

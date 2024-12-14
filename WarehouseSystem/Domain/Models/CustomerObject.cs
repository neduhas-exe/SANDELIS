namespace WarehouseSystem.Domain.Models
{
    /// <summary>
    /// Kliento objekto domeno modelis
    /// </summary>
    public class CustomerObject
    {
        /// <summary>
        /// Objekto unikalus identifikatorius
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Kliento ID kuriam priklauso objektas
        /// </summary>
        public long CustomerID { get; set; }

        #region Objekto pagrindinė informacija
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public string Status { get; set; }
        public string ProjectPhase { get; set; }
        #endregion

        #region Objekto vietos informacija
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Region { get; set; }
        #endregion

        #region Kontaktinė informacija
        public string ContactPerson { get; set; }
        public string ContactPhone { get; set; }
        public string ContactEmail { get; set; }
        #endregion

        #region Projekto informacija
        public DateTime? ProjectStartDate { get; set; }
        public DateTime? ProjectEndDate { get; set; }
        public string ProjectNotes { get; set; }
        public decimal? ProjectBudget { get; set; }
        #endregion

        #region Sisteminė informacija
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string ModifiedBy { get; set; }
        public DateTime ModifiedDate { get; set; }
        #endregion

        #region Metodai
        /// <summary>
        /// Gauti pilną objekto adresą
        /// </summary>
        public string GetFullAddress()
        {
            return $"{Address}, {City}, {PostalCode}";
        }

        /// <summary>
        /// Patikrinti ar objektas yra aktyvus
        /// </summary>
        public bool IsActive()
        {
            return Status?.Equals("Active", StringComparison.OrdinalIgnoreCase) ?? false;
        }

        /// <summary>
        /// Patikrinti ar projektas vėluoja
        /// </summary>
        public bool IsDelayed()
        {
            if (!ProjectEndDate.HasValue) return false;
            return ProjectEndDate.Value < DateTime.Today && !Status.Equals("Finished", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Atnaujinti projekto fazę
        /// </summary>
        public void UpdateProjectPhase(string newPhase, string modifiedBy)
        {
            ProjectPhase = newPhase;
            ModifiedBy = modifiedBy;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Pažymėti projektą kaip baigtą
        /// </summary>
        public void MarkAsFinished(string modifiedBy)
        {
            Status = "Finished";
            ProjectPhase = "Finished";
            ProjectEndDate = DateTime.Now;
            ModifiedBy = modifiedBy;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Pridėti projekto pastabą
        /// </summary>
        public void AddProjectNote(string note, string modifiedBy)
        {
            ProjectNotes = string.IsNullOrEmpty(ProjectNotes)
                ? note
                : $"{ProjectNotes}\n{DateTime.Now:yyyy-MM-dd}: {note}";
            ModifiedBy = modifiedBy;
            ModifiedDate = DateTime.Now;
        }

        /// <summary>
        /// Gauti projekto trukmę dienomis
        /// </summary>
        public int? GetProjectDuration()
        {
            if (!ProjectStartDate.HasValue || !ProjectEndDate.HasValue)
                return null;
            return (int)(ProjectEndDate.Value - ProjectStartDate.Value).TotalDays;
        }

        /// <summary>
        /// Patikrinti ar objekto tipas yra teisingas
        /// </summary>
        public bool IsValidObjectType()
        {
            string[] validTypes = { "Commercial", "Residential", "Apartment", "House", "Industrial" };
            return validTypes.Contains(ObjectType, StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Patikrinti ar projekto fazė yra teisinga
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

namespace Presentation.DTOs.Users
{
    /// <summary>
    /// DTO rolių hierarchijos informacijai
    /// </summary>
    public class RoleHierarchyDto
    {
        public string RoleName { get; set; }
        public string ParentRoleName { get; set; }  // Aukštesnio lygio rolė
        public int Level { get; set; }              // Hierarchijos lygis (0 - aukščiausias)
        public bool InheritsPermissions { get; set; }  // Ar paveldi tėvinės rolės teises
        public DateTime CreatedAt { get; set; }
        public string CreatedByUser { get; set; }
        public List<string> ChildRoles { get; set; } = new();  // Žemesnio lygio rolės

        public string ToCsvLine()
        {
            return $"\"{RoleName}\"," +
                   $"\"{ParentRoleName}\"," +
                   $"{Level}," +
                   $"{InheritsPermissions}," +
                   $"{CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{CreatedByUser}\"," +
                   $"\"{string.Join("|", ChildRoles)}\"";
        }

        public static string GetCsvHeader()
        {
            return "RoleName,ParentRoleName,Level,InheritsPermissions,CreatedAt,CreatedByUser,ChildRoles";
        }
    }

    /// <summary>
    /// DTO naujos rolių hierarchijos ryšio sukūrimui
    /// </summary>
    public class CreateRoleHierarchyDto
    {
        public string RoleName { get; set; }
        public string ParentRoleName { get; set; }
        public bool InheritsPermissions { get; set; } = true;
        public string CreatedByUser { get; set; }
    }

    /// <summary>
    /// DTO rolių hierarchijos ryšio atnaujinimui
    /// </summary>
    public class UpdateRoleHierarchyDto
    {
        public string RoleName { get; set; }
        public string NewParentRoleName { get; set; }
        public bool? InheritsPermissions { get; set; }
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }
}

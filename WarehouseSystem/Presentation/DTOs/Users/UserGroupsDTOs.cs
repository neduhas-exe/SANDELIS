namespace Presentation.DTOs.Users
{
    /// <summary>
    /// DTO vartotojų grupės informacijai
    /// </summary>
    public class UserGroupDto
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public List<string> Members { get; set; } = new();  // Grupės narių vartotojų vardai
        public List<string> AssignedRoles { get; set; } = new();  // Grupei priskirtos rolės
        public string GroupOwner { get; set; }  // Grupės savininkas/administratorius
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedByUser { get; set; }

        public string ToCsvLine()
        {
            return $"\"{GroupId}\"," +
                   $"\"{GroupName}\"," +
                   $"\"{Description}\"," +
                   $"\"{Department}\"," +
                   $"\"{string.Join("|", Members)}\"," +
                   $"\"{string.Join("|", AssignedRoles)}\"," +
                   $"\"{GroupOwner}\"," +
                   $"{IsActive}," +
                   $"{CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{CreatedByUser}\"," +
                   $"{(LastUpdatedAt.HasValue ? LastUpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "")}," +
                   $"\"{LastUpdatedByUser}\"";
        }

        public static string GetCsvHeader()
        {
            return "GroupId,GroupName,Description,Department,Members,AssignedRoles,GroupOwner," +
                   "IsActive,CreatedAt,CreatedByUser,LastUpdatedAt,LastUpdatedByUser";
        }
    }

    /// <summary>
    /// DTO naujos vartotojų grupės sukūrimui
    /// </summary>
    public class CreateUserGroupDto
    {
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public List<string> InitialMembers { get; set; } = new();
        public List<string> AssignedRoles { get; set; } = new();
        public string GroupOwner { get; set; }
        public string CreatedByUser { get; set; }
    }

    /// <summary>
    /// DTO vartotojų grupės atnaujinimui
    /// </summary>
    public class UpdateUserGroupDto
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }
        public string Department { get; set; }
        public string GroupOwner { get; set; }
        public bool? IsActive { get; set; }
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }

    /// <summary>
    /// DTO grupės narių valdymui
    /// </summary>
    public class UpdateGroupMembersDto
    {
        public string GroupId { get; set; }
        public List<string> MembersToAdd { get; set; } = new();
        public List<string> MembersToRemove { get; set; } = new();
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }

    /// <summary>
    /// DTO grupės rolių valdymui
    /// </summary>
    public class UpdateGroupRolesDto
    {
        public string GroupId { get; set; }
        public List<string> RolesToAdd { get; set; } = new();
        public List<string> RolesToRemove { get; set; } = new();
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }
}

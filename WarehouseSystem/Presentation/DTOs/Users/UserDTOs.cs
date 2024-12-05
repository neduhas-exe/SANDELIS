namespace Presentation.DTOs.Users
{
    /// <summary>
    /// DTO skirtas naujo vartotojo sukūrimui
    /// </summary>
    public class CreateUserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public string EmployeeCode { get; set; }
        public List<string> AssignedRoles { get; set; } = new();
        public bool IsActive { get; set; } = true;
        public string CreatedByUser { get; set; }
    }

    /// <summary>
    /// DTO skirtas vartotojo informacijos atnaujinimui
    /// </summary>
    public class UpdateUserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Department { get; set; }
        public string Position { get; set; }
        public bool? IsActive { get; set; }
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }

    /// <summary>
    /// DTO skirtas vartotojo rolių valdymui
    /// </summary>
    public class UpdateUserRolesDto
    {
        public string Username { get; set; }
        public List<string> Roles { get; set; }
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }

    /// <summary>
    /// DTO skirtas vartotojo deaktyvavimui/aktyvavimui
    /// </summary>
    public class UpdateUserStatusDto
    {
        public string Username { get; set; }
        public bool IsActive { get; set; }
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }

    /// <summary>
    /// DTO skirtas vartotojo informacijos grąžinimui
    /// </summary>
    public class UserDto
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string Department { get; set; }
        public string Position { get; set; }
        public string EmployeeCode { get; set; }
        public List<string> Roles { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUser { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public string LastUpdatedByUser { get; set; }

        public string ToCsvLine()
        {
            return $"{Username}," +
                   $"\"{Email}\"," +
                   $"\"{FirstName}\"," +
                   $"\"{LastName}\"," +
                   $"\"{Department}\"," +
                   $"\"{Position}\"," +
                   $"\"{EmployeeCode}\"," +
                   $"\"{string.Join("|", Roles)}\"," +
                   $"{IsActive}," +
                   $"{CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{CreatedByUser}\"," +
                   $"{(LastUpdatedAt.HasValue ? LastUpdatedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "")}," +
                   $"\"{LastUpdatedByUser}\"";
        }

        public static string GetCsvHeader()
        {
            return "Username,Email,FirstName,LastName,Department,Position,EmployeeCode,Roles," +
                   "IsActive,CreatedAt,CreatedByUser,LastUpdatedAt,LastUpdatedByUser";
        }
    }

    /// <summary>
    /// DTO skirtas vartotojo rolės informacijai
    /// </summary>
    public class RoleDto
    {
        public string RoleName { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; } = new();
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedByUser { get; set; }

        public string ToCsvLine()
        {
            return $"\"{RoleName}\"," +
                   $"\"{Description}\"," +
                   $"\"{string.Join("|", Permissions)}\"," +
                   $"{IsActive}," +
                   $"{CreatedAt:yyyy-MM-dd HH:mm:ss}," +
                   $"\"{CreatedByUser}\"";
        }

        public static string GetCsvHeader()
        {
            return "RoleName,Description,Permissions,IsActive,CreatedAt,CreatedByUser";
        }
    }

    /// <summary>
    /// DTO skirtas vartotojo rolės sukūrimui
    /// </summary>
    public class CreateRoleDto
    {
        public string RoleName { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; } = new();
        public string CreatedByUser { get; set; }
    }

    /// <summary>
    /// DTO skirtas vartotojo rolės atnaujinimui
    /// </summary>
    public class UpdateRoleDto
    {
        public string RoleName { get; set; }
        public string Description { get; set; }
        public List<string> Permissions { get; set; }
        public string UpdatedByUser { get; set; }
        public string UpdateReason { get; set; }
    }
}

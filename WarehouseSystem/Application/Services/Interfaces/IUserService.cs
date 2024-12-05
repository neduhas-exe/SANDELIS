using Presentation.DTOs.Users;

namespace WarehouseSystem.Services.Interfaces
{
    /// <summary>
    /// Vartotojų valdymo serviso interfeisas
    /// </summary>
    public interface IUserService
    {
        // Vartotojų operacijos
        Task<UserDto> CreateUserAsync(CreateUserDto userDto);
        Task<UserDto> GetUserByUsernameAsync(string username);
        Task<UserDto> GetUserByEmailAsync(string email);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> UpdateUserAsync(UpdateUserDto userDto);
        Task<bool> UpdateUserStatusAsync(UpdateUserStatusDto statusDto);
        Task<bool> UpdateUserRolesAsync(UpdateUserRolesDto rolesDto);
        Task<bool> DeleteUserAsync(string username, string deletedByUser, string reason);

        // Rolių operacijos
        Task<RoleDto> CreateRoleAsync(CreateRoleDto roleDto);
        Task<RoleDto> GetRoleByNameAsync(string roleName);
        Task<IEnumerable<RoleDto>> GetAllRolesAsync();
        Task<RoleDto> UpdateRoleAsync(UpdateRoleDto roleDto);
        Task<bool> DeleteRoleAsync(string roleName, string deletedByUser, string reason);

        // Teisių valdymas
        Task<bool> HasPermissionAsync(string username, string permission);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string username);
        Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName);

        // CSV operacijos
        Task ExportUsersToCsvAsync(string filePath);
        Task ImportUsersFromCsvAsync(string filePath);
        Task ExportRolesToCsvAsync(string filePath);
        Task ImportRolesFromCsvAsync(string filePath);
        
        // Auditas ir ataskaitos
        Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(string department);
        Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName);
        Task<IEnumerable<UserDto>> GetInactiveUsersAsync();
        Task<string> GenerateUserActivityReportAsync(string username);
        Task<string> GenerateRoleAssignmentReportAsync();
    }
}

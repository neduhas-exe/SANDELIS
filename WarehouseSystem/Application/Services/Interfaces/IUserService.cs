/// <summary>
    /// Vartotojų valdymo serviso interfeisas
    /// </summary>
    public interface IUserService
    {
        #region Vartotojų aktyvumas
        Task<UserActivityDto> LogUserActivityAsync(UserActivityDto activityDto);
        Task<UserSessionDto> StartUserSessionAsync(string username, string ipAddress, string userAgent);
        Task<bool> EndUserSessionAsync(string sessionId);
        Task<bool> UpdateSessionActivityAsync(string sessionId);
        Task<UserSessionDto> GetActiveSessionAsync(string username);
        Task<UserActivityStatsDto> GetUserActivityStatsAsync(
            string username,
            DateTime startDate,
            DateTime endDate);
        Task<IEnumerable<UserActivityDto>> GetUserActivitiesAsync(
            string username,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<IEnumerable<UserSessionDto>> GetUserSessionsAsync(
            string username,
            DateTime? startDate = null,
            DateTime? endDate = null);

        #region Rolių hierarchija
        Task<RoleHierarchyDto> CreateRoleHierarchyAsync(CreateRoleHierarchyDto hierarchyDto);
        Task<RoleHierarchyDto> GetRoleHierarchyAsync(string roleName);
        Task<bool> UpdateRoleHierarchyAsync(UpdateRoleHierarchyDto hierarchyDto);
        Task<bool> DeleteRoleHierarchyAsync(string roleName);
        Task<IEnumerable<RoleHierarchyDto>> GetRoleHierarchyTreeAsync();
        Task<IEnumerable<string>> GetInheritedPermissionsAsync(string roleName);
        Task<IEnumerable<string>> GetChildRolesAsync(string roleName);
        Task<IEnumerable<string>> GetParentRolesAsync(string roleName);
        
        #endregion

        #region Vartotojų grupės
        Task<UserGroupDto> CreateUserGroupAsync(CreateUserGroupDto groupDto);
        Task<UserGroupDto> GetGroupByIdAsync(string groupId);
        Task<IEnumerable<UserGroupDto>> GetAllGroupsAsync();
        Task<UserGroupDto> UpdateGroupAsync(UpdateUserGroupDto groupDto);
        Task<bool> DeleteGroupAsync(string groupId, string deletedByUser, string reason);
        
        // Grupės narių valdymas
        Task<bool> UpdateGroupMembersAsync(UpdateGroupMembersDto membersDto);
        Task<IEnumerable<UserDto>> GetGroupMembersAsync(string groupId);
        Task<IEnumerable<UserGroupDto>> GetUserGroupsAsync(string username);
        
        // Grupės rolių valdymas
        Task<bool> UpdateGroupRolesAsync(UpdateGroupRolesDto rolesDto);
        Task<IEnumerable<RoleDto>> GetGroupRolesAsync(string groupId);
        Task<IEnumerable<string>> GetGroupPermissionsAsync(string groupId);
        
        // Grupių paieška ir filtravimas
        Task<IEnumerable<UserGroupDto>> GetGroupsByDepartmentAsync(string department);
        Task<IEnumerable<UserGroupDto>> GetGroupsByRoleAsync(string roleName);
        Task<IEnumerable<UserGroupDto>> GetGroupsByOwnerAsync(string ownerUsername);
        
        #endregion

        #region Pagrindinės operacijos
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
        #endregion
    }

using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Users;

namespace WarehouseSystem.Services
{
    public partial class UserService
    {
        private readonly string _groupsFilePath;
        private static readonly ConcurrentDictionary<string, UserGroupDto> _groupsCache = new();

        private async Task InitializeGroupFiles()
        {
            _groupsFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "user_groups.csv");

            if (!File.Exists(_groupsFilePath))
            {
                await File.WriteAllTextAsync(_groupsFilePath, UserGroupDto.GetCsvHeader());
            }

            // Inicializuojame cache
            await RefreshGroupsCache();
        }

        private async Task RefreshGroupsCache()
        {
            var lines = await ReadAllLinesAsync(_groupsFilePath);
            var groups = lines.Skip(1)
                            .Select(ParseGroupLine)
                            .Where(g => g != null);

            _groupsCache.Clear();
            foreach (var group in groups)
            {
                _groupsCache.TryAdd(group.GroupId, group);
            }
        }

        public async Task<UserGroupDto> CreateUserGroupAsync(CreateUserGroupDto groupDto)
        {
            try
            {
                // Validuojame narius
                foreach (var member in groupDto.InitialMembers)
                {
                    var user = await GetUserByUsernameAsync(member);
                    if (user == null)
                    {
                        throw new KeyNotFoundException($"Vartotojas {member} nerastas");
                    }
                }

                // Validuojame roles
                foreach (var role in groupDto.AssignedRoles)
                {
                    var roleExists = await GetRoleByNameAsync(role);
                    if (roleExists == null)
                    {
                        throw new KeyNotFoundException($"Rolė {role} nerasta");
                    }
                }

                var group = new UserGroupDto
                {
                    GroupId = Guid.NewGuid().ToString(),
                    GroupName = groupDto.GroupName,
                    Description = groupDto.Description,
                    Department = groupDto.Department,
                    Members = groupDto.InitialMembers,
                    AssignedRoles = groupDto.AssignedRoles,
                    GroupOwner = groupDto.GroupOwner,
                    IsActive = true,
                    CreatedAt = DateTime.Now,
                    CreatedByUser = groupDto.CreatedByUser
                };

                // Įrašome į CSV
                var lines = await ReadAllLinesAsync(_groupsFilePath);
                lines.Add(group.ToCsvLine());
                await WriteAllLinesAsync(_groupsFilePath, lines);

                // Atnaujiname cache
                _groupsCache.TryAdd(group.GroupId, group);

                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant vartotojų grupę");
                throw;
            }
        }

        public async Task<UserGroupDto> GetGroupByIdAsync(string groupId)
        {
            if (_groupsCache.TryGetValue(groupId, out var group))
            {
                return group;
            }

            var lines = await ReadAllLinesAsync(_groupsFilePath);
            var groupLine = lines.Skip(1)
                               .FirstOrDefault(l => l.StartsWith($"\"{groupId}\""));

            return groupLine != null ? ParseGroupLine(groupLine) : null;
        }

        public async Task<IEnumerable<UserGroupDto>> GetAllGroupsAsync()
        {
            return _groupsCache.Values;
        }

        public async Task<UserGroupDto> UpdateGroupAsync(UpdateUserGroupDto groupDto)
        {
            try
            {
                var lines = await ReadAllLinesAsync(_groupsFilePath);
                var groupLines = lines.ToList();
                var index = groupLines.FindIndex(l => l.StartsWith($"\"{groupDto.GroupId}\""));

                if (index == -1)
                {
                    throw new KeyNotFoundException($"Grupė {groupDto.GroupId} nerasta");
                }

                var currentGroup = ParseGroupLine(groupLines[index]);

                if (groupDto.GroupName != null) currentGroup.GroupName = groupDto.GroupName;
                if (groupDto.Description != null) currentGroup.Description = groupDto.Description;
                if (groupDto.Department != null) currentGroup.Department = groupDto.Department;
                if (groupDto.GroupOwner != null) currentGroup.GroupOwner = groupDto.GroupOwner;
                if (groupDto.IsActive.HasValue) currentGroup.IsActive = groupDto.IsActive.Value;

                currentGroup.LastUpdatedAt = DateTime.Now;
                currentGroup.LastUpdatedByUser = groupDto.UpdatedByUser;

                groupLines[index] = currentGroup.ToCsvLine();
                await WriteAllLinesAsync(_groupsFilePath, groupLines);

                // Atnaujiname cache
                _groupsCache.AddOrUpdate(currentGroup.GroupId, currentGroup, (_, __) => currentGroup);

                return currentGroup;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant grupę");
                throw;
            }
        }

        public async Task<bool> DeleteGroupAsync(string groupId, string deletedByUser, string reason)
        {
            try
            {
                var group = await GetGroupByIdAsync(groupId);
                if (group == null)
                {
                    return false;
                }

                group.IsActive = false;
                group.LastUpdatedAt = DateTime.Now;
                group.LastUpdatedByUser = deletedByUser;

                var lines = await ReadAllLinesAsync(_groupsFilePath);
                var groupLines = lines.ToList();
                var index = groupLines.FindIndex(l => l.StartsWith($"\"{groupId}\""));

                if (index != -1)
                {
                    groupLines[index] = group.ToCsvLine();
                    await WriteAllLinesAsync(_groupsFilePath, groupLines);

                    // Atnaujiname cache
                    _groupsCache.TryRemove(groupId, out _);

                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant grupę");
                throw;
            }
        }

        // Grupės narių valdymas
        public async Task<bool> UpdateGroupMembersAsync(UpdateGroupMembersDto membersDto)
        {
            try
            {
                var group = await GetGroupByIdAsync(membersDto.GroupId);
                if (group == null)
                {
                    return false;
                }

                // Validuojame naujus narius
                foreach (var member in membersDto.MembersToAdd)
                {
                    var user = await GetUserByUsernameAsync(member);
                    if (user == null)
                    {
                        throw new KeyNotFoundException($"Vartotojas {member} nerastas");
                    }
                }

                // Pridedame naujus narius
                foreach (var member in membersDto.MembersToAdd)
                {
                    if (!group.Members.Contains(member))
                    {
                        group.Members.Add(member);
                    }
                }

                // Šaliname narius
                foreach (var member in membersDto.MembersToRemove)
                {
                    group.Members.Remove(member);
                }

                group.LastUpdatedAt = DateTime.Now;
                group.LastUpdatedByUser = membersDto.UpdatedByUser;

                // Atnaujiname CSV failą
                await UpdateGroupInCsvAsync(group);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant grupės narius");
                throw;
            }
        }

        public async Task<IEnumerable<UserDto>> GetGroupMembersAsync(string groupId)
        {
            var group = await GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return Enumerable.Empty<UserDto>();
            }

            var members = new List<UserDto>();
            foreach (var username in group.Members)
            {
                var user = await GetUserByUsernameAsync(username);
                if (user != null)
                {
                    members.Add(user);
                }
            }

            return members;
        }

        public async Task<IEnumerable<UserGroupDto>> GetUserGroupsAsync(string username)
        {
            return _groupsCache.Values.Where(g => g.Members.Contains(username));
        }

        // Grupės rolių valdymas
        public async Task<bool> UpdateGroupRolesAsync(UpdateGroupRolesDto rolesDto)
        {
            try
            {
                var group = await GetGroupByIdAsync(rolesDto.GroupId);
                if (group == null)
                {
                    return false;
                }

                // Validuojame naujas roles
                foreach (var role in rolesDto.RolesToAdd)
                {
                    var roleExists = await GetRoleByNameAsync(role);
                    if (roleExists == null)
                    {
                        throw new KeyNotFoundException($"Rolė {role} nerasta");
                    }
                }

                // Pridedame naujas roles
                foreach (var role in rolesDto.RolesToAdd)
                {
                    if (!group.AssignedRoles.Contains(role))
                    {
                        group.AssignedRoles.Add(role);
                    }
                }

                // Šaliname roles
                foreach (var role in rolesDto.RolesToRemove)
                {
                    group.AssignedRoles.Remove(role);
                }

                group.LastUpdatedAt = DateTime.Now;
                group.LastUpdatedByUser = rolesDto.UpdatedByUser;

                // Atnaujiname CSV failą
                await UpdateGroupInCsvAsync(group);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant grupės roles");
                throw;
            }
        }

        public async Task<IEnumerable<RoleDto>> GetGroupRolesAsync(string groupId)
        {
            var group = await GetGroupByIdAsync(groupId);
            if (group == null)
            {
                return Enumerable.Empty<RoleDto>();
            }

            var roles = new List<RoleDto>();
            foreach (var roleName in group.AssignedRoles)
            {
                var role = await GetRoleByNameAsync(roleName);
                if (role != null)
                {
                    roles.Add(role);
                }
            }

            return roles;
        }

        public async Task<IEnumerable<string>> GetGroupPermissionsAsync(string groupId)
        {
            var permissions = new HashSet<string>();
            var group = await GetGroupByIdAsync(groupId);
            
            if (group != null)
            {
                foreach (var roleName in group.AssignedRoles)
                {
                    var rolePermissions = await GetRolePermissionsAsync(roleName);
                    foreach (var permission in rolePermissions)
                    {
                        permissions.Add(permission);
                    }

                    // Pridedame paveldėtas teises
                    var inheritedPermissions = await GetInheritedPermissionsAsync(roleName);
                    foreach (var permission in inheritedPermissions)
                    {
                        permissions.Add(permission);
                    }
                }
            }

            return permissions;
        }

        // Grupių paieška ir filtravimas
        public async Task<IEnumerable<UserGroupDto>> GetGroupsByDepartmentAsync(string department)
        {
            return _groupsCache.Values.Where(g => g.Department == department);
        }

        public async Task<IEnumerable<UserGroupDto>> GetGroupsByRoleAsync(string roleName)
        {
            return _groupsCache.Values.Where(g => g.AssignedRoles.Contains(roleName));
        }

        public async Task<IEnumerable<UserGroupDto>> GetGroupsByOwnerAsync(string ownerUsername)
        {
            return _groupsCache.Values.Where(g => g.GroupOwner == ownerUsername);
        }

        private async Task UpdateGroupInCsvAsync(UserGroupDto group)
        {
            var lines = await ReadAllLinesAsync(_groupsFilePath);
            var groupLines = lines.ToList();
            var index = groupLines.FindIndex(l => l.StartsWith($"\"{group.GroupId}\""));

            if (index != -1)
            {
                groupLines[index] = group.ToCsvLine();
                await WriteAllLinesAsync(_groupsFilePath, groupLines);

                // Atnaujiname cache
                _groupsCache.AddOrUpdate(group.GroupId, group, (_, __) => group);
            }
        }

        private UserGroupDto ParseGroupLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new UserGroupDto
                {
                    GroupId = parts[0].Trim('"'),
                    GroupName = parts[1].Trim('"'),
                    Description = parts[2].Trim('"'),
                    Department = parts[3].Trim('"'),
                    Members = parts[4].Trim('"').Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    AssignedRoles = parts[5].Trim('"').Split('|', StringSplitOptions.RemoveEmptyEntries).ToList(),
                    GroupOwner = parts[6].Trim('"'),
                    IsActive = bool.Parse(parts[7]),
                    CreatedAt = DateTime.Parse(parts[8]),
                    CreatedByUser = parts[9].Trim('"'),
                    LastUpdatedAt = string.IsNullOrEmpty(parts[10]) ? null : DateTime.Parse(parts[10]),
                    LastUpdatedByUser = parts[11].Trim('"')
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant grupės CSV eilutę: {Line}", line);
                return null;
            }
        }

        // Validacijos metodai
        private async Task ValidateGroupMembershipAsync(string username, string groupId)
        {
            // Tikriname maksimalų grupių skaičių vienam vartotojui
            var userGroups = await GetUserGroupsAsync(username);
            var maxGroupsPerUser = 10; // Konfigūruojama reikšmė
            
            if (userGroups.Count() >= maxGroupsPerUser)
            {
                throw new InvalidOperationException($"Vartotojas negali priklausyti daugiau nei {maxGroupsPerUser} grupėms");
            }
        }

        private async Task ValidateGroupRolesAsync(string groupId, IEnumerable<string> roles)
        {
            var maxRolesPerGroup = 5; // Konfigūruojama reikšmė
            
            if (roles.Count() > maxRolesPerGroup)
            {
                throw new InvalidOperationException($"Grupė negali turėti daugiau nei {maxRolesPerGroup} rolių");
            }
        }

        // Ataskaitų generavimas
        private async Task<string> GenerateGroupMembershipReportAsync(string groupId)
        {
            var group = await GetGroupByIdAsync(groupId);
            if (group == null)
            {
                throw new KeyNotFoundException($"Grupė {groupId} nerasta");
            }

            var report = new StringBuilder();
            report.AppendLine($"Grupės {group.GroupName} narių ataskaita");
            report.AppendLine("=================================");
            report.AppendLine($"Grupės ID: {group.GroupId}");
            report.AppendLine($"Departamentas: {group.Department}");
            report.AppendLine($"Savininkas: {group.GroupOwner}");
            report.AppendLine($"Sukurta: {group.CreatedAt:yyyy-MM-dd HH:mm:ss} ({group.CreatedByUser})");
            report.AppendLine($"Būsena: {(group.IsActive ? "Aktyvi" : "Neaktyvi")}");
            
            report.AppendLine("\nPriskirtos rolės:");
            foreach (var role in group.AssignedRoles)
            {
                report.AppendLine($"- {role}");
            }

            report.AppendLine("\nGrupės nariai:");
            foreach (var username in group.Members)
            {
                var user = await GetUserByUsernameAsync(username);
                if (user != null)
                {
                    report.AppendLine($"- {user.FullName} ({username})");
                    report.AppendLine($"  Departamentas: {user.Department}");
                    report.AppendLine($"  Pareigos: {user.Position}");
                }
            }

            report.AppendLine("\nGrupės teisės:");
            var permissions = await GetGroupPermissionsAsync(groupId);
            foreach (var permission in permissions)
            {
                report.AppendLine($"- {permission}");
            }

            return report.ToString();
        }
    }
}

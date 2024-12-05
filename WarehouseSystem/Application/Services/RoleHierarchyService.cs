using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Users;

namespace WarehouseSystem.Services
{
    public partial class UserService
    {
        private readonly string _roleHierarchyFilePath;
        private static readonly ConcurrentDictionary<string, RoleHierarchyDto> _roleHierarchyCache = new();

        private async Task InitializeRoleHierarchyFiles()
        {
            _roleHierarchyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "role_hierarchy.csv");
            
            if (!File.Exists(_roleHierarchyFilePath))
            {
                await File.WriteAllTextAsync(_roleHierarchyFilePath, RoleHierarchyDto.GetCsvHeader());
            }

            // Inicializuojame cache
            await RefreshRoleHierarchyCache();
        }

        private async Task RefreshRoleHierarchyCache()
        {
            var lines = await ReadAllLinesAsync(_roleHierarchyFilePath);
            var hierarchies = lines.Skip(1)
                                 .Select(ParseRoleHierarchyLine)
                                 .Where(h => h != null);

            _roleHierarchyCache.Clear();
            foreach (var hierarchy in hierarchies)
            {
                _roleHierarchyCache.TryAdd(hierarchy.RoleName, hierarchy);
            }
        }

        public async Task<RoleHierarchyDto> CreateRoleHierarchyAsync(CreateRoleHierarchyDto hierarchyDto)
        {
            try
            {
                // Tikriname ar rolės egzistuoja
                var role = await GetRoleByNameAsync(hierarchyDto.RoleName);
                if (role == null)
                {
                    throw new KeyNotFoundException($"Rolė {hierarchyDto.RoleName} nerasta");
                }

                if (!string.IsNullOrEmpty(hierarchyDto.ParentRoleName))
                {
                    var parentRole = await GetRoleByNameAsync(hierarchyDto.ParentRoleName);
                    if (parentRole == null)
                    {
                        throw new KeyNotFoundException($"Tėvinė rolė {hierarchyDto.ParentRoleName} nerasta");
                    }
                }

                // Apskaičiuojame hierarchijos lygį
                int level = 0;
                if (!string.IsNullOrEmpty(hierarchyDto.ParentRoleName))
                {
                    var parentHierarchy = await GetRoleHierarchyAsync(hierarchyDto.ParentRoleName);
                    if (parentHierarchy != null)
                    {
                        level = parentHierarchy.Level + 1;
                    }
                }

                var hierarchy = new RoleHierarchyDto
                {
                    RoleName = hierarchyDto.RoleName,
                    ParentRoleName = hierarchyDto.ParentRoleName,
                    Level = level,
                    InheritsPermissions = hierarchyDto.InheritsPermissions,
                    CreatedAt = DateTime.Now,
                    CreatedByUser = hierarchyDto.CreatedByUser
                };

                // Įrašome į CSV
                var lines = await ReadAllLinesAsync(_roleHierarchyFilePath);
                lines.Add(hierarchy.ToCsvLine());
                await WriteAllLinesAsync(_roleHierarchyFilePath, lines);

                // Atnaujiname cache
                _roleHierarchyCache.TryAdd(hierarchy.RoleName, hierarchy);

                // Pridedame prie tėvinės rolės vaikų sąrašo
                if (!string.IsNullOrEmpty(hierarchy.ParentRoleName))
                {
                    await AddChildRoleAsync(hierarchy.ParentRoleName, hierarchy.RoleName);
                }

                return hierarchy;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant rolės hierarchiją");
                throw;
            }
        }

        public async Task<RoleHierarchyDto> GetRoleHierarchyAsync(string roleName)
        {
            if (_roleHierarchyCache.TryGetValue(roleName, out var hierarchy))
            {
                return hierarchy;
            }

            var lines = await ReadAllLinesAsync(_roleHierarchyFilePath);
            var hierarchyLine = lines.Skip(1)
                                   .FirstOrDefault(l => l.StartsWith($"\"{roleName}\""));

            return hierarchyLine != null ? ParseRoleHierarchyLine(hierarchyLine) : null;
        }

        public async Task<bool> UpdateRoleHierarchyAsync(UpdateRoleHierarchyDto hierarchyDto)
        {
            try
            {
                var lines = await ReadAllLinesAsync(_roleHierarchyFilePath);
                var hierarchyLines = lines.ToList();
                var index = hierarchyLines.FindIndex(l => l.StartsWith($"\"{hierarchyDto.RoleName}\""));

                if (index == -1)
                {
                    return false;
                }

                var currentHierarchy = ParseRoleHierarchyLine(hierarchyLines[index]);
                var oldParentName = currentHierarchy.ParentRoleName;

                // Atnaujiname tėvinę rolę
                if (!string.IsNullOrEmpty(hierarchyDto.NewParentRoleName))
                {
                    var parentRole = await GetRoleByNameAsync(hierarchyDto.NewParentRoleName);
                    if (parentRole == null)
                    {
                        throw new KeyNotFoundException($"Tėvinė rolė {hierarchyDto.NewParentRoleName} nerasta");
                    }

                    currentHierarchy.ParentRoleName = hierarchyDto.NewParentRoleName;
                    
                    // Perskaičiuojame lygį
                    var parentHierarchy = await GetRoleHierarchyAsync(hierarchyDto.NewParentRoleName);
                    currentHierarchy.Level = (parentHierarchy?.Level ?? -1) + 1;
                }

                if (hierarchyDto.InheritsPermissions.HasValue)
                {
                    currentHierarchy.InheritsPermissions = hierarchyDto.InheritsPermissions.Value;
                }

                hierarchyLines[index] = currentHierarchy.ToCsvLine();
                await WriteAllLinesAsync(_roleHierarchyFilePath, hierarchyLines);

                // Atnaujiname tėvinių rolių vaikų sąrašus
                if (oldParentName != hierarchyDto.NewParentRoleName)
                {
                    if (!string.IsNullOrEmpty(oldParentName))
                    {
                        await RemoveChildRoleAsync(oldParentName, hierarchyDto.RoleName);
                    }
                    if (!string.IsNullOrEmpty(hierarchyDto.NewParentRoleName))
                    {
                        await AddChildRoleAsync(hierarchyDto.NewParentRoleName, hierarchyDto.RoleName);
                    }
                }

                // Atnaujiname cache
                await RefreshRoleHierarchyCache();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant rolės hierarchiją");
                throw;
            }
        }

        public async Task<bool> DeleteRoleHierarchyAsync(string roleName)
        {
            try
            {
                var hierarchy = await GetRoleHierarchyAsync(roleName);
                if (hierarchy == null)
                {
                    return false;
                }

                // Patikriname ar turi vaikų
                if (hierarchy.ChildRoles.Any())
                {
                    throw new InvalidOperationException("Negalima ištrinti rolės, kuri turi pavaldžių rolių");
                }

                var lines = await ReadAllLinesAsync(_roleHierarchyFilePath);
                var hierarchyLines = lines.Where(l => !l.StartsWith($"\"{roleName}\""));
                await WriteAllLinesAsync(_roleHierarchyFilePath, hierarchyLines);

                // Atnaujiname tėvinės rolės vaikų sąrašą
                if (!string.IsNullOrEmpty(hierarchy.ParentRoleName))
                {
                    await RemoveChildRoleAsync(hierarchy.ParentRoleName, roleName);
                }

                // Atnaujiname cache
                _roleHierarchyCache.TryRemove(roleName, out _);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant rolės hierarchiją");
                throw;
            }
        }

        public async Task<IEnumerable<RoleHierarchyDto>> GetRoleHierarchyTreeAsync()
        {
            var hierarchies = _roleHierarchyCache.Values
                .OrderBy(h => h.Level)
                .ToList();

            foreach (var hierarchy in hierarchies)
            {
                hierarchy.ChildRoles = _roleHierarchyCache.Values
                    .Where(h => h.ParentRoleName == hierarchy.RoleName)
                    .Select(h => h.RoleName)
                    .ToList();
            }

            return hierarchies;
        }

        public async Task<IEnumerable<string>> GetInheritedPermissionsAsync(string roleName)
        {
            var permissions = new HashSet<string>();
            var hierarchy = await GetRoleHierarchyAsync(roleName);
            
            if (hierarchy != null && hierarchy.InheritsPermissions)
            {
                // Pridedame tėvinės rolės teises
                if (!string.IsNullOrEmpty(hierarchy.ParentRoleName))
                {
                    var parentPermissions = await GetInheritedPermissionsAsync(hierarchy.ParentRoleName);
                    foreach (var permission in parentPermissions)
                    {
                        permissions.Add(permission);
                    }
                }

                // Pridedame šios rolės teises
                var rolePermissions = await GetRolePermissionsAsync(roleName);
                foreach (var permission in rolePermissions)
                {
                    permissions.Add(permission);
                }
            }

            return permissions;
        }

        public async Task<IEnumerable<string>> GetChildRolesAsync(string roleName, bool recursive = true)
        {
            var childRoles = new HashSet<string>();
            var hierarchy = await GetRoleHierarchyAsync(roleName);

            if (hierarchy != null)
            {
                foreach (var childRole in hierarchy.ChildRoles)
                {
                    childRoles.Add(childRole);
                    if (recursive)
                    {
                        var grandChildren = await GetChildRolesAsync(childRole);
                        foreach (var grandChild in grandChildren)
                        {
                            childRoles.Add(grandChild);
                        }
                    }
                }
            }

            return childRoles;
        }

        public async Task<IEnumerable<string>> GetParentRolesAsync(string roleName, bool recursive = true)
        {
            var parentRoles = new HashSet<string>();
            var hierarchy = await GetRoleHierarchyAsync(roleName);

            if (hierarchy != null && !string.IsNullOrEmpty(hierarchy.ParentRoleName))
            {
                parentRoles.Add(hierarchy.ParentRoleName);
                if (recursive)
                {
                    var grandParents = await GetParentRolesAsync(hierarchy.ParentRoleName);
                    foreach (var grandParent in grandParents)
                    {
                        parentRoles.Add(grandParent);
                    }
                }
            }

            return parentRoles;
        }

        private async Task AddChildRoleAsync(string parentRoleName, string childRoleName)
        {
            if (_roleHierarchyCache.TryGetValue(parentRoleName, out var parentHierarchy))
            {
                parentHierarchy.ChildRoles.Add(childRoleName);
                await UpdateRoleHierarchyInCsvAsync(parentHierarchy);
            }
        }

        private async Task RemoveChildRoleAsync(string parentRoleName, string childRoleName)
        {
            if (_roleHierarchyCache.TryGetValue(parentRoleName, out var parentHierarchy))
            {
                parentHierarchy.ChildRoles.Remove(childRoleName);
                await UpdateRoleHierarchyInCsvAsync(parentHierarchy);
            }
        }

        private async Task UpdateRoleHierarchyInCsvAsync(RoleHierarchyDto hierarchy)
        {
            var lines = await ReadAllLinesAsync(_roleHierarchyFilePath);
            var hierarchyLines = lines.ToList();
            var index = hierarchyLines.FindIndex(l => l.StartsWith($"\"{hierarchy.RoleName}\""));

            if (index != -1)
            {
                hierarchyLines[index] = hierarchy.ToCsvLine();
                await WriteAllLinesAsync(_roleHierarchyFilePath, hierarchyLines);
            }
        }

        private RoleHierarchyDto ParseRoleHierarchyLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new RoleHierarchyDto
                {
                    RoleName = parts[0].Trim('"'),
                    ParentRoleName = parts[1].Trim('"'),
                    Level = int.Parse(parts[2]),
                    InheritsPermissions = bool.Parse(parts[3]),
                    CreatedAt = DateTime.Parse(parts[4]),
                    CreatedByUser = parts[5].Trim('"'),
                    ChildRoles = parts[6].Trim('"').Split('|', StringSplitOptions.RemoveEmptyEntries).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant rolių hierarchijos CSV eilutę: {Line}", line);
                return null;
            }
        }

        // Validacijos metodai
        private async Task<bool> ValidateHierarchyCyclesAsync(string roleName, string newParentName)
        {
            // Tikriname ar nauja tėvinė rolė nėra tos pačios rolės vaikas
            var childRoles = await GetChildRolesAsync(roleName);
            if (childRoles.Contains(newParentName))
            {
                return false;
            }
            return true;
        }

        private async Task<bool> ValidateHierarchyLevelsAsync(string newParentName)
        {
            // Tikriname ar neviršijame maksimalaus hierarchijos gylio
            var maxLevel = 5; // Konfigūruojama reikšmė
            var parentHierarchy = await GetRoleHierarchyAsync(newParentName);
            if (parentHierarchy != null && parentHierarchy.Level >= maxLevel)
            {
                return false;
            }
            return true;
        }

        // Ataskaitų generavimas
        private async Task<string> GenerateRoleHierarchyReportAsync()
        {
            var report = new StringBuilder();
            report.AppendLine("Rolių hierarchijos ataskaita");
            report.AppendLine("==========================");

            var hierarchyTree = await GetRoleHierarchyTreeAsync();
            foreach (var hierarchy in hierarchyTree.Where(h => h.Level == 0))
            {
                await AppendRoleHierarchyToReportAsync(hierarchy, report, 0);
            }

            return report.ToString();
        }

        private async Task AppendRoleHierarchyToReportAsync(RoleHierarchyDto hierarchy, StringBuilder report, int indent)
        {
            var indentation = new string(' ', indent * 2);
            var role = await GetRoleByNameAsync(hierarchy.RoleName);
            
            report.AppendLine($"{indentation}- {hierarchy.RoleName}");
            report.AppendLine($"{indentation}  Aprašymas: {role?.Description}");
            report.AppendLine($"{indentation}  Lygis: {hierarchy.Level}");
            report.AppendLine($"{indentation}  Paveldi teises: {(hierarchy.InheritsPermissions ? "Taip" : "Ne")}");
            
            var permissions = await GetRolePermissionsAsync(hierarchy.RoleName);
            report.AppendLine($"{indentation}  Teisės: {string.Join(", ", permissions)}");

            foreach (var childRoleName in hierarchy.ChildRoles)
            {
                var childHierarchy = await GetRoleHierarchyAsync(childRoleName);
                if (childHierarchy != null)
                {
                    await AppendRoleHierarchyToReportAsync(childHierarchy, report, indent + 1);
                }
            }
        }
    }
}

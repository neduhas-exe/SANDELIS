using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Users;

namespace WarehouseSystem.Services
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly string _usersFilePath;
        private readonly string _rolesFilePath;
        private static readonly SemaphoreSlim _csvLock = new(1, 1);

        public UserService(ILogger<UserService> logger)
        {
            _logger = logger;
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            _usersFilePath = Path.Combine(baseDirectory, "Data", "users.csv");
            _rolesFilePath = Path.Combine(baseDirectory, "Data", "roles.csv");
            
            Directory.CreateDirectory(Path.Combine(baseDirectory, "Data"));
            InitializeCSVFiles();
        }

        private void InitializeCSVFiles()
        {
            if (!File.Exists(_usersFilePath))
            {
                File.WriteAllText(_usersFilePath, UserDto.GetCsvHeader());
            }
            if (!File.Exists(_rolesFilePath))
            {
                File.WriteAllText(_rolesFilePath, RoleDto.GetCsvHeader());
            }
        }

        // Pagalbinės funkcijos CSV darbui
        private async Task<List<string>> ReadAllLinesAsync(string filePath)
        {
            await _csvLock.WaitAsync();
            try
            {
                using var reader = new StreamReader(filePath);
                var lines = new List<string>();
                while (!reader.EndOfStream)
                {
                    lines.Add(await reader.ReadLineAsync());
                }
                return lines;
            }
            finally
            {
                _csvLock.Release();
            }
        }

        private async Task WriteAllLinesAsync(string filePath, IEnumerable<string> lines)
        {
            await _csvLock.WaitAsync();
            try
            {
                await File.WriteAllLinesAsync(filePath, lines);
            }
            finally
            {
                _csvLock.Release();
            }
        }

        // Vartotojų operacijos
        public async Task<UserDto> CreateUserAsync(CreateUserDto userDto)
        {
            var existingUser = await GetUserByUsernameAsync(userDto.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException($"Vartotojas {userDto.Username} jau egzistuoja");
            }

            var user = new UserDto
            {
                Username = userDto.Username,
                Email = userDto.Email,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Department = userDto.Department,
                Position = userDto.Position,
                EmployeeCode = userDto.EmployeeCode,
                Roles = userDto.AssignedRoles,
                IsActive = userDto.IsActive,
                CreatedAt = DateTime.Now,
                CreatedByUser = userDto.CreatedByUser
            };

            var lines = await ReadAllLinesAsync(_usersFilePath);
            lines.Add(user.ToCsvLine());
            await WriteAllLinesAsync(_usersFilePath, lines);

            return user;
        }

        public async Task<UserDto> GetUserByUsernameAsync(string username)
        {
            var lines = await ReadAllLinesAsync(_usersFilePath);
            var userLine = lines.Skip(1)
                              .FirstOrDefault(l => l.StartsWith($"{username},"));

            return userLine != null ? ParseUserLine(userLine) : null;
        }

        public async Task<UserDto> GetUserByEmailAsync(string email)
        {
            var lines = await ReadAllLinesAsync(_usersFilePath);
            var userLine = lines.Skip(1)
                              .FirstOrDefault(l => l.Split(',')[1].Trim('"') == email);

            return userLine != null ? ParseUserLine(userLine) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var lines = await ReadAllLinesAsync(_usersFilePath);
            return lines.Skip(1)
                       .Select(ParseUserLine)
                       .Where(u => u != null);
        }

        public async Task<UserDto> UpdateUserAsync(UpdateUserDto userDto)
        {
            var lines = await ReadAllLinesAsync(_usersFilePath);
            var userLines = lines.ToList();
            var index = userLines.FindIndex(l => l.StartsWith($"{userDto.Username},"));

            if (index == -1)
            {
                throw new KeyNotFoundException($"Vartotojas {userDto.Username} nerastas");
            }

            var currentUser = ParseUserLine(userLines[index]);
            
            if (userDto.Email != null) currentUser.Email = userDto.Email;
            if (userDto.FirstName != null) currentUser.FirstName = userDto.FirstName;
            if (userDto.LastName != null) currentUser.LastName = userDto.LastName;
            if (userDto.Department != null) currentUser.Department = userDto.Department;
            if (userDto.Position != null) currentUser.Position = userDto.Position;
            if (userDto.IsActive.HasValue) currentUser.IsActive = userDto.IsActive.Value;
            
            currentUser.LastUpdatedAt = DateTime.Now;
            currentUser.LastUpdatedByUser = userDto.UpdatedByUser;

            userLines[index] = currentUser.ToCsvLine();
            await WriteAllLinesAsync(_usersFilePath, userLines);
            return currentUser;
        }

        public async Task<bool> UpdateUserStatusAsync(UpdateUserStatusDto statusDto)
        {
            var lines = await ReadAllLinesAsync(_usersFilePath);
            var userLines = lines.ToList();
            var index = userLines.FindIndex(l => l.StartsWith($"{statusDto.Username},"));

            if (index == -1)
            {
                return false;
            }

            var user = ParseUserLine(userLines[index]);
            user.IsActive = statusDto.IsActive;
            user.LastUpdatedAt = DateTime.Now;
            user.LastUpdatedByUser = statusDto.UpdatedByUser;

            userLines[index] = user.ToCsvLine();
            await WriteAllLinesAsync(_usersFilePath, userLines);

            return true;
        }

        public async Task<bool> UpdateUserRolesAsync(UpdateUserRolesDto rolesDto)
        {
            // Patikriname ar visos rolės egzistuoja
            var existingRoles = await GetAllRolesAsync();
            var validRoles = existingRoles.Select(r => r.RoleName).ToHashSet();
            
            if (!rolesDto.Roles.All(r => validRoles.Contains(r)))
            {
                throw new InvalidOperationException("Kai kurios nurodytos rolės neegzistuoja");
            }

            var lines = await ReadAllLinesAsync(_usersFilePath);
            var userLines = lines.ToList();
            var index = userLines.FindIndex(l => l.StartsWith($"{rolesDto.Username},"));

            if (index == -1)
            {
                return false;
            }

            var user = ParseUserLine(userLines[index]);
            user.Roles = rolesDto.Roles;
            user.LastUpdatedAt = DateTime.Now;
            user.LastUpdatedByUser = rolesDto.UpdatedByUser;

            userLines[index] = user.ToCsvLine();
            await WriteAllLinesAsync(_usersFilePath, userLines);

            return true;
        }

        public async Task<bool> DeleteUserAsync(string username, string deletedByUser, string reason)
        {
            var lines = await ReadAllLinesAsync(_usersFilePath);
            var userLines = lines.ToList();
            var index = userLines.FindIndex(l => l.StartsWith($"{username},"));

            if (index == -1)
            {
                return false;
            }

            var user = ParseUserLine(userLines[index]);
            user.IsActive = false;
            user.LastUpdatedAt = DateTime.Now;
            user.LastUpdatedByUser = deletedByUser;

            userLines[index] = user.ToCsvLine();
            await WriteAllLinesAsync(_usersFilePath, userLines);

            return true;
        }

        // Rolių operacijos
        public async Task<RoleDto> CreateRoleAsync(CreateRoleDto roleDto)
        {
            var existingRole = await GetRoleByNameAsync(roleDto.RoleName);
            if (existingRole != null)
            {
                throw new InvalidOperationException($"Rolė {roleDto.RoleName} jau egzistuoja");
            }

            var role = new RoleDto
            {
                RoleName = roleDto.RoleName,
                Description = roleDto.Description,
                Permissions = roleDto.Permissions,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedByUser = roleDto.CreatedByUser
            };

            var lines = await ReadAllLinesAsync(_rolesFilePath);
            lines.Add(role.ToCsvLine());
            await WriteAllLinesAsync(_rolesFilePath, lines);

            return role;
        }

        public async Task<RoleDto> GetRoleByNameAsync(string roleName)
        {
            var lines = await ReadAllLinesAsync(_rolesFilePath);
            var roleLine = lines.Skip(1)
                              .FirstOrDefault(l => l.Split(',')[0].Trim('"') == roleName);

            return roleLine != null ? ParseRoleLine(roleLine) : null;
        }

        public async Task<IEnumerable<RoleDto>> GetAllRolesAsync()
        {
            var lines = await ReadAllLinesAsync(_rolesFilePath);
            return lines.Skip(1)
                       .Select(ParseRoleLine)
                       .Where(r => r != null);
        }

        public async Task<RoleDto> UpdateRoleAsync(UpdateRoleDto roleDto)
        {
            var lines = await ReadAllLinesAsync(_rolesFilePath);
            var roleLines = lines.ToList();
            var index = roleLines.FindIndex(l => l.Split(',')[0].Trim('"') == roleDto.RoleName);

            if (index == -1)
            {
                throw new KeyNotFoundException($"Rolė {roleDto.RoleName} nerasta");
            }

            var currentRole = ParseRoleLine(roleLines[index]);
            
            if (roleDto.Description != null) currentRole.Description = roleDto.Description;
            if (roleDto.Permissions != null) currentRole.Permissions = roleDto.Permissions;

            roleLines[index] = currentRole.ToCsvLine();
            await WriteAllLinesAsync(_rolesFilePath, roleLines);

            return currentRole;
        }

        public async Task<bool> DeleteRoleAsync(string roleName, string deletedByUser, string reason)
        {
            // Patikriname ar rolė nėra naudojama
            var users = await GetAllUsersAsync();
            if (users.Any(u => u.Roles.Contains(roleName)))
            {
                throw new InvalidOperationException("Negalima ištrinti rolės, kuri yra priskirta vartotojams");
            }

            var lines = await ReadAllLinesAsync(_rolesFilePath);
            var roleLines = lines.Where(l => !l.Split(',')[0].Trim('"').Equals(roleName));
            await WriteAllLinesAsync(_rolesFilePath, roleLines);

            return true;
        }

        // Teisių valdymas
        public async Task<bool> HasPermissionAsync(string username, string permission)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null || !user.IsActive)
            {
                return false;
            }

            var userPermissions = await GetUserPermissionsAsync(username);
            return userPermissions.Contains(permission);
        }

        public async Task<IEnumerable<string>> GetUserPermissionsAsync(string username)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
            {
                return Enumerable.Empty<string>();
            }

            var allPermissions = new HashSet<string>();
            foreach (var roleName in user.Roles)
            {
                var rolePermissions = await GetRolePermissionsAsync(roleName);
                foreach (var permission in rolePermissions)
                {
                    allPermissions.Add(permission);
                }
            }

            return allPermissions;
        }

        public async Task<IEnumerable<string>> GetRolePermissionsAsync(string roleName)
        {
            var role = await GetRoleByNameAsync(roleName);
            return role?.Permissions ?? Enumerable.Empty<string>();
        }

        // CSV operacijos
        public async Task ExportUsersToCsvAsync(string filePath)
        {
            var users = await GetAllUsersAsync();
            var lines = new List<string> { UserDto.GetCsvHeader() };
            lines.AddRange(users.Select(u => u.ToCsvLine()));
            await File.WriteAllLinesAsync(filePath, lines);
        }

        public async Task ImportUsersFromCsvAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var header = lines.First();
            
            if (header != UserDto.GetCsvHeader())
            {
                throw new InvalidOperationException("CSV failo struktūra neatitinka reikalaujamos struktūros");
            }

            var currentUsers = await ReadAllLinesAsync(_usersFilePath);
            var newUsers = lines.Skip(1)
                               .Where(l => !currentUsers.Contains(l));

            currentUsers.AddRange(newUsers);
            await WriteAllLinesAsync(_usersFilePath, currentUsers);
        }

        public async Task ExportRolesToCsvAsync(string filePath)
        {
            var roles = await GetAllRolesAsync();
            var lines = new List<string> { RoleDto.GetCsvHeader() };
            lines.AddRange(roles.Select(r => r.ToCsvLine()));
            await File.WriteAllLinesAsync(filePath, lines);
        }

        public async Task ImportRolesFromCsvAsync(string filePath)
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            var header = lines.First();
            
            if (header != RoleDto.GetCsvHeader())
            {
                throw new InvalidOperationException("CSV failo struktūra neatitinka reikalaujamos struktūros");
            }

            var currentRoles = await ReadAllLinesAsync(_rolesFilePath);
            var newRoles = lines.Skip(1)
                               .Where(l => !currentRoles.Contains(l));

            currentRoles.AddRange(newRoles);
            await WriteAllLinesAsync(_rolesFilePath, currentRoles);
        }

        // Auditas ir ataskaitos
        public async Task<IEnumerable<UserDto>> GetUsersByDepartmentAsync(string department)
        {
            var users = await GetAllUsersAsync();
            return users.Where(u => u.Department == department);
        }

        public async Task<IEnumerable<UserDto>> GetUsersByRoleAsync(string roleName)
        {
            var users = await GetAllUsersAsync();
            return users.Where(u => u.Roles.Contains(roleName));
        }

        public async Task<IEnumerable<UserDto>> GetInactiveUsersAsync()
        {
            var users = await GetAllUsersAsync();
            return users.Where(u => !u.IsActive);
        }

        public async Task<string> GenerateUserActivityReportAsync(string username)
        {
            var user = await GetUserByUsernameAsync(username);
            if (user == null)
            {
                throw new KeyNotFoundException($"Vartotojas {username} nerastas");
            }

            var report = new StringBuilder();
            report.AppendLine($"Vartotojo {username} veiklos ataskaita");
            report.AppendLine("----------------------------------------");
            report.AppendLine($"Vardas, pavardė: {user.FullName}");
            report.AppendLine($"Departamentas: {user.Department}");
            report.AppendLine($"Pareigos: {user.Position}");
            report.AppendLine($"Rolės: {string.Join(", ", user.Roles)}");
            report.AppendLine($"Statusas: {(user.IsActive ? "Aktyvus" : "Neaktyvus")}");
            report.AppendLine($"Sukurtas: {user.CreatedAt:yyyy-MM-dd HH:mm:ss} ({user.CreatedByUser})");
            
            if (user.LastUpdatedAt.HasValue)
            {
                report.AppendLine($"Paskutinis atnaujinimas: {user.LastUpdatedAt:yyyy-MM-dd HH:mm:ss} ({user.LastUpdatedByUser})");
            }

            var permissions = await GetUserPermissionsAsync(username);
            report.AppendLine("\nTurimos teisės:");
            foreach (var permission in permissions)
            {
                report.AppendLine($"- {permission}");
            }

            return report.ToString();
        }

        public async Task<string> GenerateRoleAssignmentReportAsync()
        {
            var users = await GetAllUsersAsync();
            var roles = await GetAllRolesAsync();
            
            var report = new StringBuilder();
            report.AppendLine("Rolių priskyrimo ataskaita");
            report.AppendLine("----------------------------------------");

            foreach (var role in roles)
            {
                var usersWithRole = users.Where(u => u.Roles.Contains(role.RoleName));
                report.AppendLine($"\nRolė: {role.RoleName}");
                report.AppendLine($"Aprašymas: {role.Description}");
                report.AppendLine($"Vartotojų skaičius: {usersWithRole.Count()}");
                report.AppendLine("Vartotojai:");
                foreach (var user in usersWithRole)
                {
                    report.AppendLine($"- {user.FullName} ({user.Username})");
                }
            }

            return report.ToString();
        }

        // Pagalbinės funkcijos
        private UserDto ParseUserLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new UserDto
                {
                    Username = parts[0],
                    Email = parts[1].Trim('"'),
                    FirstName = parts[2].Trim('"'),
                    LastName = parts[3].Trim('"'),
                    Department = parts[4].Trim('"'),
                    Position = parts[5].Trim('"'),
                    EmployeeCode = parts[6].Trim('"'),
                    Roles = parts[7].Trim('"').Split('|').ToList(),
                    IsActive = bool.Parse(parts[8]),
                    CreatedAt = DateTime.Parse(parts[9]),
                    CreatedByUser = parts[10].Trim('"'),
                    LastUpdatedAt = string.IsNullOrEmpty(parts[11]) ? null : DateTime.Parse(parts[11]),
                    LastUpdatedByUser = parts[12].Trim('"')
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant vartotojo CSV eilutę: {Line}", line);
                return null;
            }
        }

        private RoleDto ParseRoleLine(string line)
        {
            try
            {
                var parts = line.Split(',');
                return new RoleDto
                {
                    RoleName = parts[0].Trim('"'),
                    Description = parts[1].Trim('"'),
                    Permissions = parts[2].Trim('"').Split('|').ToList(),
                    IsActive = bool.Parse(parts[3]),
                    CreatedAt = DateTime.Parse(parts[4]),
                    CreatedByUser = parts[5].Trim('"')
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida apdorojant rolės CSV eilutę: {Line}", line);
                return null;
            }
        }
    }
}

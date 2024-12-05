using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WarehouseSystem.Services.Interfaces;
using Presentation.DTOs.Users;

namespace WarehouseSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserService userService,
            ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        #region Vartotojų valdymas

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                var users = await _userService.GetAllUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant vartotojų sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("{username}")]
        public async Task<ActionResult<UserDto>> GetUser(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return NotFound($"Vartotojas {username} nerastas");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant vartotoją {Username}", username);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("email/{email}")]
        public async Task<ActionResult<UserDto>> GetUserByEmail(string email)
        {
            try
            {
                var user = await _userService.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return NotFound($"Vartotojas su el. paštu {email} nerastas");
                }
                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida ieškant vartotojo pagal el. paštą {Email}", email);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto userDto)
        {
            try
            {
                var createdUser = await _userService.CreateUserAsync(userDto);
                return CreatedAtAction(
                    nameof(GetUser),
                    new { username = createdUser.Username },
                    createdUser
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant naują vartotoją");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPut("{username}")]
        public async Task<ActionResult<UserDto>> UpdateUser(
            string username,
            [FromBody] UpdateUserDto userDto)
        {
            try
            {
                if (username != userDto.Username)
                {
                    return BadRequest("Vartotojo vardas nesutampa su URL nurodytu vardu");
                }

                var updatedUser = await _userService.UpdateUserAsync(userDto);
                return Ok(updatedUser);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Vartotojas {username} nerastas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant vartotoją {Username}", username);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPut("{username}/status")]
        public async Task<IActionResult> UpdateUserStatus(
            string username,
            [FromBody] UpdateUserStatusDto statusDto)
        {
            try
            {
                if (username != statusDto.Username)
                {
                    return BadRequest("Vartotojo vardas nesutampa su URL nurodytu vardu");
                }

                var success = await _userService.UpdateUserStatusAsync(statusDto);
                if (!success)
                {
                    return NotFound($"Vartotojas {username} nerastas");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant vartotojo {Username} statusą", username);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpDelete("{username}")]
        public async Task<IActionResult> DeleteUser(
            string username,
            [FromBody] DeleteUserRequest request)
        {
            try
            {
                var success = await _userService.DeleteUserAsync(
                    username,
                    request.DeletedByUser,
                    request.Reason
                );

                if (!success)
                {
                    return NotFound($"Vartotojas {username} nerastas");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant vartotoją {Username}", username);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Rolių valdymas

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            try
            {
                var roles = await _userService.GetAllRolesAsync();
                return Ok(roles);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant rolių sąrašą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("roles/{roleName}")]
        public async Task<ActionResult<RoleDto>> GetRole(string roleName)
        {
            try
            {
                var role = await _userService.GetRoleByNameAsync(roleName);
                if (role == null)
                {
                    return NotFound($"Rolė {roleName} nerasta");
                }
                return Ok(role);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant rolę {RoleName}", roleName);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("roles")]
        public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleDto roleDto)
        {
            try
            {
                var createdRole = await _userService.CreateRoleAsync(roleDto);
                return CreatedAtAction(
                    nameof(GetRole),
                    new { roleName = createdRole.RoleName },
                    createdRole
                );
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida kuriant naują rolę");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPut("roles/{roleName}")]
        public async Task<ActionResult<RoleDto>> UpdateRole(
            string roleName,
            [FromBody] UpdateRoleDto roleDto)
        {
            try
            {
                if (roleName != roleDto.RoleName)
                {
                    return BadRequest("Rolės pavadinimas nesutampa su URL nurodytu pavadinimu");
                }

                var updatedRole = await _userService.UpdateRoleAsync(roleDto);
                return Ok(updatedRole);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Rolė {roleName} nerasta");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant rolę {RoleName}", roleName);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpDelete("roles/{roleName}")]
        public async Task<IActionResult> DeleteRole(
            string roleName,
            [FromBody] DeleteRoleRequest request)
        {
            try
            {
                var success = await _userService.DeleteRoleAsync(
                    roleName,
                    request.DeletedByUser,
                    request.Reason
                );

                if (!success)
                {
                    return NotFound($"Rolė {roleName} nerasta");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida trinant rolę {RoleName}", roleName);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPut("{username}/roles")]
        public async Task<IActionResult> UpdateUserRoles(
            string username,
            [FromBody] UpdateUserRolesDto rolesDto)
        {
            try
            {
                if (username != rolesDto.Username)
                {
                    return BadRequest("Vartotojo vardas nesutampa su URL nurodytu vardu");
                }

                var success = await _userService.UpdateUserRolesAsync(rolesDto);
                if (!success)
                {
                    return NotFound($"Vartotojas {username} nerastas");
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida atnaujinant vartotojo {Username} roles", username);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Teisių tikrinimas

        [HttpGet("{username}/permissions")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserPermissions(string username)
        {
            try
            {
                var permissions = await _userService.GetUserPermissionsAsync(username);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant vartotojo {Username} teises", username);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("roles/{roleName}/permissions")]
        public async Task<ActionResult<IEnumerable<string>>> GetRolePermissions(string roleName)
        {
            try
            {
                var permissions = await _userService.GetRolePermissionsAsync(roleName);
                return Ok(permissions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant rolės {RoleName} teises", roleName);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region CSV operacijos

        [HttpGet("export")]
        public async Task<IActionResult> ExportUsers()
        {
            try
            {
                var fileName = $"users_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var tempPath = Path.Combine(Path.GetTempPath(), fileName);

                await _userService.ExportUsersToCsvAsync(tempPath);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
                System.IO.File.Delete(tempPath);

                return File(
                    fileBytes,
                    "text/csv",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida eksportuojant vartotojus į CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportUsers(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nepateiktas CSV failas");
                }

                var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);

                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _userService.ImportUsersFromCsvAsync(tempPath);
                System.IO.File.Delete(tempPath);

                return Ok("CSV importuotas sėkmingai");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida importuojant vartotojus iš CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("roles/export")]
        public async Task<IActionResult> ExportRoles()
        {
            try
            {
                var fileName = $"roles_{DateTime.Now:yyyyMMddHHmmss}.csv";
                var tempPath = Path.Combine(Path.GetTempPath(), fileName);

                await _userService.ExportRolesToCsvAsync(tempPath);

                var fileBytes = await System.IO.File.ReadAllBytesAsync(tempPath);
                System.IO.File.Delete(tempPath);

                return File(
                    fileBytes,
                    "text/csv",
                    fileName
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida eksportuojant roles į CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpPost("roles/import")]
        public async Task<IActionResult> ImportRoles(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("Nepateiktas CSV failas");
                }

                var tempPath = Path.Combine(Path.GetTempPath(), file.FileName);

                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                await _userService.ImportRolesFromCsvAsync(tempPath);
                System.IO.File.Delete(tempPath);

                return Ok("CSV importuotas sėkmingai");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida importuojant roles iš CSV");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion

        #region Ataskaitos ir statistika

        [HttpGet("department/{department}")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByDepartment(string department)
        {
            try
            {
                var users = await _userService.GetUsersByDepartmentAsync(department);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant departamento {Department} vartotojus", department);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("role/{roleName}/users")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsersByRole(string roleName)
        {
            try
            {
                var users = await _userService.GetUsersByRoleAsync(roleName);
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant vartotojus su role {RoleName}", roleName);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("inactive")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetInactiveUsers()
        {
            try
            {
                var users = await _userService.GetInactiveUsersAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida gaunant neaktyvius vartotojus");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("{username}/activity-report")]
        public async Task<ActionResult<string>> GenerateUserActivityReport(string username)
        {
            try
            {
                var report = await _userService.GenerateUserActivityReportAsync(username);
                return Ok(report);
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"Vartotojas {username} nerastas");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida generuojant vartotojo {Username} veiklos ataskaitą", username);
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        [HttpGet("roles/assignment-report")]
        public async Task<ActionResult<string>> GenerateRoleAssignmentReport()
        {
            try
            {
                var report = await _userService.GenerateRoleAssignmentReportAsync();
                return Ok(report);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Klaida generuojant rolių priskyrimo ataskaitą");
                return StatusCode(500, "Įvyko vidinė serverio klaida");
            }
        }

        #endregion
    }

    public class DeleteUserRequest
    {
        public string DeletedByUser { get; set; }
        public string Reason { get; set; }
    }

    public class DeleteRoleRequest
    {
        public string DeletedByUser { get; set; }
        public string Reason { get; set; }
    }
}

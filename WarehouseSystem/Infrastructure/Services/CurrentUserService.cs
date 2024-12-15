// Infrastructure/Services/CurrentUserService.cs
using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Infrastructure.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public long UserId
    {
        get
        {
            // Čia reikėtų pritaikyti pagal jūsų autentifikacijos sistemą
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userId != null ? long.Parse(userId) : 0;
        }
    }

    public string UserName
    {
        get
        {
            return _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
        }
    }
}

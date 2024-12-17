using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Domain.Models
{
    public interface IHttpContextAccessor
    {
        HttpContext HttpContext { get; }
    }

    public interface ICurrentUserService
    {
        long UserId { get; }
        string UserName { get; }
    }

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
                var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier);
                return claim != null ? long.Parse(claim.Value) : 0;
            }
        }

        public string UserName => _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "System";
    }
}

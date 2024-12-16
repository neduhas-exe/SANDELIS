// Infrastructure/Services/CurrentUserService.cs

namespace Infrastructure.Services
{
    internal interface IHttpContextAccessor
    {
        ReadOnlySpan<byte> HttpContext { get; set; }
    }
}
using Domain.Interfaces;

namespace Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        public string GetCurrentUserLogin()
        {
            // For testing purposes, return a hardcoded value
            return "neduhas-exe";
        }

        public DateTime GetCurrentDateTime()
        {
            // Return UTC time for consistency
            return DateTime.UtcNow;
        }

        public bool IsAuthenticated()
        {
            // For Swagger testing, always return true
            return true;
        }
    }
}
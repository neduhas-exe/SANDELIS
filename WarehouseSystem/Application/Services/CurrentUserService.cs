using Application.Services.Interfaces;
using Domain.Models;

namespace Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        // Hardcoded reikšmės testavimui
        public long UserId => 1; // Grąžins fiksuotą ID = 1
        public string UserName => "TestUser"; // Grąžins fiksuotą vardą "TestUser"
    }
}
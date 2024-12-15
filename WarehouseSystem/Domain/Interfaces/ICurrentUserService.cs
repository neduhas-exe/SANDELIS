// Domain/Interfaces/ICurrentUserService.cs
namespace Domain.Interfaces;

public interface ICurrentUserService
{
    long UserId { get; }
    string UserName { get; }
}

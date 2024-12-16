// Application/Interfaces/IUserService.cs
using Domain.Models;

public interface IUserService
{
    User GetById(long id);
    User GetByUsername(string username);
    List<User> List();
    User Create(User user);
    User Update(User user);
}

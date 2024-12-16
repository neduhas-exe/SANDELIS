// Infrastructure/Interfaces/IUserRepository.cs
using YourNamespace.Models; // Add this using directive

public interface IUserRepository
{
    User GetById(long id);
    User GetByUsername(string username);
    List<User> List();
    User Create(User user);
    User Update(User user);
}

// Infrastructure/Interfaces/IUserRepository.cs
public interface IUserRepository
{
    User GetById(long id);
    User GetByUsername(string username);
    List<User> List();
    User Create(User user);
    User Update(User user);
}

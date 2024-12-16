// Application/Interfaces/IUserService.cs
public interface IUserService
{
    User GetById(long id);
    User GetByUsername(string username);
    List<User> List();
    User Create(User user);
    User Update(User user);
}

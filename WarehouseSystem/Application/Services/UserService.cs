// Application/Services/UserService.cs
using Domain.Models;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    
    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
    
    public User GetById(long id)
    {
        return _userRepository.GetById(id);
    }
    
    public User GetByUsername(string username)
    {
        return _userRepository.GetByUsername(username);
    }
    
    public List<User> List()
    {
        return _userRepository.List();
    }
    
    public User Create(User user)
    {
        user.CreatedDate = DateTime.UtcNow;
        return _userRepository.Create(user);
    }
    
    public User Update(User user)
    {
        return _userRepository.Update(user);
    }
}

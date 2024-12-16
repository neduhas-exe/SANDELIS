// Infrastructure/Repositories/UserRepository.cs
using Domain.Models;
using Infrastructure.Interfaces;

namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;
    
    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public User GetById(long id)
    {
        return _context.Users.Find(id);
    }
    
    public User GetByUsername(string username)
    {
        return _context.Users.FirstOrDefault(u => u.UserName == username);
    }
    
    public List<User> List()
    {
        return _context.Users.Where(u => u.IsActive).ToList();
    }
    
    public User Create(User user)
    {
        _context.Users.Add(user);
        _context.SaveChanges();
        return user;
    }
    
    public User Update(User user)
    {
        var existingUser = _context.Users.Find(user.Id);
        if (existingUser == null)
            return null;
            
        _context.Entry(existingUser).CurrentValues.SetValues(user);
        _context.SaveChanges();
        return existingUser;
    }
}

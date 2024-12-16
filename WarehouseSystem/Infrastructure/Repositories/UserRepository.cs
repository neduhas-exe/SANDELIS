// Infrastructure/Repositories/UserRepository.cs
using CsvHelper;
using CsvHelper.Configuration;
using Domain.Models;


namespace Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly string _filePath;
    private readonly CsvConfiguration _csvConfig;

    public UserRepository(string filePath)
    {
        _filePath = filePath;
        _csvConfig = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };
    }

    public User GetById(long id)
    {
        var users = ReadUsersFromFile();
        return users.FirstOrDefault(u => u.Id == id);
    }

    public User GetByUsername(string username)
    {
        var users = ReadUsersFromFile();
        return users.FirstOrDefault(u => u.UserName == username);
    }

    public List<User> List()
    {
        return ReadUsersFromFile().Where(u => u.IsActive).ToList();
    }

    public User Create(User user)
    {
        var users = ReadUsersFromFile();
        users.Add(user);
        WriteUsersToFile(users);
        return user;
    }

    public User Update(User user)
    {
        var users = ReadUsersFromFile();
        var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
        if (existingUser == null)
            return null;

        users.Remove(existingUser);
        users.Add(user);
        WriteUsersToFile(users);
        return user;
    }

    private List<User> ReadUsersFromFile()
    {
        if (!File.Exists(_filePath))
            return new List<User>();

        using (var reader = new StreamReader(_filePath))
        using (var csv = new CsvReader(reader, _csvConfig))
        {
            return csv.GetRecords<User>().ToList();
        }
    }

    private void WriteUsersToFile(List<User> users)
    {
        using (var writer = new StreamWriter(_filePath))
        using (var csv = new CsvWriter(writer, _csvConfig))
        {
            csv.WriteRecords(users);
        }
    }
}

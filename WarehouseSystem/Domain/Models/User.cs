// Domain/Models/User.cs
namespace Domain.Models;

public class User
{
    public long Id { get; set; }
    
    /// <summary>
    /// Vartotojo prisijungimo vardas
    /// </summary>
    public string UserName { get; set; }
    
    /// <summary>
    /// Vartotojo vardas
    /// </summary>
    public string FirstName { get; set; }
    
    /// <summary>
    /// Vartotojo pavardė
    /// </summary>
    public string LastName { get; set; }
    
    /// <summary>
    /// Vartotojo el. paštas
    /// </summary>
    public string Email { get; set; }
    
    /// <summary>
    /// Ar vartotojas aktyvus
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    /// <summary>
    /// Vartotojo sukūrimo data
    /// </summary>
    public DateTime CreatedDate { get; set; }
    
    /// <summary>
    /// Paskutinio prisijungimo data
    /// </summary>
    public DateTime? LastLoginDate { get; set; }

    public static implicit operator string(User v)
    {
        throw new NotImplementedException();
    }
}

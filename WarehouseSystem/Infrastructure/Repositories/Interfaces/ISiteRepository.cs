// Path: WarehouseSystem/Infrastructure/Repositories/Interfaces/ISiteRepository.cs
using Domain.Models;

public interface ISiteRepository
{
    Site Get(long id);
    List<Site> List();
    Site Create(Site site);
}
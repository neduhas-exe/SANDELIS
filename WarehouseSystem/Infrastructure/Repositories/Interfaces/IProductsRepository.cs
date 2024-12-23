using Domain.Models;

namespace Infrastructure.Repositories.Interfaces;

public interface IProductsRepository
{
    public Product Get(long id);
    public List<Product> List();
    public Product Create(Product product);
}
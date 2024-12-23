using Domain.Models;

namespace Application.Services.Interfaces;

public interface IProductsService
{
    public Product Get(long id);
    public List<Product> List();
    public Product Create(Product product);
}
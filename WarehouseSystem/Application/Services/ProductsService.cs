using Application.Services.Interfaces;
using Domain.Models;
using Infrastructure.Repositories.Interfaces;

namespace Application.Services;

public class ProductsService(IProductsRepository productsRepository) : IProductsService
{
    private readonly IProductsRepository _productsRepository = productsRepository;

    public Product Get(long id) => _productsRepository.Get(id);

    public List<Product> List() => _productsRepository.List();

    public Product Create(Product product) => _productsRepository.Create(product);
}

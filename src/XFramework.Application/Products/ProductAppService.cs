using XFramework.Application.Contracts.Products;

namespace XFramework.Application.Products;

public class ProductAppService
    : IProductAppService
{
    public Task<string> GetNameAsync()
    {
        return Task.FromResult("Test Product");
    }
}
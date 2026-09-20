using XFramework.Application.Contracts.Abstractions;

namespace XFramework.Application.Contracts.Products;

public interface IProductAppService
    : IApplicationService
{
    Task<string> GetNameAsync();
}
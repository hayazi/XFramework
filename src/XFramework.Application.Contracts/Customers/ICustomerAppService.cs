using XFramework.Application.Contracts.Abstractions;

namespace XFramework.Application.Contracts.Customers;

public interface ICustomerAppService
    : ICrudAppService<
        CustomerDto,
        Guid,
        CustomerCreateDto,
        CustomerUpdateDto>
{
}
using XFramework.Application.Abstractions;
using XFramework.Application.Contracts.Customers;
using XFramework.Application.Services;
using XFramework.Domain.Customers;
using XFramework.Application.Attributes;
namespace XFramework.Application.Customers;

[Authorize("CRM.Customer")]
[UnitOfWork]
[Validate]
public class CustomerAppService
    : CrudAppService<
        Customer,
        CustomerDto,
        Guid,
        CustomerCreateDto,
        CustomerUpdateDto>,
      ICustomerAppService
{
    public CustomerAppService(
        IRepository<Customer, Guid> repository,
        IUnitOfWork unitOfWork)
        : base(repository, unitOfWork)
    {
    }

    protected override CustomerDto MapToDto(
        Customer entity)
    {
        return new CustomerDto
        {
            Id = entity.Id,
            Code = entity.Code,
            Name = entity.Name,
            Phone = entity.Phone
        };
    }

    protected override Task<Customer> MapToEntityAsync(
        CustomerCreateDto input)
    {
        var customer = new Customer(
            input.Code,
            input.Name,
            input.Phone);

        return Task.FromResult(customer);
    }

    protected override Task MapToEntityAsync(
        CustomerUpdateDto input,
        Customer entity)
    {
        entity.Update(
            input.Name,
            input.Phone);

        return Task.CompletedTask;
    }
}
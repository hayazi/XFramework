using XFramework.Application.Abstractions;
using XFramework.Application.Attributes;
using XFramework.Application.Contracts.Customers;
using XFramework.Application.Services;
using XFramework.Domain.Customers;
namespace XFramework.Application.Customers;
[Validate]
public sealed class CustomerAppService(IRepository<Customer,Guid> repository,IUnitOfWork unitOfWork):CrudAppService<Customer,CustomerDto,Guid,CustomerCreateDto,CustomerUpdateDto>(repository,unitOfWork),ICustomerAppService
{ protected override CustomerDto MapToDto(Customer e)=>new(){Id=e.Id,Code=e.Code,Name=e.Name,Phone=e.Phone}; protected override Task<Customer> MapToEntityAsync(CustomerCreateDto i,CancellationToken ct)=>Task.FromResult(new Customer(i.Code,i.Name,i.Phone)); protected override Task MapToEntityAsync(CustomerUpdateDto i,Customer e,CancellationToken ct){e.Update(i.Name,i.Phone);return Task.CompletedTask;} }

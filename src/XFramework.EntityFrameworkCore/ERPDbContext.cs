using Microsoft.EntityFrameworkCore;
using XFramework.Domain.Customers;

namespace XFramework.EntityFrameworkCore;

public class ERPDbContext : XFrameworkDbContext
{
    public ERPDbContext(
        DbContextOptions<ERPDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers => Set<Customer>();
}
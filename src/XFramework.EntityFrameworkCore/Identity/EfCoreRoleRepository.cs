using Microsoft.EntityFrameworkCore;
using XFramework.Application.Abstractions;
using XFramework.Domain.Identity;
namespace XFramework.EntityFrameworkCore.Identity;
public sealed class EfCoreRoleRepository(XFrameworkIdentityDbContext db):IRoleRepository
{ 
    public Task<Role?> GetAsync(Guid id,CancellationToken ct=default)=>db.ApplicationRoles.FirstOrDefaultAsync(x=>x.Id==id,ct); 
    public Task<Role?> FirstOrDefaultAsync(IQueryable<Role> q,CancellationToken ct=default)=>q.FirstOrDefaultAsync(ct);
    public Task<Role?> SingleOrDefaultAsync(IQueryable<Role> q,CancellationToken ct=default)=>q.SingleOrDefaultAsync(ct);
    public IQueryable<Role> GetQueryable()=>db.ApplicationRoles; 
    public Task<int> CountAsync(IQueryable<Role> q,CancellationToken ct=default)=>q.CountAsync(ct); 
    public Task<List<Role>> ToListAsync(IQueryable<Role> q,CancellationToken ct=default)=>q.ToListAsync(ct); 
    public async Task AddAsync(Role e,CancellationToken ct=default)=>await db.ApplicationRoles.AddAsync(e,ct); 
    public Task UpdateAsync(Role e,CancellationToken ct=default){db.ApplicationRoles.Update(e);return Task.CompletedTask;} 
    public Task DeleteAsync(Role e,CancellationToken ct=default){db.ApplicationRoles.Remove(e);return Task.CompletedTask;} 
    public async Task<IReadOnlyList<Permission>> GetPermissionsAsync(Guid roleId,CancellationToken ct=default)=>await (from rp in db.ApplicationRolePermissions join p in db.ApplicationPermissions on rp.PermissionId equals p.Id where rp.RoleId==roleId orderby p.Name select p).AsNoTracking().ToListAsync(ct); 
    public async Task SetPermissionsAsync(Guid roleId,IReadOnlyCollection<Guid> ids,CancellationToken ct=default){var existing=await db.ApplicationRolePermissions.Where(x=>x.RoleId==roleId).ToListAsync(ct);db.ApplicationRolePermissions.RemoveRange(existing);await db.ApplicationRolePermissions.AddRangeAsync(ids.Distinct().Select(x=>RolePermission.Create(roleId,x)),ct);}
}

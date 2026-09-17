using Microsoft.EntityFrameworkCore;
using XFramework.Domain.Authorization;
using XFramework.EntityFrameworkCore.Identity;
namespace XFramework.EntityFrameworkCore.Authorization;
public sealed class EfCorePermissionRepository(XFrameworkIdentityDbContext db):IPermissionRepository
{ public Task<bool> IsGrantedAsync(Guid userId,string permissionName,CancellationToken ct=default)=>(from ur in db.ApplicationUserRoles join rp in db.ApplicationRolePermissions on ur.RoleId equals rp.RoleId join p in db.ApplicationPermissions on rp.PermissionId equals p.Id join r in db.ApplicationRoles on ur.RoleId equals r.Id where ur.UserId==userId && r.IsActive && p.IsEnabled && p.Name==permissionName select p.Id).AnyAsync(ct); }

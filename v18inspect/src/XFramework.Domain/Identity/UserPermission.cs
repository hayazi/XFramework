namespace XFramework.Domain.Identity;
public sealed class UserPermission{private UserPermission(){} public Guid UserId{get;private set;} public Guid PermissionId{get;private set;} public User User{get;private set;}=null!; public Permission Permission{get;private set;}=null!; public static UserPermission Create(Guid userId,Guid permissionId)=>new(){UserId=userId,PermissionId=permissionId};}

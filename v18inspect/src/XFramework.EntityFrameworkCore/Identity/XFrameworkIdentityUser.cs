using Microsoft.AspNetCore.Identity;

namespace XFramework.EntityFrameworkCore.Identity;

public sealed class XFrameworkIdentityUser
    : IdentityUser<Guid>
{
    public Guid ApplicationUserId { get; set; }
}
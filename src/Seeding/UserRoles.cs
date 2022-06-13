using System;
using Microsoft.AspNetCore.Identity;

namespace Seeding;

public static class UserRoles
{
    public static readonly IdentityUserRole<Guid> AdminUserRole = new()
    {
        UserId = Users.AdminUser.Id,
        RoleId = Roles.AdminRole.Id
    };
}
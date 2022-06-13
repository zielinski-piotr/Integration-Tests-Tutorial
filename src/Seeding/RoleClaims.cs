using System;
using System.Collections.Generic;
using Auth.Common;
using Microsoft.AspNetCore.Identity;

namespace Seeding;

public static class RoleClaims
{
    public static readonly IReadOnlyList<IdentityRoleClaim<Guid>> AdminRoleClaims = new List<IdentityRoleClaim<Guid>>
    {
        new()
        {
            Id = 10001,
            RoleId = Roles.AdminRole.Id,
            ClaimType = PermissionClaim.PermissionClaimType,
            ClaimValue = UserPermission.CanCreateHousePermission.ToString()
        },
        new()
        {
            Id = 10002,
            RoleId = Roles.AdminRole.Id,
            ClaimType = PermissionClaim.PermissionClaimType,
            ClaimValue = UserPermission.CanRemoveHousePermission.ToString()
        },
        new()
        {
            Id = 10003,
            RoleId = Roles.AdminRole.Id,
            ClaimType = PermissionClaim.PermissionClaimType,
            ClaimValue = UserPermission.CanGetHousePermission.ToString()
        },
        new()
        {
            Id = 10004,
            RoleId = Roles.AdminRole.Id,
            ClaimType = PermissionClaim.PermissionClaimType,
            ClaimValue = UserPermission.CanGetHousesPermission.ToString()
        },
        
        new()
        {
            Id = 10005,
            RoleId = Roles.AdminRole.Id,
            ClaimType = PermissionClaim.PermissionClaimType,
            ClaimValue = UserPermission.CanUpdateHousePermission.ToString()
        }
    };
}
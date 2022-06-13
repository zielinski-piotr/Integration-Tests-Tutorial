using System;
using Auth.Domain;
using Microsoft.AspNetCore.Identity;

namespace Seeding;

public static class Users
{
    private const string AdminUserName = "test@grpc.test";
    public const string AdminPassword = "p@sSw0rd123";

    public static readonly SampleIdentityUser AdminUser = new()
    {
        Id = Guid.Parse("790EE045-9894-4865-BFF5-6737337C3CA4"),
        UserName = AdminUserName,
        Email = AdminUserName,
        EmailConfirmed = true,
        TwoFactorEnabled = false,
        NormalizedUserName = AdminUserName.Normalize().ToUpper(),
        NormalizedEmail = AdminUserName.Normalize().ToUpper(),
        SecurityStamp = Guid.Parse("88F0112B-2941-4F74-AA29-1F26056C3778").ToString()
    };

    static Users()
    {
        var passwordHasher = new PasswordHasher<SampleIdentityUser>();
        AdminUser.PasswordHash = passwordHasher.HashPassword(AdminUser, AdminPassword);
    }
}
using System;
using Auth.Domain;

namespace Seeding;

public static class Roles
{
    private const string AdminRoleName = "Admin";

    public static readonly SampleIdentityRole AdminRole = new()
    {
        Id = Guid.Parse("BF5B5CB3-CF24-4A81-AC50-2B61CD3FB71B"),
        Name = AdminRoleName,
        NormalizedName = AdminRoleName.Normalize().ToUpper()
    };
}
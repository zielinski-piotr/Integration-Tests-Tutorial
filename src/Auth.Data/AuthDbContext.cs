using Auth.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Data;

public class AuthDbContext : IdentityDbContext<SampleIdentityUser, SampleIdentityRole, Guid>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<SampleIdentityRole>().HasData(Seeding.Roles.AdminRole);

        builder.Entity<SampleIdentityUser>().HasData(Seeding.Users.AdminUser);

        builder.Entity<IdentityUserRole<Guid>>().HasData(Seeding.UserRoles.AdminUserRole);

        builder.Entity<IdentityRoleClaim<Guid>>().HasData(Seeding.RoleClaims.AdminRoleClaims);
    }
}
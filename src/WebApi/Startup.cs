using System;
using System.Linq;
using Auth.Common;
using Auth.Data;
using Auth.Domain;
using Auth.Service;
using AutoMapper;
using Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Service;

namespace Api;

public class Startup
{
    private readonly string _connectionString;
    private readonly string _authConnectionString;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly SqliteConnection _keepAliveConnection;
    private readonly SqliteConnection _keepAliveAuthConnection;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
        _connectionString = $"DataSource={GenerateDbName()};mode=memory;cache=shared";
        _authConnectionString = $"DataSource={GenerateDbName()};mode=memory;cache=shared";
        _keepAliveConnection = new SqliteConnection(_connectionString);
        _keepAliveAuthConnection = new SqliteConnection(_authConnectionString);
        _keepAliveConnection.Open();
        _keepAliveAuthConnection.Open();
    }

    private IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers().AddNewtonsoftJson();

        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo {Title = "Api", Version = "v1"});
            c.CustomSchemaIds(x => x.FullName!.Replace("+", "_"));
        });

        services.AddDbContext<SampleDbContext>(
            options =>
                options.UseSqlite(_connectionString));

        services.AddDbContext<AuthDbContext>(
            options =>
                options.UseSqlite(_authConnectionString));

        var jwtTokenConfig = Configuration.GetSection("Jwt").Get<JwtTokenConfig>();

        services.AddSingleton(jwtTokenConfig);

        services.AddIdentity<SampleIdentityUser, SampleIdentityRole>()
            .AddEntityFrameworkStores<AuthDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<IdentityOptions>(options =>
        {
            // Password settings.
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequireUppercase = true;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings.
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings.
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
            options.User.RequireUniqueEmail = false;
        });

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtTokenConfig.Issuer,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Convert.FromBase64String(jwtTokenConfig.Secret)),

                    RequireAudience = true,
                    ValidateAudience = true,
                    ValidAudience = jwtTokenConfig.Audience,

                    RequireExpirationTime = true,
                    ValidateLifetime = true,

                    ValidTypes = new[] {"at+jwt", "JWT"},

                    ClockSkew = TimeSpan.FromMinutes(1)
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(AuthorizationPolicies.CanRemoveHouse,
                policy => policy.RequireClaim(PermissionClaim.PermissionClaimType,
                    UserPermission.CanRemoveHousePermission.ToString()));
            
            options.AddPolicy(AuthorizationPolicies.CanGetHouse,
                policy => policy.RequireClaim(PermissionClaim.PermissionClaimType,
                    UserPermission.CanGetHousePermission.ToString()));
            
            options.AddPolicy(AuthorizationPolicies.CanGetHouses,
                policy => policy.RequireClaim(PermissionClaim.PermissionClaimType,
                    UserPermission.CanGetHousesPermission.ToString()));
            
            options.AddPolicy(AuthorizationPolicies.CanCreateHouse,
                policy => policy.RequireClaim(PermissionClaim.PermissionClaimType,
                    UserPermission.CanCreateHousePermission.ToString()));
            
            options.AddPolicy(AuthorizationPolicies.CanUpdateHouse,
                policy => policy.RequireClaim(PermissionClaim.PermissionClaimType,
                    UserPermission.CanUpdateHousePermission.ToString()));
        });

        ConfigureAutomapper(services);

        services.AddTransient<IHouseService, HouseService>();
        services.AddTransient<IAuthService, AuthService>();
        services.AddTransient<IRepository, Repository>();
    }

    public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        app.UseHttpsRedirection();

        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
    }

    private static void ConfigureAutomapper(IServiceCollection services)
    {
        services.AddSingleton(_ => new MapperConfiguration(c => { c.AddProfile<MapperProfile>(); }).CreateMapper());
    }

    private static string GenerateDbName()
    {
        var random = new Random();

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, 10)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}
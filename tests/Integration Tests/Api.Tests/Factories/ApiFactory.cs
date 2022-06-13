using Auth.Data;
using Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Api.Tests.Factories
{
    public class ApiFactory : WebApplicationFactory<Startup>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var serviceProvider = services.BuildServiceProvider();

                using var scope = serviceProvider.CreateScope();
                var scopedServices = scope.ServiceProvider;
                var context = scopedServices
                    .GetRequiredService<SampleDbContext>();
                
                var authContext = scopedServices
                    .GetRequiredService<AuthDbContext>();

                try
                {
                    context.Database.EnsureCreated();
                    authContext.Database.EnsureCreated();
                }
                catch (Exception ex)
                {
                    var logger = scopedServices.GetRequiredService<ILogger<HouseControllerTests>>();
                    logger.LogError(ex, "An error occurred seeding " +
                                        "the database with test messages. Error: {Message}",
                        ex.Message);
                }
            });
        }
    }
}
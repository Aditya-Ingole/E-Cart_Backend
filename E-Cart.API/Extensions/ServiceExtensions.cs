using ECart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ECart.API.Extensions
{
    public static class ServiceExtensions
    {
        public static void AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sqlOptions =>
                    {
                        sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                        // This tells EF Core that migrations live in the Infrastructure project,
                        // not in the API project.
                    }
                );
            });
        }
    }
}
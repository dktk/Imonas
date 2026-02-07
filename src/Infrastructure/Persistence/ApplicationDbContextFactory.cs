using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Persistence
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            // Navigate to the WebUI project directory where appsettings.json is located
            var basePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "SmartAdmin.WebUI");

            // If running from Infrastructure project, use current directory
            if (!Directory.Exists(basePath))
            {
                basePath = Path.Combine(Directory.GetCurrentDirectory(), "../../SmartAdmin.WebUI");
            }

            // If still not found, try absolute path
            if (!Directory.Exists(basePath))
            {
                basePath = Path.GetFullPath(Path.Combine(
                    Path.GetDirectoryName(typeof(ApplicationDbContext).Assembly.Location) ?? "",
                    "../../../SmartAdmin.WebUI"));
            }

            var configuration = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile("appsettings.Development.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Try to get connection string, use a default if not found
            var connectionString = configuration.GetConnectionString("DefaultConnection");

            optionsBuilder.UseNpgsql(connectionString,
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));

            return new ApplicationDbContext(configuration, optionsBuilder.Options, null, null, null);
        }
    }
}

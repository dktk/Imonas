//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Design;
//using Microsoft.Extensions.Configuration;

//namespace PspConnectors.Data
//{
//    public class DatabaseDesignTimeDbContextFactory
//    : IDesignTimeDbContextFactory<AppDbContext>
//    {
//        private readonly IConfiguration _configuration;

//        public DatabaseDesignTimeDbContextFactory()
//        {
            
//        }

//        public DatabaseDesignTimeDbContextFactory(IConfiguration configuration)
//        {
//            _configuration = configuration;
//        }

//        public AppDbContext CreateDbContext(string[] args)
//        {
//            var builder = new DbContextOptionsBuilder<AppDbContext>();
//            builder.UseNpgsql(builder =>
//            {
                
//            });

//            return new AppDbContext(builder.Options);
//        }
//    }
//}

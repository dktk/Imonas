//using PspConnectors.Methods;

//using Microsoft.EntityFrameworkCore;
//using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
//using Microsoft.Extensions.Configuration;

//namespace PspConnectors.Data
//{
//    public class AppDbContext : DbContext
//    {
//        private readonly IConfiguration _configuration;

//        public AppDbContext() { }
//        public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
//        {
//            _configuration = configuration;
//        }

//        public DbSet<SourceData> Sources { get; set; }
//        public DbSet<TargetData> Targets { get; set; }
//        public DbSet<Tenant> Tenants { get; set; }
//        public DbSet<Run> Runs { get; set; }


//        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        {
//            optionsBuilder.UseNpgsql(s =>
//            {
//                _configuration.GetConnectionString("PostgresDb");
//            });

//            base.OnConfiguring(optionsBuilder);
//        }

//        protected override void OnModelCreating(ModelBuilder modelBuilder)
//        {
//            base.OnModelCreating(modelBuilder);

//            var dtoUtcConverter = new ValueConverter<DateTimeOffset, DateTimeOffset>(
//                                        toProvider => toProvider.ToUniversalTime(),    // write as UTC (offset 0)
//                                        fromProvider => fromProvider                   // Npgsql already returns UTC
//                                    );

//            modelBuilder.Entity<SourceData>()
//                .Property(x => x.RequestDate)
//                .HasColumnType("timestamp with time zone");

//            modelBuilder.Entity<TargetData>()
//                            .Property(x => x.Date)
//                            .HasColumnType("timestamp with time zone");

//            modelBuilder.Entity<Run>()
//                            .Property(x => x.Date)
//                            .HasColumnType("timestamp with time zone");
//            modelBuilder.Entity<Run>()
//                                        .Property(x => x.StartDate)
//                                        .HasColumnType("timestamp with time zone");
//            modelBuilder.Entity<Run>()
//                                        .Property(x => x.EndDate)
//                                        .HasColumnType("timestamp with time zone");

//            foreach (var entity in modelBuilder.Model.GetEntityTypes())
//            {
//                foreach (var prop in entity.GetProperties())
//                {
//                    if (prop.ClrType == typeof(DateTimeOffset) || prop.ClrType == typeof(DateTimeOffset?))
//                        prop.SetValueConverter(dtoUtcConverter);
//                }
//            }
//        }
//    }

//    public class Tenant
//    {
//        public Guid Id { get; set; }
//        public required string Name { get; set; }
//        public bool IsActive { get; set; } = true;
//    }

//    public class Run
//    {
//        public Guid Id { set; get; }
//        public required string Trigger { get; set; }
//        public required DateTimeOffset Date { get; set; }
//        public required string System { get; set; }
//        public required DateTimeOffset StartDate { get; set; }
//        public required DateTimeOffset EndDate { get; set; }
//        public required string Source { get; set; }
//        public required string Target { get; set; }
//    }
//}

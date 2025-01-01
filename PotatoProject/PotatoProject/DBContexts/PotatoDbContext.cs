using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.Sec;

namespace PotatoProject.DBContexts
{
    public class PotatoDbContext : DbContext
    {
        public DbSet<Models.Potato> Potatos { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            var connectionString = configuration.GetConnectionString("ApplicationDatabase");

            if (new[] { "{0}", "{1}", "{2}" }.All(c => connectionString.Contains(c)))
            {
                var db_name = Environment.GetEnvironmentVariable("APP_DB_MYSQL_DATABASE");
                var db_user = Environment.GetEnvironmentVariable("APP_DB_MYSQL_USER");
                var db_password = Environment.GetEnvironmentVariable("APP_DB_MYSQL_PASSWORD");
                connectionString = string.Format(connectionString, db_name, db_user, db_password);
            }

            optionsBuilder.UseMySQL(connectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Use Fluent API to configure
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Models.Potato>(entity =>
            {
                // Map entities to tables
                entity.ToTable("Potatos");

                // Configure Primary Keys
                entity.HasKey(e => e.Id).HasName("Id");

                // Configure columns
                entity.Property(e => e.Id).HasColumnName("Id").HasColumnType("int").ValueGeneratedOnAdd();
                entity.Property(e => e.Description).HasColumnName("Description").HasMaxLength(50).IsRequired();
            });
        }

        public async Task<Models.Potato?> queryPotatoByIdAsync(int id)
        {
            try
            {
                return await this.Potatos.FindAsync(id);
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                Serilog.Log.Error(e.ToString());
                return null;
            }
        }

        public async Task<Models.Potato?> createPotatoAsync(Models.Potato potato)
        {
            try
            {
                // Creates the database if not exists
                await this.Database.EnsureCreatedAsync();

                // Add potato to existing potatos
                await this.Potatos.AddAsync(potato);

                // Saves changes
                int result = await this.SaveChangesAsync();

                if (result > 0)
                {
                    return potato;
                }
                else
                {
                    return null;
                }
            }
            catch (MySql.Data.MySqlClient.MySqlException e)
            {
                Serilog.Log.Error(e.ToString());
                return null;
            }
        }
    }
}
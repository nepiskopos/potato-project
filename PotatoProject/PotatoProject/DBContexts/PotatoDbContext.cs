using Microsoft.EntityFrameworkCore;

namespace PotatoProject.DBContexts
{
    public class PotatoDbContext : DbContext
    {
        public DbSet<Models.Potato> Potatos { get; set; }

        public PotatoDbContext() : base()
        { }

        public PotatoDbContext(DbContextOptions<PotatoDbContext> options) : base(options)
        { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json")
                .Build();

            optionsBuilder.UseMySQL(configuration.GetConnectionString("ApplicationDatabase"));
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
            catch (MySql.Data.MySqlClient.MySqlException)
            {
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
            catch (MySql.Data.MySqlClient.MySqlException)
            {
                return null;
            }
        }
    }
}
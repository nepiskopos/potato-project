using Serilog;
using FastEndpoints;
using FastEndpoints.Swagger;
using PotatoProject.Mapper;
using Microsoft.EntityFrameworkCore;

namespace PotatoProject
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddSwaggerGen();
            builder.Services.SwaggerDocument();
            // Add FastEndpoints service
            builder.Services.AddFastEndpoints();
            // Add AutoMapper
            builder.Services.AddAutoMapper(typeof(PotatoAutoMapper));
            // Add service database support
            builder.Services.AddDbContext<DBContexts.PotatoDbContext>(
                options =>
                {
                    if (!builder.Environment.IsDevelopment())
                    {
                        var db_name = Environment.GetEnvironmentVariable("APP_DB_MYSQL_DATABASE");
                        var db_user = Environment.GetEnvironmentVariable("APP_DB_MYSQL_USER");
                        var db_password = Environment.GetEnvironmentVariable("APP_DB_MYSQL_PASSWORD");
                        var connectionString = string.Format(builder.Configuration.GetConnectionString("ApplicationDatabase"), db_name, db_user, db_password);
                        options.UseMySQL(connectionString);
                    }
            });
            // Add logging support with Serilog
            builder.Services.AddLogging();
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration);
                if (!builder.Environment.IsDevelopment())
                {
                    var db_name = Environment.GetEnvironmentVariable("LOG_DB_MYSQL_DATABASE");
                    var db_user = Environment.GetEnvironmentVariable("LOG_DB_MYSQL_USER");
                    var db_password = Environment.GetEnvironmentVariable("LOG_DB_MYSQL_PASSWORD");
                    var connectionString = string.Format(builder.Configuration.GetConnectionString("ApplicationDatabase"), db_name, db_user, db_password);
                    configuration.WriteTo.MySQL(connectionString);
                }
                else
                {
                    configuration.WriteTo.MySQL(context.Configuration.GetConnectionString("SerilogDatabase"));
                }
            });
            builder.WebHost.ConfigureKestrel(o =>
            {
                o.Limits.MaxRequestBodySize = 15000000; // set max allowed file size for PDF documents to 14.3MB
            });
            builder.WebHost.UseUrls(["http://+:8080", "http://+:8081"]);


            var app = builder.Build();

            app.UseFastEndpoints();

            app.UseSwaggerGen();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
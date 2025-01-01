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
            builder.Services.AddDbContext<DBContexts.PotatoDbContext>();
            // Add logging support with Serilog
            builder.Services.AddLogging();
            builder.Host.UseSerilog((context, services, configuration) =>
            {
                var connectionString = builder.Configuration.GetConnectionString("SerilogDatabase");

                if (new[] { "{0}", "{1}", "{2}" }.All(c => connectionString.Contains(c)))
                {
                    var db_name = Environment.GetEnvironmentVariable("LOG_DB_MYSQL_DATABASE");
                    var db_user = Environment.GetEnvironmentVariable("LOG_DB_MYSQL_USER");
                    var db_password = Environment.GetEnvironmentVariable("LOG_DB_MYSQL_PASSWORD");
                    connectionString = string.Format(connectionString, db_name, db_user, db_password);
                }
                System.Diagnostics.Debug.WriteLine(connectionString);
                configuration.ReadFrom.Configuration(context.Configuration);
                configuration.WriteTo.MySQL(connectionString);
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
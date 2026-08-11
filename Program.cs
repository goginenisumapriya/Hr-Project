using CricketerAPI.Data;
using CricketerAPI.Repository;
using CricketerAPI.Services;
using Microsoft.EntityFrameworkCore;

namespace CricketerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // SQL Server Database
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(
                    builder.Configuration.GetConnectionString("DefaultConnection")));

            // Repository Registration
            builder.Services.AddScoped<ICricketerRepository, CricketerRepository>();

            // Service Registration
            builder.Services.AddScoped<ICricketerService, CricketerService>();

            // Azure Blob Storage Service Registration
            builder.Services.AddScoped<IBlobStorageService, BlobStorageService>();

            // Add Controllers
            builder.Services.AddControllers();

            // Swagger
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Apply Migrations & Seed Data
            using (var scope = app.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                context.Database.Migrate();

                if (!context.Cricketers.Any())
                {
                    context.Cricketers.AddRange(SeedData.GetCricketers());
                    context.SaveChanges();
                }
            }

            // Configure HTTP Request Pipeline

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cricketer API V1");
                c.RoutePrefix = "swagger";
            });

            app.MapGet("/", () => Results.Redirect("/swagger"));

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
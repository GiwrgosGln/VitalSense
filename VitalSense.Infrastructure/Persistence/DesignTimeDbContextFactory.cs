using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace VitalSense.Infrastructure.Data;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetParent(Directory.GetCurrentDirectory())!.FullName;
        var configPath = Path.Combine(basePath, "VitalSense.Api", "appsettings.json");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.GetDirectoryName(configPath)!)
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

        return new AppDbContext(optionsBuilder.Options);
    }
}
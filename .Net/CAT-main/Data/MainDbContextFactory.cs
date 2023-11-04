using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;  // Required for Path.Combine
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CAT.Data
{
    /// <summary>
    /// TranslationsDbContextFactory
    /// </summary>
    public class MainDbContextFactory__ : IDbContextFactory<MainDbContext>
    {
        public MainDbContext CreateDbContext()
        {
            // Configuration setup
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())  // Set the path to the current directory
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var configuration = configurationBuilder.Build();

            // Getting connection string
            var mainConnectionString = configuration.GetConnectionString("MainDbConnection")
                ?? throw new InvalidOperationException("Connection string 'MainDbConnection' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<MainDbContext>();
            optionsBuilder.UseSqlServer(mainConnectionString); // For SQL Server. Replace with appropriate DB provider if different.

            return new MainDbContext(optionsBuilder.Options);
        }
    }
}

using CAT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace CAT.Configuration
{
    public class DatabaseConfigurationProvider : ConfigurationProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseConfigurationProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public override void Load()
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            // Retrieve settings from the database
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
            var settings = dbContext.AppSettings.AsNoTracking().ToList();

            foreach (var setting in settings)
            {
                Data[setting.Key] = setting.Value;
            }
        }

        public void Reload()
        {
            Data.Clear(); // Clear the existing data

            // Retrieve settings from the database
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MainDbContext>();
            var settings = dbContext.AppSettings.AsNoTracking().ToList();

            foreach (var setting in settings)
            {
                Data[setting.Key] = setting.Value;
            }

            // Trigger a change notification
            OnReload();
        }
    }


    public class DatabaseConfigurationSource : IConfigurationSource
    {
        private IConfigurationProvider _databaseConfigurationProvider = default!;

        private readonly IServiceProvider _serviceProvider;

        public DatabaseConfigurationSource(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
            _databaseConfigurationProvider = new DatabaseConfigurationProvider(_serviceProvider);
        }

        public IConfigurationProvider GetConfigurationProvider()
        {
            return _databaseConfigurationProvider;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return _databaseConfigurationProvider;
        }
    }
}

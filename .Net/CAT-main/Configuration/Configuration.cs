using CAT.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;

namespace CAT.Configuration
{
    public class DatabaseConfigurationProvider : ConfigurationProvider
    {
        private readonly MainDbContext _dbContext;

        public DatabaseConfigurationProvider(MainDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public override void Load()
        {
            Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

            // Retrieve settings from the database
            var settings = _dbContext.AppSettings.ToList();

            foreach (var setting in settings)
            {
                Data[setting.Key] = setting.Value;
            }

            // You can also implement a change notification mechanism if needed
            // For example, use a timer to periodically check for updates in the database
        }

        public void Reload()
        {
            Data.Clear(); // Clear the existing data

            // Retrieve settings from the database
            var settings = _dbContext.AppSettings.ToList();

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
        private readonly MainDbContext _dbContext;
        private IConfigurationProvider _databaseConfigurationProvider = default!;

        public DatabaseConfigurationSource(MainDbContext dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public IConfigurationProvider GetConfigurationProvider()
        {
            return _databaseConfigurationProvider;
        }

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            _databaseConfigurationProvider = new DatabaseConfigurationProvider(_dbContext);
            return _databaseConfigurationProvider;
        }
    }
}

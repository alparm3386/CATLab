namespace CAT.Services.Common
{
    using CAT.Data;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Caching.Memory;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

    public class ConstantRepository
    {
        private readonly DbContextContainer _dbContextContainer;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromHours(1); // Adjust as needed

        public ConstantRepository(DbContextContainer dbContextContainer, IMemoryCache memoryCache)
        {
            _dbContextContainer = dbContextContainer;
            _cache = memoryCache;
        }

        public async Task<string> GetConstantAsync(string key)
        {
            // Try to get the value from cache
            if (_cache.TryGetValue(key, out string value))
            {
                return value;
            }

            // If not in cache, fetch from the database
            value = await _dbContextContainer!.MainContext!.ConfigConstants!
                                    .Where(c => c.Key == key)
                                    .Select(c => c.Value)
                                    .FirstOrDefaultAsync();

            // Store the value in the cache
            if (value != null)
            {
                _cache.Set(key, value, _cacheDuration);
            }

            return value;
        }

        public async Task RefreshConstantAsync(string key)
        {
            var value = await _dbContextContainer.MainContext.ConfigConstants
                                        .Where(c => c.Key == key)
                                        .Select(c => c.Value)
                                        .FirstOrDefaultAsync();

            if (value != null)
            {
                _cache.Set(key, value, _cacheDuration);
            }
            else
            {
                _cache.Remove(key);
            }
        }
    }
}

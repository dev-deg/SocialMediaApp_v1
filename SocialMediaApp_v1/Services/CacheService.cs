// File: Services/CacheService.cs
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SocialMediaApp_v1.Interfaces;
using StackExchange.Redis;

namespace SocialMediaApp_v1.Services
{
    public class CacheService : ICacheService, IDisposable
    {
        private readonly ILogger<CacheService> _logger;
        private readonly ConnectionMultiplexer _redis;
        private readonly IDatabase _database;

        public CacheService(IConfiguration configuration, ILogger<CacheService> logger)
        {
            _logger = logger;
            var connectionString = configuration["Authentication:Redis:ConnectionString"];
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _database = _redis.GetDatabase();
        }

        public async Task<string> GetAsync(string key)
        {
            var value = await _database.StringGetAsync(key);
            _logger.LogInformation($"Retrieved value for key: {key}");
            return value;
        }

        public async Task SetAsync(string key, string value, TimeSpan? expiry = null)
        {
            await _database.StringSetAsync(key, value, expiry);
            _logger.LogInformation($"Set value for key: {key}");
        }

        public async Task DeleteAsync(string key)
        {
            await _database.KeyDeleteAsync(key);
            _logger.LogInformation($"Deleted key: {key}");
        }

        public void Dispose()
        {
            _redis?.Dispose();
        }
    }
}
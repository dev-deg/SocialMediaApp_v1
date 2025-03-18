using Google.Cloud.SecretManager.V1;
using SocialMediaApp_v1.Interfaces;

namespace SocialMediaApp_v1.Services
{
    public class SecretManagerService: ISecretManagerService
    {

        private readonly ILogger<SecretManagerService> _logger;
        private readonly SecretManagerServiceClient _client;
        public SecretManagerService(ILogger<SecretManagerService> logger) { 
            _logger = logger;
            _client = SecretManagerServiceClient.Create();
        }

        public async Task LoadSecretsAsync(IConfiguration configuration)
        {
            string projectId = "452965498820";//configuration["Authentication:Google:ProjectId"];

            if (string.IsNullOrEmpty(projectId))
            {
                _logger.LogWarning("Google Cloud Project Id is missing.. Skipping secrets retrieval.");
                return;
            }
            _logger.LogInformation("Fetching secrets from the secret manager..");
            var secretNames = new[]
            {
                "Authentication-Google-ClientId",
                "Authentication-Google-ClientSecret",
                "Authentication-Google-StorageBucketName",
                "Redis-ConnectionString"
            };
            
            foreach (var secretName in secretNames)
            {
                var secretValue = await GetSecretAsync(projectId, secretName);
                if (!string.IsNullOrEmpty(secretValue))
                {
                    var formattedKey = secretName.Replace("-", ":");
                    configuration[formattedKey] = secretValue;
                    _logger.LogInformation($"Secret {formattedKey} loaded successfully!");
                }
                else
                {
                    _logger.LogWarning($"Secret {secretName} is missing or empty");
                }
            }
        }

        public async Task<string> GetSecretAsync(string projectId, string secretName, string? version = null)
        {
            if (version == null) version = "1";
            
            var secretVersionName = new SecretVersionName(projectId, secretName, version);

            try
            {
                var response = await _client.AccessSecretVersionAsync(secretVersionName);
                _logger.LogInformation($"Secret version {secretVersionName} retrieved successfully!");
                return response.Payload.Data.ToStringUtf8();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving secret {secretName}.");
                return null;
            } 
        }


    }
}

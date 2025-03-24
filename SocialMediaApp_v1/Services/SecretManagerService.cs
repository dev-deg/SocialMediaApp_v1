﻿using Google.Cloud.SecretManager.V1;
using SocialMediaApp_v1.Interfaces;

namespace SocialMediaApp_v1.Services
{
    public class SecretManagerService: ISecretManagerService
    {
        private readonly ICloudLoggingService _logger;
        private readonly SecretManagerServiceClient? _client;
        
        public SecretManagerService(ICloudLoggingService logger) { 
            _logger = logger;
            _logger.LogInformationAsync("Initializing SecretManagerService").Wait();
            Console.WriteLine("Initializing SecretManagerService");
            
            try
            {
                _client = SecretManagerServiceClient.Create();
                _logger.LogInformationAsync("SecretManagerServiceClient created successfully").Wait();
                Console.WriteLine("SecretManagerServiceClient created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogCriticalAsync("Failed to create SecretManagerServiceClient", ex).Wait();
                Console.WriteLine($"ERROR creating SecretManagerServiceClient: {ex.Message}");
                // Don't throw the exception, so the application can still start with default values
                _client = null;
            }
        }

        public async Task LoadSecretsAsync(IConfiguration configuration)
        {
            if (_client == null)
            {
                await _logger.LogWarningAsync("SecretManagerServiceClient is not initialized. Skipping secrets retrieval.");
                Console.WriteLine("WARNING: SecretManagerServiceClient is not initialized. Skipping secrets retrieval.");
                return;
            }

            string projectId = configuration["Authentication:Google:ProjectId"] ?? "452965498820";

            if (string.IsNullOrEmpty(projectId))
            {
                await _logger.LogWarningAsync("Google Cloud Project Id is missing. Skipping secrets retrieval.");
                Console.WriteLine("WARNING: Google Cloud Project Id is missing. Skipping secrets retrieval.");
                return;
            }
            
            await _logger.LogInformationAsync($"Fetching secrets from the secret manager for project {projectId}");
            Console.WriteLine($"Fetching secrets from the secret manager for project {projectId}");
            
            var secretNames = new[]
            {
                "Authentication-Google-ClientId",
                "Authentication-Google-ClientSecret",
                "Authentication-Google-StorageBucketName",
                "Redis-ConnectionString"
            };
            
            foreach (var secretName in secretNames)
            {
                try
                {
                    Console.WriteLine($"Retrieving secret: {secretName}");
                    var secretValue = await GetSecretAsync(projectId, secretName, "1");
                    if (!string.IsNullOrEmpty(secretValue))
                    {
                        var formattedKey = secretName.Replace("-", ":");
                        configuration[formattedKey] = secretValue;
                        await _logger.LogInformationAsync($"Secret {formattedKey} loaded successfully!");
                        Console.WriteLine($"Secret {formattedKey} loaded successfully!");
                    }
                    else
                    {
                        await _logger.LogWarningAsync($"Secret {secretName} is missing or empty");
                        Console.WriteLine($"WARNING: Secret {secretName} is missing or empty");
                    }
                }
                catch (Exception ex)
                {
                    await _logger.LogErrorAsync($"Error loading secret {secretName}", ex);
                    Console.WriteLine($"ERROR loading secret {secretName}: {ex.Message}");
                }
            }
        }

        public async Task<string> GetSecretAsync(string projectId, string secretName, string? version = null)
        {
            if (_client == null)
            {
                Console.WriteLine("Cannot access secrets - SecretManagerServiceClient is null");
                return null;
            }
            
            if (version == null) version = "1";
            
            var secretVersionName = new SecretVersionName(projectId, secretName, version);

            try
            {
                await _logger.LogDebugAsync($"Accessing secret {secretName}, version {version}");
                var response = await _client.AccessSecretVersionAsync(secretVersionName);
                await _logger.LogInformationAsync($"Secret version {secretVersionName} retrieved successfully!");
                return response.Payload.Data.ToStringUtf8();
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync($"Error retrieving secret {secretName}.", ex);
                Console.WriteLine($"ERROR retrieving secret {secretName}: {ex.Message}");
                return null;
            } 
        }
    }
}

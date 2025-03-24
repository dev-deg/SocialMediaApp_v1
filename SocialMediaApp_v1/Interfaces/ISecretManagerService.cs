﻿namespace SocialMediaApp_v1.Interfaces
{
    public interface ISecretManagerService
    {
        Task LoadSecretsAsync(IConfiguration configuration);

        Task<string> GetSecretAsync(string projectId, string secretName, string? version = null);
    }
}

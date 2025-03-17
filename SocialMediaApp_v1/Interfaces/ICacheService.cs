namespace SocialMediaApp_v1.Interfaces
{
    public interface ICacheService
    {
        Task<string> GetAsync(string key);
        Task SetAsync(string key, string value, TimeSpan? expiry);
        Task DeleteAsync(string key);
    }
}
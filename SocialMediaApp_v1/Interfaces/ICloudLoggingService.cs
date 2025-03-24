namespace SocialMediaApp_v1.Interfaces
{
    public interface ICloudLoggingService
    {
        Task LogDebugAsync(string message, Dictionary<string, string>? metadata = null);
        Task LogInformationAsync(string message, Dictionary<string, string>? metadata = null);
        Task LogWarningAsync(string message, Dictionary<string, string>? metadata = null);
        Task LogErrorAsync(string message, Exception? exception = null, Dictionary<string, string>? metadata = null);
        Task LogCriticalAsync(string message, Exception? exception = null, Dictionary<string, string>? metadata = null);
        void SetLogLevel(LogLevel minimumLogLevel);
        LogLevel GetCurrentLogLevel();
    }
}

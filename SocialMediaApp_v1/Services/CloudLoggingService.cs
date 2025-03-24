using Google.Api;
using Google.Cloud.Logging.V2;
using Google.Cloud.Logging.Type;
using SocialMediaApp_v1.Interfaces;
using System.Text;

namespace SocialMediaApp_v1.Services
{
    public class CloudLoggingService : ICloudLoggingService
    {
        private readonly string _projectId;
        private readonly LoggingServiceV2Client? _loggingClient;
        private readonly ILogger<CloudLoggingService> _localLogger;
        private readonly string _logName;
        private readonly IHostEnvironment _environment;
        private LogLevel _currentLogLevel = LogLevel.Information;
        private bool _isInitialized = false;

        public CloudLoggingService(IConfiguration configuration, ILogger<CloudLoggingService> logger, IHostEnvironment environment)
        {
            _localLogger = logger;
            _environment = environment;
            
            try
            {
                _projectId = configuration["Authentication:Google:ProjectId"];
                
                if (string.IsNullOrEmpty(_projectId))
                {
                    _localLogger.LogWarning("Google ProjectId not configured. Using local console logging only.");
                    Console.WriteLine("WARNING: Google ProjectId not configured. Using local console logging only.");
                    return;
                }
                
                _logName = configuration["Logging:Google:LogName"] ?? "social-media-app-logs";
                _loggingClient = LoggingServiceV2Client.Create();
                _isInitialized = true;
                
                _localLogger.LogInformation("CloudLoggingService initialized successfully");
                Console.WriteLine("CloudLoggingService initialized successfully");
            }
            catch (Exception ex)
            {
                _localLogger.LogError(ex, "Failed to initialize CloudLoggingService. Using local logging only.");
                Console.WriteLine($"ERROR initializing CloudLoggingService: {ex.Message}");
                // Don't throw - allow the application to continue with local logging
            }
        }

        public LogLevel GetCurrentLogLevel() => _currentLogLevel;

        public void SetLogLevel(LogLevel minimumLogLevel)
        {
            _currentLogLevel = minimumLogLevel;
            _localLogger.LogInformation($"Cloud logging level set to {minimumLogLevel}");
        }

        public async Task LogDebugAsync(string message, Dictionary<string, string>? metadata = null)
        {
            if (_currentLogLevel > LogLevel.Debug) return;
            await WriteLogEntryAsync(LogSeverity.Debug, message, null, metadata);
        }

        public async Task LogInformationAsync(string message, Dictionary<string, string>? metadata = null)
        {
            if (_currentLogLevel > LogLevel.Information) return;
            await WriteLogEntryAsync(LogSeverity.Info, message, null, metadata);
        }

        public async Task LogWarningAsync(string message, Dictionary<string, string>? metadata = null)
        {
            if (_currentLogLevel > LogLevel.Warning) return;
            await WriteLogEntryAsync(LogSeverity.Warning, message, null, metadata);
        }

        public async Task LogErrorAsync(string message, Exception? exception = null, Dictionary<string, string>? metadata = null)
        {
            if (_currentLogLevel > LogLevel.Error) return;
            await WriteLogEntryAsync(LogSeverity.Error, message, exception, metadata);
        }

        public async Task LogCriticalAsync(string message, Exception? exception = null, Dictionary<string, string>? metadata = null)
        {
            if (_currentLogLevel > LogLevel.Critical) return;
            await WriteLogEntryAsync(LogSeverity.Critical, message, exception, metadata);
        }

        private async Task WriteLogEntryAsync(LogSeverity severity, string message, Exception? exception = null, Dictionary<string, string>? metadata = null)
        {
            // Always write to console for immediate feedback
            LogToConsole(severity, message, exception);
            
            // If not properly initialized, fall back to local logging
            if (!_isInitialized || _loggingClient == null)
            {
                LogToLocalLogger(severity, message, exception);
                return;
            }
            
            try
            {
                var logName = new LogName(_projectId, _logName);
                var resource = new MonitoredResource { Type = "global" };

                var logEntry = new LogEntry
                {
                    LogName = logName.ToString(),
                    Severity = severity,
                    TextPayload = BuildLogMessage(message, exception)
                };

                if (metadata != null)
                {
                    foreach (var item in metadata)
                    {
                        logEntry.Labels.Add(item.Key, item.Value);
                    }
                }

                // Add basic execution context information
                logEntry.Labels.Add("environment", _environment.EnvironmentName);
                logEntry.Labels.Add("machine", Environment.MachineName);
                logEntry.Labels.Add("timestamp", DateTime.UtcNow.ToString("o"));

                var request = new WriteLogEntriesRequest
                {
                    LogName = logName.ToString(),
                    Resource = resource,
                    Entries = { logEntry },
                };

                await _loggingClient.WriteLogEntriesAsync(request);
            }
            catch (Exception ex)
            {
                // Fallback to local logging if cloud logging fails
                LogToLocalLogger(severity, message, exception);
                _localLogger.LogError(ex, $"Failed to write to Cloud Logging");
            }
        }
        
        private void LogToConsole(LogSeverity severity, string message, Exception? exception)
        {
            // Always log to console for immediate visibility
            var prefix = severity switch
            {
                LogSeverity.Debug => "[DEBUG] ",
                LogSeverity.Info => "[INFO] ",
                LogSeverity.Warning => "[WARNING] ",
                LogSeverity.Error => "[ERROR] ",
                LogSeverity.Critical => "[CRITICAL] ",
                _ => "[INFO] "
            };
            
            Console.WriteLine($"{prefix}{message}");
            
            if (exception != null)
            {
                Console.WriteLine($"Exception: {exception.Message}");
                Console.WriteLine($"StackTrace: {exception.StackTrace}");
            }
        }
        
        private void LogToLocalLogger(LogSeverity severity, string message, Exception? exception)
        {
            switch (severity)
            {
                case LogSeverity.Debug:
                    _localLogger.LogDebug(exception, message);
                    break;
                case LogSeverity.Info:
                    _localLogger.LogInformation(exception, message);
                    break;
                case LogSeverity.Warning:
                    _localLogger.LogWarning(exception, message);
                    break;
                case LogSeverity.Error:
                    _localLogger.LogError(exception, message);
                    break;
                case LogSeverity.Critical:
                    _localLogger.LogCritical(exception, message);
                    break;
                default:
                    _localLogger.LogInformation(exception, message);
                    break;
            }
        }

        private string BuildLogMessage(string message, Exception? exception)
        {
            var builder = new StringBuilder();
            builder.AppendLine(message);

            if (exception != null)
            {
                builder.AppendLine($"Exception: {exception.GetType().Name}");
                builder.AppendLine($"Message: {exception.Message}");
                builder.AppendLine($"StackTrace: {exception.StackTrace}");

                var innerException = exception.InnerException;
                while (innerException != null)
                {
                    builder.AppendLine($"Inner Exception: {innerException.GetType().Name}");
                    builder.AppendLine($"Inner Message: {innerException.Message}");
                    builder.AppendLine($"Inner StackTrace: {innerException.StackTrace}");
                    innerException = innerException.InnerException;
                }
            }

            return builder.ToString();
        }
    }
}

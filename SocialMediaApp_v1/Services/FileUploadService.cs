using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using SocialMediaApp_v1.Interfaces;

namespace SocialMediaApp_v1.Services;

public class FileUploadService: IFileUploadService
{
    private readonly ILogger<FileUploadService> _logger;
    private readonly StorageClient _storageClient;
    private readonly string _bucketName;

    public FileUploadService(ILogger<FileUploadService> logger, IConfiguration configuration)
    {
        _logger = logger;
        
        GoogleCredential googleCredential =
            GoogleCredential.FromFile(configuration.GetValue<string>("Authentication:Google:ServiceAccountCredentials"));
        _bucketName = configuration.GetValue<string>("Authentication:Google:StorageBucketName");
        _storageClient = StorageClient.Create(googleCredential);
    }

    public Task<string> UploadFileAsync(IFormFile file, string fileNameForStorage)
    {
        throw new NotImplementedException();
    }
}
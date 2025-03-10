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

    public async Task<string> UploadFileAsync(IFormFile file, string fileNameForStorage)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentException("File is empty or null", nameof(file));
        }

        try
        {
            // Generate a unique file name if none is provided
            if (string.IsNullOrEmpty(fileNameForStorage))
            {
                fileNameForStorage = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            }

            // Set content type based on file extension
            string contentType = file.ContentType;

            using (var memoryStream = new MemoryStream())
            {
                await file.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                // Upload to Google Cloud Storage
                var options = new UploadObjectOptions();
                var storageObject = await _storageClient.UploadObjectAsync(
                    _bucketName,
                    fileNameForStorage,
                    contentType,
                    memoryStream,
                    options);

                _logger.LogInformation($"File {fileNameForStorage} uploaded to bucket {_bucketName}");
            
                // Return the public URL for the uploaded file
                return $"https://storage.googleapis.com/{_bucketName}/{fileNameForStorage}";
            }
        }
        catch (Google.GoogleApiException googleEx)
        {
            _logger.LogError(googleEx, $"Google API error during file upload: {googleEx.Message}");
            throw new ApplicationException("Error uploading file to cloud storage", googleEx);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unexpected error during file upload: {ex.Message}");
            throw new ApplicationException("Unexpected error during file upload", ex);
        }
    }
}
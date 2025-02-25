namespace SocialMediaApp_v1.Interfaces;

public interface IFileUploadService
{
    Task<string> UploadFileAsync(IFormFile file, string fileNameForStorage);
}
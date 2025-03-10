using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp_v1.DataAccess;
using SocialMediaApp_v1.Models;
using SocialMediaApp_v1.Interfaces;
using System.Threading.Tasks;

namespace SocialMediaApp_v1.Controllers;

public class SocialController : Controller
{
    private readonly ILogger<SocialController> _logger;
    private FirestoreRepository _repo;
    private readonly IFileUploadService _fileUploadService;

    public SocialController(
        ILogger<SocialController> logger, 
        FirestoreRepository repo,
        IFileUploadService fileUploadService)
    {
        _logger = logger;
        _repo = repo;
        _fileUploadService = fileUploadService;
    }

    [Authorize]
    public IActionResult Index()
    {
        return View(_repo.GetPosts().Result);
    }

    [Route("CreatePost")]
    [HttpPost]
    public async Task<IActionResult> CreatePost(SocialMediaPost post)
    {
        post.PostId = Guid.NewGuid().ToString();
        post.PostAuthor = User.Identity.Name;
        post.PostDate = DateTimeOffset.UtcNow;
        await _repo.AddPost(post);
        return RedirectToAction("Index", "Social");
    }
    
    [Authorize]
    [HttpPost]
    [Route("UploadImage")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { success = false, message = "No file was uploaded" });
            }

            // Validate file type
            string[] permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
            string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            
            if (!permittedExtensions.Contains(fileExtension))
            {
                return BadRequest(new { success = false, message = "Invalid file type" });
            }

            // Generate a unique filename
            string fileName = $"{Guid.NewGuid()}{fileExtension}";
            
            // Upload file to Google Cloud Storage
            string imageUrl = await _fileUploadService.UploadFileAsync(file, fileName);
            
            // Return the URL of the uploaded image
            return Ok(new { success = true, imageUrl });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
    [Authorize]
    [HttpPost]
    [Route("DeletePost")]
    public async Task<IActionResult> DeletePost(string postId)
    {
        try
        {
            // Get the specific post by ID instead of loading all posts
            var post = await _repo.GetPostById(postId);

            if (post == null)
            {
                return NotFound(new { success = false, message = "Post not found" });
            }

            // Check if the current user is the author of the post
            if (post.PostAuthor != User.Identity.Name)
            {
                return Forbid();
            }

            // Delete the image from cloud storage if it exists
            if (!string.IsNullOrEmpty(post.ImageUrl))
            {
                await _fileUploadService.DeleteFileAsync(post.ImageUrl);
            }

            // Delete the post from Firestore
            await _repo.DeletePost(postId);

            return Ok(new { success = true });
        }
        catch (ApplicationException ex)
        {
            _logger.LogError(ex, $"Error deleting post {postId}");
            return StatusCode(500, new { success = false, message = ex.Message });
        }
    }
}
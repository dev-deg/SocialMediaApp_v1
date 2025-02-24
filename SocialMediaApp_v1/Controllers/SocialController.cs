using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp_v1.DataAccess;
using SocialMediaApp_v1.Models;

namespace SocialMediaApp_v1.Controllers;

public class SocialController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private FirestoreRepository _repo;
    public SocialController(ILogger<HomeController> logger, FirestoreRepository repo)
    {
        _logger = logger;
        _repo = repo;
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
}
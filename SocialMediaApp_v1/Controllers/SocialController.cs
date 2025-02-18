using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SocialMediaApp_v1.Controllers;

public class SocialController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public SocialController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }
    
    [Authorize]
    public IActionResult Index()
    {
        return View();
    }

    [Authorize]
    public async Task<IActionResult> CreatePost()
    {
        return Index();
    }
}
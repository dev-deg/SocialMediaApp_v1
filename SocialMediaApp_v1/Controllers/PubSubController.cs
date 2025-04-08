// Controllers/PubSubController.cs
using Microsoft.AspNetCore.Mvc;
using SocialMediaApp_v1.Interfaces;

namespace SocialMediaApp_v1.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PubSubController : ControllerBase
{
    private readonly IPubSubService _pubSubService;

    public PubSubController(IPubSubService pubSubService)
    {
        _pubSubService = pubSubService;
    }

    [HttpPost("publish")]
    public async Task<IActionResult> PublishMessage([FromBody] DeleteFileRequest request)
    {
        try
        {
            var message = System.Text.Json.JsonSerializer.Serialize(request);
            await _pubSubService.PublishMessageAsync(message);
            return Ok();
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}

public class DeleteFileRequest
{
    public string Filename { get; set; }
}
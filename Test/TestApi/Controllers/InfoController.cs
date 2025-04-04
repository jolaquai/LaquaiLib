using Microsoft.AspNetCore.Mvc;

namespace TestApi.Controllers;
[Route("api/[controller]")]
[ApiController]
public class InfoController : ControllerBase
{
    public InfoController()
    {
    }

    [HttpGet]
    public IActionResult Respond([FromQuery] int code) => new ContentResult()
    {
        StatusCode = code,
        Content = $"Responding with requested status code result '{code}'.",
        ContentType = "text/plain"
    };
}

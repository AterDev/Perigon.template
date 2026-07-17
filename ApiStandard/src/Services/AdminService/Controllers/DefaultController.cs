namespace AdminService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DefaultController : ControllerBase
{
    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok("AdminService");
    }
}

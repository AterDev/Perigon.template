namespace AdminService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DefaultController : ControllerBase
{
    [HttpGet("info")]
    public ActionResult Info()
    {
        return Ok("AdminService");
    }
}

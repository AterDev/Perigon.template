namespace ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DefaultController(AdminServiceClient adminServiceClient) : ControllerBase
{
    [HttpGet("info")]
    public IActionResult Info()
    {
        return Ok("ApiService");
    }

    [HttpGet("inner-link")]
    public async Task<IActionResult> InnerLink(CancellationToken cancellationToken)
    {
        var content = await adminServiceClient.GetInfoAsync(cancellationToken);

        return Content(content, "application/json");
    }
}

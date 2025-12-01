using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace BusinessFinancialAccounting.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DemoController : ControllerBase
{
    private static readonly ActivitySource ActivitySource =
        new("BusinessFinancialAccounting");

    [HttpGet("long-operation")]
    public async Task<IActionResult> LongOperation()
    {
        using var activity = ActivitySource.StartActivity("LongOperation");

        activity?.SetTag("operation.type", "demo-long");
        activity?.SetTag("user.demo", "test-user");
        activity?.SetTag("simulated", true);

        await Task.Delay(5000);

        activity?.AddEvent(new ActivityEvent("finished-work"));

        return Ok(new { message = "Long operation done" });
    }
}

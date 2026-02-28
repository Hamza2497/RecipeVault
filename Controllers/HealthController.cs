// HealthController.cs
//
// What is this file?
// ------------------
// In ASP.NET Core, "Controllers" are classes that handle incoming HTTP requests.
// Each public method inside a controller is called an "action" and maps to a URL route.
//
// Why do we have a HealthController?
// -----------------------------------
// A health endpoint is a simple API route that lets anyone (or a monitoring tool)
// quickly check if the server is up and running. It's a common best practice in
// web APIs — kind of like a heartbeat signal.
//
// How does routing work here?
// ----------------------------
// The [Route("api/[controller]")] attribute above the class tells ASP.NET to prefix
// all routes in this controller with "api/health" — [controller] is automatically
// replaced with the class name minus the word "Controller".
//
// [ApiController] enables some helpful default behaviors like automatic model validation.

using Microsoft.AspNetCore.Mvc;

namespace RecipeVault.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    // GET /api/health
    //
    // [HttpGet] marks this method as the handler for GET requests to this controller's route.
    // It returns an anonymous object { status = "..." } which ASP.NET Core automatically
    // serializes to JSON — no extra work required.
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new { status = "RecipeVault is running" });
    }
}

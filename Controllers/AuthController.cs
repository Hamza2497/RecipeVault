using Microsoft.AspNetCore.Mvc;
using RecipeVault.DTOs;
using RecipeVault.Services;

namespace RecipeVault.Controllers;

/// <summary>
/// API Controller for user authentication (registration and login).
///
/// Controllers handle HTTP requests and return JSON responses.
/// This controller specifically handles user account creation and login.
///
/// Routing:
/// [ApiController] - tells ASP.NET this is a REST API controller
/// [Route("api/[controller]")] - routes all endpoints to /api/auth
/// The service is injected via dependency injection (the DI container provides it automatically)
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // The authentication service, injected via dependency injection
    // This service handles password hashing, validation, and JWT generation
    private readonly IAuthService _authService;

    /// <summary>
    /// Constructor: receives the injected IAuthService.
    /// ASP.NET automatically provides this when a request arrives.
    /// </summary>
    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// POST /api/auth/register
    /// Creates a new user account.
    ///
    /// Request body example:
    /// {
    ///   "username": "john_doe",
    ///   "email": "john@example.com",
    ///   "password": "SecurePassword123!"
    /// }
    ///
    /// Response example:
    /// {
    ///   "token": "eyJhbGc...",
    ///   "username": "john_doe",
    ///   "email": "john@example.com"
    /// }
    ///
    /// HTTP Response Codes:
    /// 200 OK - User created successfully, returns AuthResponseDto with JWT token
    /// 400 Bad Request - Invalid request (missing fields, email already registered, etc.)
    ///
    /// [FromBody] tells ASP.NET to deserialize the JSON request body into RegisterDto
    /// </summary>
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] RegisterDto registerDto)
    {
        try
        {
            // Call the service to register the user
            // The service handles password hashing, database insertion, and token generation
            var response = await _authService.RegisterAsync(registerDto);

            // Return 200 OK with the JWT token and user info
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            // If the service throws an exception (e.g., email already exists),
            // return 400 Bad Request with the error message
            return BadRequest(new { message = ex?.Message ?? "Registration failed" });
        }
    }

    /// <summary>
    /// POST /api/auth/login
    /// Authenticates a user and returns a JWT token.
    ///
    /// Request body example:
    /// {
    ///   "email": "john@example.com",
    ///   "password": "SecurePassword123!"
    /// }
    ///
    /// Response example:
    /// {
    ///   "token": "eyJhbGc...",
    ///   "username": "john_doe",
    ///   "email": "john@example.com"
    /// }
    ///
    /// HTTP Response Codes:
    /// 200 OK - Login successful, returns AuthResponseDto with JWT token
    /// 401 Unauthorized - Invalid email or password
    ///
    /// The client includes the returned token in subsequent requests:
    /// Authorization: Bearer {token}
    ///
    /// [FromBody] tells ASP.NET to deserialize the JSON request body into LoginDto
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
    {
        try
        {
            // Call the service to authenticate the user
            // The service verifies the password and generates a JWT token
            var response = await _authService.LoginAsync(loginDto);

            // Return 200 OK with the JWT token and user info
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            // If the service throws an exception (e.g., invalid email/password),
            // return 401 Unauthorized with a generic error message
            // We use a generic message for security (don't reveal whether email exists)
            return Unauthorized(new { message = "Invalid email or password" });
        }
    }
}

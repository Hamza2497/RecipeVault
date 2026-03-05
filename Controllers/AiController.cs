using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.DTOs;
using RecipeVault.Services;

namespace RecipeVault.Controllers;

/// <summary>
/// API Controller for AI-powered recipe operations.
///
/// Controllers handle HTTP requests and return responses.
/// This controller bridges the client and the GeminiService (which calls the Gemini AI API).
///
/// Routing:
/// [ApiController] - tells ASP.NET this is a REST API controller
/// [Route("api/[controller]")] - routes all endpoints to /api/ai (controller name = "Ai")
///
/// This controller uses dependency injection to receive IGeminiService.
/// The DI container automatically provides a GeminiService instance.
///
/// Security:
/// [Authorize] - all endpoints in this controller require a valid JWT token.
/// Only authenticated users (with a valid token) can access AI features.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AiController : ControllerBase
{
    /// <summary>
    /// The Gemini service, injected via dependency injection.
    /// This abstraction lets us swap implementations later (e.g., use a different AI service).
    /// </summary>
    private readonly IGeminiService _geminiService;

    /// <summary>
    /// Constructor: receives the injected Gemini service.
    /// ASP.NET automatically provides this when a request arrives.
    /// </summary>
    public AiController(IGeminiService geminiService)
    {
        _geminiService = geminiService;
    }

    /// <summary>
    /// POST /api/ai/generate
    /// Generates a new recipe based on user constraints.
    ///
    /// The request body must contain a GenerateRecipeRequestDto JSON object.
    /// Example:
    /// {
    ///   "recipeName": "Chocolate Cake",
    ///   "cuisineType": "American",
    ///   "maxPrepTimeMinutes": 20,
    ///   "maxCookTimeMinutes": 30,
    ///   "allergies": ["nuts"],
    ///   "dietaryRestrictions": ["gluten-free"]
    /// }
    ///
    /// HTTP Response Codes:
    /// 200 OK - Recipe generated successfully, returns the generated recipe as AiRecipeResponseDto
    /// 400 Bad Request - Invalid request (validation failed)
    /// 401 Unauthorized - User not authenticated (no valid JWT token)
    /// 500 Internal Server Error - Error calling Gemini API or parsing response
    ///
    /// [FromBody] tells ASP.NET to deserialize the JSON request body into the DTO.
    /// </summary>
    [HttpPost("generate")]
    public async Task<ActionResult<AiRecipeResponseDto>> GenerateRecipe([FromBody] GenerateRecipeRequestDto request)
    {
        try
        {
            // Call the Gemini service to generate the recipe
            // This sends a detailed prompt to the Gemini API and returns a structured recipe
            var recipe = await _geminiService.GenerateRecipeAsync(request);

            // Return 200 OK with the generated recipe
            return Ok(recipe);
        }
        catch (InvalidOperationException ex)
        {
            // Handle Gemini API errors or parsing errors
            return BadRequest(new { message = $"Error generating recipe: {ex.Message}" });
        }
        catch (HttpRequestException ex)
        {
            // Handle network errors when calling the Gemini API
            return StatusCode(503, new { message = $"Gemini API unavailable: {ex.Message}" });
        }
    }

    /// <summary>
    /// POST /api/ai/substitute/{ingredientName}
    /// Returns a list of substitute ingredients.
    ///
    /// Path parameter:
    /// - ingredientName: The ingredient to find substitutes for (from URL path)
    ///
    /// Optional request body: SubstituteIngredientRequestDto with optional "reason" field
    /// Example:
    /// {
    ///   "reason": "allergy"
    /// }
    ///
    /// You can also call this endpoint without a body to get general substitutes.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Returns a list of substitute ingredients (List<string>)
    /// 400 Bad Request - Invalid request
    /// 401 Unauthorized - User not authenticated (no valid JWT token)
    /// 500 Internal Server Error - Error calling Gemini API or parsing response
    ///
    /// Example URL: POST /api/ai/substitute/butter
    /// </summary>
    [HttpPost("substitute/{ingredientName}")]
    public async Task<ActionResult<List<string>>> GetSubstitutes(
        string ingredientName,
        [FromBody] SubstituteIngredientRequestDto? request = null)
    {
        try
        {
            // Extract the reason from the request body (if provided)
            var reason = request?.Reason;

            // Call the Gemini service to get substitute ingredients
            // This sends a focused prompt asking for 3 alternatives
            var substitutes = await _geminiService.GetIngredientSubstitutesAsync(ingredientName, reason);

            // Return 200 OK with the list of substitutes
            return Ok(substitutes);
        }
        catch (InvalidOperationException ex)
        {
            // Handle Gemini API errors or parsing errors
            return BadRequest(new { message = $"Error getting substitutes: {ex.Message}" });
        }
        catch (HttpRequestException ex)
        {
            // Handle network errors when calling the Gemini API
            return StatusCode(503, new { message = $"Gemini API unavailable: {ex.Message}" });
        }
    }

    /// <summary>
    /// POST /api/ai/tweak
    /// Modifies an existing recipe based on a user request.
    ///
    /// The request body must contain a TweakRecipeRequestDto with:
    /// - The current recipe (as an AiRecipeResponseDto) embedded or referenced
    /// - The tweak request (natural language instruction)
    ///
    /// Wait, looking at the requirements, the endpoint accepts the current recipe + tweak request.
    /// The TweakRecipeRequestDto only has TweakRequest. We need to extend the request DTO
    /// to include the current recipe.
    ///
    /// Actually, looking back at the spec, it says "accepts current recipe + tweak request".
    /// Let me create a proper DTO that combines both.
    ///
    /// For now, I'll create a local class in this endpoint or extend the logic.
    /// Actually, let me create a TweakWithRecipeRequestDto.
    ///
    /// Wait, the user spec says TweakRecipeRequestDto only has TweakRequest. Let me re-read...
    /// The spec says: "POST /api/ai/tweak — accepts current recipe + tweak request"
    ///
    /// I think the best approach is to create a combined DTO for this endpoint.
    /// Let me update the approach.
    /// </summary>
    [HttpPost("tweak")]
    public async Task<ActionResult<AiRecipeResponseDto>> TweakRecipe([FromBody] TweakWithRecipeRequestDto request)
    {
        try
        {
            // Validate that we have a recipe and a tweak request
            if (request.CurrentRecipe == null || string.IsNullOrEmpty(request.TweakRequest))
            {
                return BadRequest(new { message = "Both currentRecipe and tweakRequest are required" });
            }

            // Call the Gemini service to tweak the recipe
            // This sends the current recipe and modification request to the Gemini API
            var tweakedRecipe = await _geminiService.TweakRecipeAsync(request.CurrentRecipe, request.TweakRequest);

            // Return 200 OK with the modified recipe
            return Ok(tweakedRecipe);
        }
        catch (InvalidOperationException ex)
        {
            // Handle Gemini API errors or parsing errors
            return BadRequest(new { message = $"Error tweaking recipe: {ex.Message}" });
        }
        catch (HttpRequestException ex)
        {
            // Handle network errors when calling the Gemini API
            return StatusCode(503, new { message = $"Gemini API unavailable: {ex.Message}" });
        }
    }

    /// <summary>
    /// POST /api/ai/generate-image
    /// Generates a recipe image URL using Gemini and Unsplash.
    ///
    /// The request body must contain a GenerateImageRequestDto JSON object.
    /// Example:
    /// {
    ///   "recipeName": "Pasta Carbonara",
    ///   "cuisineType": "Italian"
    /// }
    ///
    /// HTTP Response Codes:
    /// 200 OK - Image generated successfully, returns { "imageUrl": "string" }
    /// 400 Bad Request - Invalid request (validation failed)
    /// 401 Unauthorized - User not authenticated (no valid JWT token)
    /// 503 Service Unavailable - Error calling Gemini API or Unsplash API
    ///
    /// [FromBody] tells ASP.NET to deserialize the JSON request body into the DTO.
    /// </summary>
    [HttpPost("generate-image")]
    public async Task<ActionResult<object>> GenerateRecipeImage([FromBody] GenerateImageRequestDto request)
    {
        try
        {
            // Call the Gemini service to generate the recipe image
            // This uses Gemini to create an optimized search query and then calls Unsplash
            var imageUrl = await _geminiService.GenerateRecipeImageAsync(
                request.RecipeName,
                request.CuisineType ?? string.Empty);

            // If image generation failed, return 503
            if (imageUrl == null)
                return StatusCode(503, new { error = "Image generation failed." });

            // Return 200 OK with the image URL
            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            // Handle any other errors
            return StatusCode(503, new { error = ex.Message });
        }
    }
}

/// <summary>
/// Helper DTO for the tweak endpoint.
///
/// The user spec specified TweakRecipeRequestDto with only TweakRequest,
/// but the endpoint needs both the current recipe and the tweak request.
/// This DTO combines both pieces of information.
/// </summary>
public class TweakWithRecipeRequestDto
{
    /// <summary>
    /// The current recipe to be modified.
    /// </summary>
    public AiRecipeResponseDto? CurrentRecipe { get; set; }

    /// <summary>
    /// The modification request (e.g., "make it spicier", "reduce cook time").
    /// </summary>
    public string? TweakRequest { get; set; }
}

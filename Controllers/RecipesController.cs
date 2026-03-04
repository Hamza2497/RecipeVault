using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecipeVault.DTOs;
using RecipeVault.Models;
using RecipeVault.Repositories;

// Helper method to extract authenticated user ID from JWT claims
// We use this in multiple endpoints, so defining it once here reduces code duplication

namespace RecipeVault.Controllers;

/// <summary>
/// API Controller for recipe operations.
///
/// Controllers handle HTTP requests and return responses.
/// They act as the bridge between the client (frontend, mobile app, etc.)
/// and the business logic (repositories, services).
///
/// Routing:
/// [ApiController] - tells ASP.NET this is a REST API controller
/// [Route("api/[controller]")] - routes all endpoints to /api/recipes (controller name = "Recipes")
///
/// This controller uses dependency injection to receive IRecipeRepository.
/// The DI container automatically provides a RecipeRepository instance.
///
/// Security:
/// [Authorize] - all endpoints in this controller require a valid JWT token
/// Only authenticated users (with a valid token) can access recipe operations.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipesController : ControllerBase
{
    /// <summary>
    /// The recipe repository, injected via dependency injection.
    /// This abstraction lets us swap implementations later if needed.
    /// </summary>
    private readonly IRecipeRepository _repository;

    /// <summary>
    /// Constructor: receives the injected repository.
    /// ASP.NET automatically provides this when a request arrives.
    /// </summary>
    public RecipesController(IRecipeRepository repository)
    {
        _repository = repository;
    }

    /// <summary>
    /// Helper method to extract the authenticated user's ID from the JWT token.
    ///
    /// We use this method in multiple endpoints to get the user ID from the current request's claims.
    /// This keeps the code DRY (Don't Repeat Yourself) and makes error handling consistent.
    ///
    /// Returns (userId, null) if successful, or (0, errorResponse) if extraction fails.
    /// </summary>
    private (int userId, ActionResult? error) GetUserIdFromClaims()
    {
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            var errorResponse = Unauthorized(new { message = "Invalid token - user ID not found" });
            return (0, errorResponse);
        }

        return (userId, null);
    }

    /// <summary>
    /// GET /api/recipes
    /// Retrieves all recipes belonging to the authenticated user.
    ///
    /// Security: This endpoint returns only the recipes that belong to the requesting user.
    /// Users cannot see or access other users' recipes through this endpoint.
    /// The user ID is extracted from the JWT token in the Authorization header.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Returns list of user's recipes (even if empty)
    /// 401 Unauthorized - If the JWT token is invalid or missing
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<RecipeResponseDto>>> GetAll()
    {
        // Extract the authenticated user's ID from the JWT token
        var (userId, error) = GetUserIdFromClaims();
        if (error != null)
        {
            return error;
        }

        // Fetch all recipes from the repository
        var recipes = await _repository.GetAllAsync();

        // Filter to only include recipes that belong to the authenticated user
        // This is a CRITICAL security step - it prevents users from seeing each other's recipes
        var userRecipes = recipes.Where(r => r.UserId == userId).ToList();

        // Convert each Recipe model to a RecipeResponseDto
        // This "mapping" ensures we only return the fields defined in RecipeResponseDto
        var dtos = userRecipes.Select(MapToDto).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// GET /api/recipes/search
    /// Searches and filters recipes belonging to the authenticated user.
    ///
    /// This endpoint allows flexible searching with optional filters that can be combined:
    /// - name: searches recipe names (case-insensitive)
    /// - ingredient: searches for recipes containing a specific ingredient (substring match)
    /// - cuisineType: filters by cuisine type (exact match)
    /// - maxPrepTime: finds recipes with prep time ≤ this value in minutes
    ///
    /// Security: Results are filtered to only show the authenticated user's recipes.
    /// Users cannot search or discover recipes belonging to other users through this endpoint.
    ///
    /// Examples:
    /// GET /api/recipes/search?name=Pasta
    /// GET /api/recipes/search?cuisineType=Thai&maxPrepTime=20
    /// GET /api/recipes/search?ingredient=garlic&cuisineType=Italian
    ///
    /// HTTP Response Codes:
    /// 200 OK - Returns list of matching recipes (empty list if no matches)
    /// 401 Unauthorized - If the JWT token is invalid or missing
    ///
    /// [FromQuery] tells ASP.NET to read these values from the URL query string, not the request body.
    /// All parameters are optional - they default to null if not provided.
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<List<RecipeResponseDto>>> Search(
        [FromQuery] string? name = null,
        [FromQuery] string? ingredient = null,
        [FromQuery] string? cuisineType = null,
        [FromQuery] int? maxPrepTime = null
    )
    {
        // Extract the authenticated user's ID from the JWT token
        var (userId, error) = GetUserIdFromClaims();
        if (error != null)
        {
            return error;
        }

        // Call the repository method to get filtered recipes
        // We pass the optional parameters as-is; the repository handles null checks
        var recipes = await _repository.SearchRecipesAsync(name, ingredient, cuisineType, maxPrepTime);

        // Filter to only include recipes that belong to the authenticated user
        // This ensures users can only search within their own recipe collection
        var userRecipes = recipes.Where(r => r.UserId == userId).ToList();

        // Convert each Recipe model to a RecipeResponseDto
        // This ensures we only return the API-facing fields, not internal database fields
        var dtos = userRecipes.Select(MapToDto).ToList();

        // Return 200 OK with the search results (even if empty)
        return Ok(dtos);
    }

    /// <summary>
    /// GET /api/recipes/public
    /// Retrieves all recipes that are marked as public.
    ///
    /// This endpoint is accessible to all users, including those without authentication.
    /// It returns a list of recipes that recipe owners have explicitly shared publicly.
    ///
    /// This endpoint enables recipe discovery and sharing without requiring login,
    /// allowing users to browse and potentially save public recipes from other users.
    ///
    /// Security: This endpoint does NOT require authentication ([AllowAnonymous]).
    /// Only recipes where IsPublic == true are returned.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Returns list of public recipes (even if empty)
    ///
    /// Note: This endpoint is placed before the {id} route to avoid conflicts.
    /// ASP.NET routes are evaluated top-to-bottom, so more specific routes must come first.
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    public async Task<ActionResult<List<RecipeResponseDto>>> GetPublicRecipes()
    {
        // Fetch all public recipes from the repository
        // The repository handles the filtering (IsPublic == true)
        var recipes = await _repository.GetPublicRecipesAsync();

        // Convert each Recipe model to a RecipeResponseDto
        // This ensures we only return the API-facing fields
        var dtos = recipes.Select(MapToDto).ToList();

        // Return 200 OK with the public recipes (even if empty)
        return Ok(dtos);
    }

    /// <summary>
    /// GET /api/recipes/{id}
    /// Retrieves a single recipe by its ID, if it belongs to the authenticated user.
    ///
    /// Security: This endpoint verifies that the requested recipe belongs to the authenticated user
    /// before returning it. A user cannot retrieve another user's recipe, even if they know the ID.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Returns the recipe
    /// 404 Not Found - If the recipe doesn't exist or doesn't belong to the authenticated user
    /// 401 Unauthorized - If the JWT token is invalid or missing
    ///
    /// {id} is a route parameter: /api/recipes/5 → id = 5
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RecipeResponseDto>> GetById(int id)
    {
        // Extract the authenticated user's ID from the JWT token
        var (userId, error) = GetUserIdFromClaims();
        if (error != null)
        {
            return error;
        }

        // Try to fetch the recipe
        var recipe = await _repository.GetByIdAsync(id);

        // If not found, or if it doesn't belong to the authenticated user, return 404
        if (recipe == null || recipe.UserId != userId)
        {
            return NotFound();
        }

        // Convert and return the recipe
        return Ok(MapToDto(recipe));
    }

    /// <summary>
    /// POST /api/recipes
    /// Creates a new recipe for the authenticated user.
    ///
    /// The request body must contain a CreateRecipeDto JSON object.
    /// Example: {"name": "Pasta", "description": "Italian pasta dish", ...}
    ///
    /// HTTP Response Codes:
    /// 201 Created - Recipe created successfully, returns the created recipe
    /// 400 Bad Request - Invalid request (validation failed)
    /// 401 Unauthorized - User not authenticated (no valid JWT token)
    ///
    /// [FromBody] tells ASP.NET to deserialize the JSON request body into the DTO
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RecipeResponseDto>> Create([FromBody] CreateRecipeDto createDto)
    {
        // Extract the authenticated user's ID from the JWT token claims.
        //
        // How it works:
        // 1. The [Authorize] attribute validates the JWT token
        // 2. ASP.NET extracts the claims (Id, Username, Email) from the token
        // 3. These claims are stored in User.Claims (available in every authenticated request)
        // 4. We look for the NameIdentifier claim, which contains the user's ID
        //
        // NameIdentifier is a standard claim type for user IDs in .NET.
        // It was set when we generated the JWT token in AuthService.GenerateJwtToken().
        string? userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        // If the claim doesn't exist, something went wrong - return an error
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "Invalid token - user ID not found" });
        }

        // Convert the DTO to a Recipe model
        var recipe = new Recipe
        {
            Name = createDto.Name,
            Description = createDto.Description,
            Ingredients = createDto.Ingredients,
            Instructions = createDto.Instructions,
            CuisineType = createDto.CuisineType,
            PrepTimeMinutes = createDto.PrepTimeMinutes,
            CookTimeMinutes = createDto.CookTimeMinutes,
            Servings = createDto.Servings,
            ImageUrl = createDto.ImageUrl,
            // Use the actual authenticated user's ID from the JWT token
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        // Save to database via repository
        var savedRecipe = await _repository.CreateAsync(recipe);

        // Convert to response DTO
        var responseDto = MapToDto(savedRecipe);

        // Return 201 Created with the new recipe and its location header
        // The "nameof(GetById)" generates the route name for the Location header
        return CreatedAtAction(nameof(GetById), new { id = savedRecipe.Id }, responseDto);
    }

    /// <summary>
    /// PUT /api/recipes/{id}
    /// Updates an existing recipe, if it belongs to the authenticated user.
    ///
    /// The request body contains an UpdateRecipeDto with the fields to update.
    /// Any fields in the DTO that are null/not provided will not change the recipe.
    ///
    /// Security: This endpoint verifies that the recipe belongs to the authenticated user
    /// before allowing any updates. Users can only modify their own recipes.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Recipe updated successfully
    /// 404 Not Found - If the recipe doesn't exist or doesn't belong to the authenticated user
    /// 400 Bad Request - Invalid request
    /// 401 Unauthorized - If the JWT token is invalid or missing
    ///
    /// {id} is a route parameter: /api/recipes/5 → id = 5
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RecipeResponseDto>> Update(int id, [FromBody] UpdateRecipeDto updateDto)
    {
        // Extract the authenticated user's ID from the JWT token
        var (userId, error) = GetUserIdFromClaims();
        if (error != null)
        {
            return error;
        }

        // First, check if the recipe exists and belongs to the authenticated user
        var recipe = await _repository.GetByIdAsync(id);
        if (recipe == null || recipe.UserId != userId)
        {
            return NotFound();
        }

        // Update only the fields provided in the DTO
        // We use the null-coalescing operator (??) to keep existing values if not provided
        if (!string.IsNullOrEmpty(updateDto.Name))
            recipe.Name = updateDto.Name;

        if (updateDto.Description != null)
            recipe.Description = updateDto.Description;

        if (updateDto.Ingredients != null)
            recipe.Ingredients = updateDto.Ingredients;

        if (updateDto.Instructions != null)
            recipe.Instructions = updateDto.Instructions;

        if (updateDto.CuisineType != null)
            recipe.CuisineType = updateDto.CuisineType;

        if (updateDto.PrepTimeMinutes.HasValue)
            recipe.PrepTimeMinutes = updateDto.PrepTimeMinutes.Value;

        if (updateDto.CookTimeMinutes.HasValue)
            recipe.CookTimeMinutes = updateDto.CookTimeMinutes.Value;

        if (updateDto.Servings.HasValue)
            recipe.Servings = updateDto.Servings.Value;

        if (updateDto.ImageUrl != null)
            recipe.ImageUrl = updateDto.ImageUrl;

        // Save the updated recipe
        var updatedRecipe = await _repository.UpdateAsync(recipe);

        // Convert to response DTO and return
        return Ok(MapToDto(updatedRecipe));
    }

    /// <summary>
    /// DELETE /api/recipes/{id}
    /// Deletes a recipe, if it belongs to the authenticated user.
    ///
    /// Security: This endpoint verifies that the recipe belongs to the authenticated user
    /// before allowing deletion. Users can only delete their own recipes.
    ///
    /// HTTP Response Codes:
    /// 204 No Content - Recipe deleted successfully (no response body)
    /// 404 Not Found - If the recipe doesn't exist or doesn't belong to the authenticated user
    /// 401 Unauthorized - If the JWT token is invalid or missing
    ///
    /// {id} is a route parameter: /api/recipes/5 → id = 5
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Extract the authenticated user's ID from the JWT token
        var (userId, error) = GetUserIdFromClaims();
        if (error != null)
        {
            return error;
        }

        // Check if the recipe exists and belongs to the authenticated user
        var recipe = await _repository.GetByIdAsync(id);
        if (recipe == null || recipe.UserId != userId)
        {
            return NotFound();
        }

        // Delete it
        await _repository.DeleteAsync(id);

        // Return 204 No Content (standard response for successful deletion)
        return NoContent();
    }

    /// <summary>
    /// PATCH /api/recipes/{id}/status
    /// Updates the status of a recipe (e.g., "favourite", "to-try", "made-before").
    ///
    /// The request body contains an UpdateStatusDto with the new status value.
    /// Valid status values are: "favourite", "to-try", "made-before", or null to clear the status.
    ///
    /// Security: Only the recipe owner can update the recipe's status.
    /// The authenticated user's ID is extracted from the JWT token and verified against the recipe owner.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Status updated successfully, returns the updated recipe
    /// 404 Not Found - If the recipe doesn't exist or doesn't belong to the authenticated user
    /// 400 Bad Request - Invalid request (validation failed)
    /// 401 Unauthorized - If the JWT token is invalid or missing
    ///
    /// {id} is a route parameter: /api/recipes/5/status → id = 5
    /// [FromBody] tells ASP.NET to deserialize the JSON request body into the DTO
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<ActionResult<RecipeResponseDto>> UpdateStatus(int id, [FromBody] UpdateStatusDto updateDto)
    {
        // Extract the authenticated user's ID from the JWT token
        var (userId, error) = GetUserIdFromClaims();
        if (error != null)
        {
            return error;
        }

        // Call the repository method to update the status with ownership verification
        // The repository returns false if the recipe doesn't exist or doesn't belong to the user
        var success = await _repository.UpdateStatusAsync(id, updateDto.Status, userId);

        if (!success)
        {
            return NotFound();
        }

        // Fetch the updated recipe to return in the response
        var updatedRecipe = await _repository.GetByIdAsync(id);
        if (updatedRecipe == null)
        {
            return NotFound();
        }

        // Convert to response DTO and return
        return Ok(MapToDto(updatedRecipe));
    }

    /// <summary>
    /// PATCH /api/recipes/{id}/visibility
    /// Updates whether a recipe is public or private.
    ///
    /// The request body contains an UpdateVisibilityDto with the IsPublic boolean.
    /// When IsPublic is true, the recipe will be visible in the public recipes endpoint.
    /// When IsPublic is false, the recipe is private and only visible to its owner.
    ///
    /// Security: Only the recipe owner can change the visibility of their recipe.
    /// The authenticated user's ID is extracted from the JWT token and verified against the recipe owner.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Visibility updated successfully, returns the updated recipe
    /// 404 Not Found - If the recipe doesn't exist or doesn't belong to the authenticated user
    /// 400 Bad Request - Invalid request (validation failed)
    /// 401 Unauthorized - If the JWT token is invalid or missing
    ///
    /// {id} is a route parameter: /api/recipes/5/visibility → id = 5
    /// [FromBody] tells ASP.NET to deserialize the JSON request body into the DTO
    /// </summary>
    [HttpPatch("{id}/visibility")]
    public async Task<ActionResult<RecipeResponseDto>> UpdateVisibility(int id, [FromBody] UpdateVisibilityDto updateDto)
    {
        // Extract the authenticated user's ID from the JWT token
        var (userId, error) = GetUserIdFromClaims();
        if (error != null)
        {
            return error;
        }

        // Call the repository method to update the visibility with ownership verification
        // The repository returns false if the recipe doesn't exist or doesn't belong to the user
        var success = await _repository.UpdateVisibilityAsync(id, updateDto.IsPublic, userId);

        if (!success)
        {
            return NotFound();
        }

        // Fetch the updated recipe to return in the response
        var updatedRecipe = await _repository.GetByIdAsync(id);
        if (updatedRecipe == null)
        {
            return NotFound();
        }

        // Convert to response DTO and return
        return Ok(MapToDto(updatedRecipe));
    }

    /// <summary>
    /// Helper method: Maps a Recipe model to a RecipeResponseDto.
    ///
    /// This method converts internal database models to the public API response format.
    /// Why? It ensures we only expose the fields we intend to, and it decouples
    /// the database schema from the API contract.
    ///
    /// This is a simple manual mapping. In larger projects, you'd use libraries
    /// like AutoMapper to automate this for you.
    /// </summary>
    private RecipeResponseDto MapToDto(Recipe recipe)
    {
        return new RecipeResponseDto
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            Ingredients = recipe.Ingredients,
            Instructions = recipe.Instructions,
            CuisineType = recipe.CuisineType,
            PrepTimeMinutes = recipe.PrepTimeMinutes,
            CookTimeMinutes = recipe.CookTimeMinutes,
            Servings = recipe.Servings,
            ImageUrl = recipe.ImageUrl,
            Status = recipe.Status,
            IsPublic = recipe.IsPublic,
            CreatedAt = recipe.CreatedAt
        };
    }
}

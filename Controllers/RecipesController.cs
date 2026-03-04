using Microsoft.AspNetCore.Mvc;
using RecipeVault.DTOs;
using RecipeVault.Models;
using RecipeVault.Repositories;

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
/// </summary>
[ApiController]
[Route("api/[controller]")]
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
    /// GET /api/recipes
    /// Retrieves all recipes from the database.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Returns list of recipes (even if empty)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<RecipeResponseDto>>> GetAll()
    {
        // Fetch all recipes from the repository
        var recipes = await _repository.GetAllAsync();

        // Convert each Recipe model to a RecipeResponseDto
        // This "mapping" ensures we only return the fields defined in RecipeResponseDto
        var dtos = recipes.Select(MapToDto).ToList();

        return Ok(dtos);
    }

    /// <summary>
    /// GET /api/recipes/{id}
    /// Retrieves a single recipe by its ID.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Returns the recipe
    /// 404 Not Found - If the recipe doesn't exist
    ///
    /// {id} is a route parameter: /api/recipes/5 → id = 5
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RecipeResponseDto>> GetById(int id)
    {
        // Try to fetch the recipe
        var recipe = await _repository.GetByIdAsync(id);

        // If not found, return 404
        if (recipe == null)
        {
            return NotFound();
        }

        // Convert and return the recipe
        return Ok(MapToDto(recipe));
    }

    /// <summary>
    /// POST /api/recipes
    /// Creates a new recipe.
    ///
    /// The request body must contain a CreateRecipeDto JSON object.
    /// Example: {"name": "Pasta", "description": "Italian pasta dish", ...}
    ///
    /// HTTP Response Codes:
    /// 201 Created - Recipe created successfully, returns the created recipe
    /// 400 Bad Request - Invalid request (validation failed)
    ///
    /// [FromBody] tells ASP.NET to deserialize the JSON request body into the DTO
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RecipeResponseDto>> Create([FromBody] CreateRecipeDto createDto)
    {
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
            // TODO: Replace with actual authenticated user's ID in Step 5
            UserId = 1, // Hardcoded for now - will use current user's ID after auth is implemented
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
    /// Updates an existing recipe.
    ///
    /// The request body contains an UpdateRecipeDto with the fields to update.
    /// Any fields in the DTO that are null/not provided will not change the recipe.
    ///
    /// HTTP Response Codes:
    /// 200 OK - Recipe updated successfully
    /// 404 Not Found - If the recipe doesn't exist
    /// 400 Bad Request - Invalid request
    ///
    /// {id} is a route parameter: /api/recipes/5 → id = 5
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RecipeResponseDto>> Update(int id, [FromBody] UpdateRecipeDto updateDto)
    {
        // First, check if the recipe exists
        var recipe = await _repository.GetByIdAsync(id);
        if (recipe == null)
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
    /// Deletes a recipe.
    ///
    /// HTTP Response Codes:
    /// 204 No Content - Recipe deleted successfully (no response body)
    /// 404 Not Found - If the recipe doesn't exist
    ///
    /// {id} is a route parameter: /api/recipes/5 → id = 5
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        // Check if the recipe exists
        var recipe = await _repository.GetByIdAsync(id);
        if (recipe == null)
        {
            return NotFound();
        }

        // Delete it
        await _repository.DeleteAsync(id);

        // Return 204 No Content (standard response for successful deletion)
        return NoContent();
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

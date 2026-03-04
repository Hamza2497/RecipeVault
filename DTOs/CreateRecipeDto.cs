namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for creating a new recipe.
///
/// DTOs are "data containers" used to transfer information between layers of your app.
/// They define what data a client can send to the API.
/// We use a separate DTO instead of accepting the Recipe model directly because:
/// 1. The client shouldn't send Id, CreatedAt, or UserId - the server generates these
/// 2. Status and IsPublic aren't part of recipe creation (they have defaults)
/// 3. DTOs decouple the API contract from the database model
///
/// This DTO requires Name but makes other fields optional, letting users create minimal recipes.
/// </summary>
public class CreateRecipeDto
{
    /// <summary>
    /// The recipe name (required). Must be provided when creating a recipe.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Optional description of the recipe.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional comma-separated list of ingredients.
    /// </summary>
    public string? Ingredients { get; set; }

    /// <summary>
    /// Optional step-by-step cooking instructions.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Optional cuisine type (e.g., "Italian", "Asian").
    /// </summary>
    public string? CuisineType { get; set; }

    /// <summary>
    /// Optional prep time in minutes.
    /// </summary>
    public int PrepTimeMinutes { get; set; }

    /// <summary>
    /// Optional cook time in minutes.
    /// </summary>
    public int CookTimeMinutes { get; set; }

    /// <summary>
    /// Optional number of servings.
    /// </summary>
    public int Servings { get; set; }

    /// <summary>
    /// Optional URL to a recipe image.
    /// </summary>
    public string? ImageUrl { get; set; }
}

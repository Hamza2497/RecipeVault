namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for updating an existing recipe.
///
/// This DTO is similar to CreateRecipeDto but makes ALL fields optional.
/// This allows clients to update just the fields they want to change
/// without having to send the entire recipe.
///
/// For example, a client can send just {"Name": "New Name"} to update only the name,
/// leaving other fields unchanged.
/// </summary>
public class UpdateRecipeDto
{
    /// <summary>
    /// Optional: update the recipe name.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Optional: update the recipe description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Optional: update the ingredients list.
    /// </summary>
    public string? Ingredients { get; set; }

    /// <summary>
    /// Optional: update the cooking instructions.
    /// </summary>
    public string? Instructions { get; set; }

    /// <summary>
    /// Optional: update the cuisine type.
    /// </summary>
    public string? CuisineType { get; set; }

    /// <summary>
    /// Optional: update the prep time in minutes.
    /// </summary>
    public int? PrepTimeMinutes { get; set; }

    /// <summary>
    /// Optional: update the cook time in minutes.
    /// </summary>
    public int? CookTimeMinutes { get; set; }

    /// <summary>
    /// Optional: update the number of servings.
    /// </summary>
    public int? Servings { get; set; }

    /// <summary>
    /// Optional: update the recipe image URL.
    /// </summary>
    public string? ImageUrl { get; set; }
}

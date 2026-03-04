namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object for AI-generated recipe responses.
///
/// This DTO represents a recipe generated or modified by the Gemini AI.
/// It includes structured data for ingredients and instructions as lists,
/// making it easy for clients to display and work with the recipe data.
///
/// This DTO is used as a response for:
/// - POST /api/ai/generate (generates a new recipe)
/// - POST /api/ai/tweak (modifies an existing recipe)
/// </summary>
public class AiRecipeResponseDto
{
    /// <summary>
    /// The recipe name.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The list of ingredients for this recipe.
    /// Each item is a string like "2 cups flour" or "1 tablespoon olive oil".
    /// </summary>
    public required List<string> Ingredients { get; set; }

    /// <summary>
    /// The cooking instructions for this recipe.
    /// Each item is a numbered step (the number prefix is included in the string).
    /// Examples: "1. Preheat oven to 350°F", "2. Mix flour and sugar"
    /// </summary>
    public required List<string> Instructions { get; set; }

    /// <summary>
    /// The cuisine type (e.g., "Italian", "Asian", "Mexican").
    /// </summary>
    public string? CuisineType { get; set; }

    /// <summary>
    /// Preparation time in minutes.
    /// The time required to prepare ingredients before cooking.
    /// </summary>
    public int PrepTimeMinutes { get; set; }

    /// <summary>
    /// Cooking time in minutes.
    /// The time required to actively cook the recipe.
    /// </summary>
    public int CookTimeMinutes { get; set; }

    /// <summary>
    /// Number of servings this recipe makes.
    /// </summary>
    public int Servings { get; set; }
}

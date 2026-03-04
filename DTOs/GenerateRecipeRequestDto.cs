namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object for requesting AI-generated recipe creation.
///
/// This DTO captures the user's constraints and preferences for generating a new recipe.
/// All fields except RecipeName are optional, allowing flexible recipe generation.
/// The GeminiService uses these constraints to craft a detailed prompt for the AI.
/// </summary>
public class GenerateRecipeRequestDto
{
    /// <summary>
    /// The name or type of recipe to generate (required).
    /// Examples: "Chocolate Cake", "Vegetable Stir Fry", "Fish Tacos"
    /// </summary>
    public required string RecipeName { get; set; }

    /// <summary>
    /// The cuisine type to generate (optional).
    /// Examples: "Italian", "Thai", "Mexican", "Indian"
    /// If not provided, the AI will choose an appropriate cuisine.
    /// </summary>
    public string? CuisineType { get; set; }

    /// <summary>
    /// Maximum preparation time in minutes (optional).
    /// The AI will generate recipes that can be prepped within this time.
    /// If not provided, no prep time constraint is applied.
    /// </summary>
    public int? MaxPrepTimeMinutes { get; set; }

    /// <summary>
    /// Maximum cooking time in minutes (optional).
    /// The AI will generate recipes that can be cooked within this time.
    /// If not provided, no cook time constraint is applied.
    /// </summary>
    public int? MaxCookTimeMinutes { get; set; }

    /// <summary>
    /// List of allergens to avoid (optional).
    /// Examples: ["peanuts", "shellfish", "dairy", "gluten"]
    /// The AI will exclude ingredients containing these allergens.
    /// </summary>
    public List<string>? Allergies { get; set; }

    /// <summary>
    /// List of dietary restrictions to follow (optional).
    /// Examples: ["vegan", "gluten-free", "low-carb", "keto"]
    /// The AI will ensure the recipe adheres to these restrictions.
    /// </summary>
    public List<string>? DietaryRestrictions { get; set; }
}

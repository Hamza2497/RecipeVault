namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object for requesting modifications to an existing recipe.
///
/// This DTO captures a user's request to modify an existing recipe.
/// The request is sent to the GeminiService along with the current recipe,
/// and the AI returns a modified version of the recipe.
/// </summary>
public class TweakRecipeRequestDto
{
    /// <summary>
    /// The modification request (required).
    /// Examples: "make it spicier", "reduce cook time by half", "make it vegan",
    /// "add more garlic", "use fewer calories", "make it less salty"
    /// This is a natural language instruction that the AI will interpret and apply.
    /// </summary>
    public required string TweakRequest { get; set; }
}

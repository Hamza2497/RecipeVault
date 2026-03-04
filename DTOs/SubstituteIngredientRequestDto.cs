namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object for requesting ingredient substitutes.
///
/// This DTO captures only the optional reason for needing a substitute.
/// The ingredient name comes from the URL path parameter in the endpoint (e.g., /substitute/butter).
/// The GeminiService uses this information to generate relevant substitute suggestions.
/// The reason helps the AI choose appropriate substitutes (e.g., different substitutes for
/// allergies vs. taste preferences vs. availability).
/// </summary>
public class SubstituteIngredientRequestDto
{
    /// <summary>
    /// The ingredient to replace (optional - now comes from URL path instead).
    /// The ingredient name is provided as a path parameter in the endpoint route.
    /// Example: POST /api/ai/substitute/butter
    /// This property is kept for compatibility if needed in the request body.
    /// </summary>
    public string? IngredientToReplace { get; set; }

    /// <summary>
    /// The reason for needing a substitute (optional).
    /// Examples: "allergy", "preference", "not available", "dietary restriction"
    /// This helps the AI choose contextually appropriate substitutes.
    /// If not provided, the AI will provide general-purpose substitutes.
    /// </summary>
    public string? Reason { get; set; }
}

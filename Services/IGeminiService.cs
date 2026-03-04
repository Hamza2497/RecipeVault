using RecipeVault.DTOs;

namespace RecipeVault.Services;

/// <summary>
/// Interface for Gemini AI service.
///
/// An interface defines a "contract" - a list of methods that any class implementing it must provide.
/// This allows us to:
/// 1. Swap implementations (e.g., use a different AI service) without changing dependent code
/// 2. Mock the service for unit testing
/// 3. Keep the service logic separate from the controller code
///
/// The GeminiService class implements these methods to call the Gemini API
/// and convert the responses into DTOs.
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// Generates a new recipe based on user constraints.
    ///
    /// This method:
    /// 1. Takes the user's constraints (recipe name, cuisine, prep/cook time limits, allergies, dietary restrictions)
    /// 2. Constructs a detailed prompt for the Gemini API
    /// 3. Sends the prompt and receives JSON formatted recipe data back
    /// 4. Parses and returns the recipe as an AiRecipeResponseDto
    ///
    /// The Gemini API is instructed to return ONLY JSON with no preamble or markdown formatting.
    /// </summary>
    Task<AiRecipeResponseDto> GenerateRecipeAsync(GenerateRecipeRequestDto request);

    /// <summary>
    /// Returns a list of ingredient substitutes.
    ///
    /// This method:
    /// 1. Takes an ingredient name and optional reason for replacement
    /// 2. Constructs a prompt asking for 3 substitute options
    /// 3. Sends the prompt to the Gemini API
    /// 4. Receives and parses a JSON array of substitute ingredient names
    /// 5. Returns the list of substitutes
    ///
    /// The returned list contains maximum 3 alternatives.
    /// </summary>
    Task<List<string>> GetIngredientSubstitutesAsync(string ingredientToReplace, string? reason = null);

    /// <summary>
    /// Modifies an existing recipe based on a user request.
    ///
    /// This method:
    /// 1. Takes the current recipe and a tweak request (e.g., "make it spicier")
    /// 2. Constructs a prompt with the recipe details and modification request
    /// 3. Sends the prompt to the Gemini API
    /// 4. Receives the modified recipe as JSON
    /// 5. Parses and returns the updated recipe as an AiRecipeResponseDto
    ///
    /// The modification request is interpreted naturally by the AI, allowing for
    /// flexible requests like "reduce cook time", "add more vegetables", "make it keto", etc.
    /// </summary>
    Task<AiRecipeResponseDto> TweakRecipeAsync(AiRecipeResponseDto currentRecipe, string tweakRequest);
}

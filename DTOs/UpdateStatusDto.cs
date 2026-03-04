namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for updating a recipe's status.
///
/// This DTO is used when a client wants to change the status of a recipe
/// (e.g., marking it as a favourite, adding it to "to-try", or marking it as "made before").
///
/// The Status field accepts one of three values:
/// - "favourite": The user's favourite recipe
/// - "to-try": A recipe the user wants to try
/// - "made-before": A recipe the user has already made
///
/// By using a DTO for this request, we ensure type safety and make the API contract clear.
/// </summary>
public class UpdateStatusDto
{
    /// <summary>
    /// The new status for the recipe.
    /// Valid values: "favourite", "to-try", "made-before"
    /// </summary>
    public required string Status { get; set; }
}

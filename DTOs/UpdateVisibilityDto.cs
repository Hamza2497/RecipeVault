namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for updating a recipe's visibility.
///
/// This DTO is used when a client wants to change whether a recipe is public or private.
/// When IsPublic is true, the recipe is visible to all users (it will show up in the public recipes endpoint).
/// When IsPublic is false, the recipe is private and only visible to the recipe owner.
///
/// This is a simple wrapper around a single boolean, but using a DTO provides:
/// 1. Clarity: The intent is explicit in the API contract
/// 2. Flexibility: We can add more visibility-related fields in the future without breaking the API
/// 3. Consistency: All request bodies follow the DTO pattern
/// </summary>
public class UpdateVisibilityDto
{
    /// <summary>
    /// Whether the recipe should be public (true) or private (false).
    /// </summary>
    public required bool IsPublic { get; set; }
}

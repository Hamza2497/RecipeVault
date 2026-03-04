using RecipeVault.Models;

namespace RecipeVault.Repositories;

/// <summary>
/// Repository interface for Recipe data access operations.
///
/// An interface defines a "contract" - it specifies what methods must exist
/// without implementing them. The RecipeRepository class will implement this interface.
///
/// Why use interfaces?
/// 1. Abstraction: Controllers depend on the interface, not the concrete class
/// 2. Testability: You can create fake/mock repositories for testing
/// 3. Flexibility: You can swap implementations (e.g., swap SQLite for PostgreSQL)
///    without changing the controller code
///
/// This is the "Repository Pattern" - a common design pattern for data access.
/// The repository acts as an "in-memory collection" abstraction over the database.
/// </summary>
public interface IRecipeRepository
{
    /// <summary>
    /// Retrieves all recipes from the database.
    /// Async methods are marked with Task/Task&lt;T&gt; and should be awaited.
    /// </summary>
    /// <returns>A collection of all recipes.</returns>
    Task<List<Recipe>> GetAllAsync();

    /// <summary>
    /// Retrieves a single recipe by its ID.
    /// Returns null if no recipe with that ID exists.
    /// </summary>
    /// <param name="id">The recipe ID to look up.</param>
    /// <returns>The recipe if found; null otherwise.</returns>
    Task<Recipe?> GetByIdAsync(int id);

    /// <summary>
    /// Creates and saves a new recipe to the database.
    /// The recipe object is prepared by the caller (the controller).
    /// </summary>
    /// <param name="recipe">The Recipe object to save. Must have Name and UserId set.</param>
    /// <returns>The saved recipe (may include generated fields like Id and CreatedAt).</returns>
    Task<Recipe> CreateAsync(Recipe recipe);

    /// <summary>
    /// Updates an existing recipe in the database.
    /// The recipe object should have the Id set to identify which record to update.
    /// </summary>
    /// <param name="recipe">The Recipe object with updated values.</param>
    /// <returns>The updated recipe.</returns>
    Task<Recipe> UpdateAsync(Recipe recipe);

    /// <summary>
    /// Deletes a recipe from the database by its ID.
    /// </summary>
    /// <param name="id">The ID of the recipe to delete.</param>
    Task DeleteAsync(int id);

    /// <summary>
    /// Searches for recipes based on optional filter parameters.
    ///
    /// This method demonstrates flexible filtering - each parameter is optional.
    /// If a parameter is null or empty, that filter is skipped.
    /// This allows callers to use any combination of filters.
    ///
    /// For example:
    /// - SearchAsync("Pasta", null, null, null) finds recipes by name only
    /// - SearchAsync(null, "garlic", "Italian", null) combines ingredient and cuisine filters
    /// - SearchAsync(null, null, null, 30) finds recipes with prep time ≤ 30 minutes
    ///
    /// Why optional parameters? Flexibility. Different searches need different filters,
    /// and having one method handle all combinations is cleaner than multiple overloads.
    /// </summary>
    /// <param name="name">Optional: Search for recipe name (case-insensitive partial match)</param>
    /// <param name="ingredient">Optional: Search for ingredient substring in the ingredients list</param>
    /// <param name="cuisineType">Optional: Filter by exact cuisine type match</param>
    /// <param name="maxPrepTime">Optional: Filter for recipes with prep time ≤ this value (in minutes)</param>
    /// <returns>A list of recipes matching the provided filters.</returns>
    Task<List<Recipe>> SearchRecipesAsync(
        string? name = null,
        string? ingredient = null,
        string? cuisineType = null,
        int? maxPrepTime = null
    );
}

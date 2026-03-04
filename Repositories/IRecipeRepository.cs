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
}

using Microsoft.EntityFrameworkCore;
using RecipeVault.Data;
using RecipeVault.Models;

namespace RecipeVault.Repositories;

/// <summary>
/// Concrete implementation of IRecipeRepository using Entity Framework and SQLite.
///
/// This class implements the IRecipeRepository interface and performs actual database operations
/// using AppDbContext (our Entity Framework context). It abstracts away the DbContext details
/// from the controller layer.
///
/// Why have this separate class?
/// 1. The controller talks to IRecipeRepository (the interface)
/// 2. The repository talks to AppDbContext (the database)
/// 3. This separation makes testing and changing databases easier
///
/// Pattern: Dependency Injection
/// This class receives AppDbContext via constructor injection.
/// The DI container automatically provides the right instance when needed.
/// </summary>
public class RecipeRepository : IRecipeRepository
{
    /// <summary>
    /// The database context, injected via dependency injection.
    /// This is the bridge between C# objects and the SQLite database.
    /// </summary>
    private readonly AppDbContext _context;

    /// <summary>
    /// Constructor: receives the database context.
    /// The DI system automatically provides this when creating a RecipeRepository.
    /// </summary>
    public RecipeRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Gets all recipes from the database.
    /// ToListAsync() runs the query asynchronously and returns all results as a List.
    /// </summary>
    public async Task<List<Recipe>> GetAllAsync()
    {
        return await _context.Recipes.ToListAsync();
    }

    /// <summary>
    /// Gets a single recipe by ID.
    /// FirstOrDefaultAsync() returns the first matching item or null if not found.
    /// This is better than Find() because it can apply filters like we might later.
    /// </summary>
    public async Task<Recipe?> GetByIdAsync(int id)
    {
        return await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
    }

    /// <summary>
    /// Creates a new recipe in the database.
    /// Steps:
    /// 1. Add the recipe to the context (it's now "tracked")
    /// 2. SaveChangesAsync() persists it to the database
    /// 3. Return the saved recipe (which now has its database-generated Id)
    /// </summary>
    public async Task<Recipe> CreateAsync(Recipe recipe)
    {
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();
        return recipe;
    }

    /// <summary>
    /// Updates an existing recipe in the database.
    /// Steps:
    /// 1. Update() tells EF to mark the recipe as modified
    /// 2. SaveChangesAsync() sends the UPDATE command to the database
    /// 3. Return the updated recipe
    /// </summary>
    public async Task<Recipe> UpdateAsync(Recipe recipe)
    {
        _context.Recipes.Update(recipe);
        await _context.SaveChangesAsync();
        return recipe;
    }

    /// <summary>
    /// Deletes a recipe by its ID.
    /// Steps:
    /// 1. Find the recipe with the given ID
    /// 2. If it exists, remove it from the context
    /// 3. SaveChangesAsync() deletes it from the database
    /// </summary>
    public async Task DeleteAsync(int id)
    {
        var recipe = await _context.Recipes.FirstOrDefaultAsync(r => r.Id == id);
        if (recipe != null)
        {
            _context.Recipes.Remove(recipe);
            await _context.SaveChangesAsync();
        }
    }
}

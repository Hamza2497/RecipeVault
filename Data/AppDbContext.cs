using Microsoft.EntityFrameworkCore;
using RecipeVault.Models;

namespace RecipeVault.Data;

/// <summary>
/// The main database context for RecipeVault.
///
/// DbContext is the bridge between your C# objects and the database.
/// It tracks changes to your models and translates them into SQL commands.
/// Each DbSet&lt;T&gt; property represents a table in the database.
/// </summary>
public class AppDbContext : DbContext
{
    /// <summary>
    /// Constructor that accepts Entity Framework's DbContextOptions.
    /// This allows dependency injection to configure the database connection at startup.
    /// In Program.cs, we tell EF which database to use (SQLite in our case).
    /// </summary>
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// The Users table.
    /// DbSet&lt;User&gt; allows you to query and save User objects.
    /// Example: await context.Users.ToListAsync();
    /// </summary>
    public DbSet<User> Users { get; set; }

    /// <summary>
    /// The Recipes table.
    /// DbSet&lt;Recipe&gt; allows you to query and save Recipe objects.
    /// Example: await context.Recipes.Include(r => r.User).ToListAsync();
    /// The Include method loads the related User data in one query.
    /// </summary>
    public DbSet<Recipe> Recipes { get; set; }

    /// <summary>
    /// Optional: Override OnModelCreating to configure relationships and constraints.
    /// For now, we rely on Entity Framework conventions to set up the database.
    ///
    /// Conventions used:
    /// - Property named "Id" → Primary Key
    /// - Property named "{EntityName}Id" (e.g., UserId) → Foreign Key
    /// - "required" modifier → NOT NULL in database
    /// - Navigation properties → Relationship configurations
    /// </summary>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure the relationship between User and Recipe
        // A user can have many recipes, a recipe belongs to one user
        modelBuilder.Entity<Recipe>()
            .HasOne(r => r.User)           // Recipe has one User
            .WithMany(u => u.Recipes)      // User has many Recipes
            .HasForeignKey(r => r.UserId)  // The foreign key is UserId
            .OnDelete(DeleteBehavior.Cascade); // If a user is deleted, delete their recipes too
    }
}

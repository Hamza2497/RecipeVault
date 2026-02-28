namespace RecipeVault.Models;

/// <summary>
/// Represents a user account in the RecipeVault application.
///
/// In a real application, passwords would be hashed using a secure algorithm like bcrypt.
/// The PasswordHash field stores the hashed version, never the plain password.
/// </summary>
public class User
{
    /// <summary>
    /// Primary key - uniquely identifies this user in the database.
    /// Entity Framework automatically recognizes properties named "Id" as primary keys.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// The username for login. Required - users must provide this when signing up.
    /// The "required" modifier ensures this field cannot be null.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The user's email address. Required for password recovery and notifications.
    /// The "required" modifier ensures this field cannot be null.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// The hashed version of the user's password (never store plain passwords!).
    /// This is computed on the server during registration/password change.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Timestamp when this user account was created.
    /// Set to UTC now when a new user registers.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Navigation property: a list of all recipes owned by this user.
    /// This allows you to write code like: var userRecipes = user.Recipes;
    /// Entity Framework manages this relationship - you don't manually populate it.
    /// </summary>
    public List<Recipe> Recipes { get; set; } = new();
}

namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for user registration.
///
/// DTOs are "data containers" that carry information between your API's public interface
/// and your internal business logic. They let you control exactly what data clients can send.
///
/// This DTO defines the required fields for creating a new user account.
/// The "required" modifier means these fields cannot be null or empty.
/// </summary>
public class RegisterDto
{
    /// <summary>
    /// The username the user chooses to sign in with.
    /// Required - clients must provide this when registering.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The user's email address for account recovery and notifications.
    /// Required - clients must provide this when registering.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// The plain-text password the user enters.
    /// IMPORTANT: This is ONLY transmitted over HTTPS and is hashed on the server immediately.
    /// We NEVER store the plain password in the database.
    /// Required - clients must provide this when registering.
    /// </summary>
    public required string Password { get; set; }
}

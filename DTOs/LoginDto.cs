namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for user login.
///
/// This DTO defines the fields required for a user to authenticate.
/// Unlike RegisterDto, it doesn't need a username field—just the email and password.
/// </summary>
public class LoginDto
{
    /// <summary>
    /// The user's email address - used to look up their account.
    /// Required - clients must provide this when logging in.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// The plain-text password the user enters.
    /// The server will hash it and compare with the stored PasswordHash.
    /// Required - clients must provide this when logging in.
    /// </summary>
    public required string Password { get; set; }
}

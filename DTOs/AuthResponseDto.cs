namespace RecipeVault.DTOs;

/// <summary>
/// Data Transfer Object (DTO) for user information in authentication responses.
/// Contains the user's ID, username, and email.
/// </summary>
public class UserDto
{
    /// <summary>
    /// The unique identifier for the user.
    /// </summary>
    public required int Id { get; set; }

    /// <summary>
    /// The username of the user.
    /// </summary>
    public required string Username { get; set; }

    /// <summary>
    /// The email of the user.
    /// </summary>
    public required string Email { get; set; }
}

/// <summary>
/// Data Transfer Object (DTO) for authentication responses.
///
/// This is returned after successful registration or login.
/// It contains the JWT token the client will use to authenticate future API requests,
/// plus nested user information.
/// </summary>
public class AuthResponseDto
{
    /// <summary>
    /// The JWT token the client will use to authenticate requests.
    ///
    /// How it works:
    /// 1. Client receives this token after login/registration
    /// 2. Client includes it in every API request as: Authorization: Bearer {token}
    /// 3. Server validates the token, extracts the user info, and allows the request
    ///
    /// The token is cryptographically signed so it can't be forged or tampered with.
    /// </summary>
    public required string Token { get; set; }

    /// <summary>
    /// The authenticated user's information (ID, username, email).
    /// </summary>
    public required UserDto User { get; set; }
}

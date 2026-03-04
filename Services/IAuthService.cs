using RecipeVault.DTOs;

namespace RecipeVault.Services;

/// <summary>
/// Interface for authentication service.
///
/// An interface defines a "contract" - a list of methods that any class implementing it must provide.
/// This lets us swap implementations later (for example, to use a different JWT library)
/// without changing the code that uses it.
///
/// The AuthService class will implement these methods to handle user registration and login.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user account.
    ///
    /// This method:
    /// 1. Validates that the email is not already registered
    /// 2. Hashes the password using BCrypt (irreversible - makes it secure)
    /// 3. Creates a new User in the database
    /// 4. Generates and returns a JWT token (so they're logged in immediately)
    ///
    /// Throws an exception if the email is already registered.
    /// </summary>
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);

    /// <summary>
    /// Authenticate a user and return a JWT token.
    ///
    /// This method:
    /// 1. Finds the user by email
    /// 2. Hashes the provided password and compares it with the stored hash
    /// 3. If the password matches, generates and returns a JWT token
    ///
    /// Throws an exception if the user doesn't exist or the password is wrong.
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}

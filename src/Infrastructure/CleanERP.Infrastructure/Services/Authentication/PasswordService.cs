using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using CleanERP.Application.Common.Interfaces;
using CleanERP.Shared.Configuration;
using CleanERP.Shared.Models;

namespace CleanERP.Infrastructure.Services.Authentication;

/// <summary>
/// Password service implementation for hashing and validation
/// </summary>
public class PasswordService : IPasswordService
{
    private readonly PasswordSettings _passwordSettings;
    private readonly ILogger<PasswordService> _logger;

    public PasswordService(
        IOptions<PasswordSettings> passwordSettings,
        ILogger<PasswordService> logger)
    {
        _passwordSettings = passwordSettings.Value;
        _logger = logger;
    }

    public string HashPassword(string password)
    {
        try
        {
            // Generate a random salt
            byte[] salt = new byte[16];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            // Hash the password with the salt using PBKDF2
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] hash = pbkdf2.GetBytes(32);

            // Combine salt and hash
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            // Convert to base64 string
            return Convert.ToBase64String(hashBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error hashing password");
            throw;
        }
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            // Convert hash from base64
            byte[] hashBytes = Convert.FromBase64String(hash);

            // Extract the salt (first 16 bytes)
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Hash the provided password with the extracted salt
            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
            byte[] computedHash = pbkdf2.GetBytes(32);

            // Compare the computed hash with the stored hash (last 32 bytes)
            for (int i = 0; i < 32; i++)
            {
                if (hashBytes[i + 16] != computedHash[i])
                {
                    return false;
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying password");
            return false;
        }
    }

    public Result<bool> ValidatePassword(string password)
    {
        try
        {
            var errors = new List<string>();

            // Check length
            if (password.Length < _passwordSettings.RequiredLength)
            {
                errors.Add($"Password must be at least {_passwordSettings.RequiredLength} characters long");
            }

            // Check for digit
            if (_passwordSettings.RequireDigit && !password.Any(char.IsDigit))
            {
                errors.Add("Password must contain at least one digit");
            }

            // Check for lowercase
            if (_passwordSettings.RequireLowercase && !password.Any(char.IsLower))
            {
                errors.Add("Password must contain at least one lowercase letter");
            }

            // Check for uppercase
            if (_passwordSettings.RequireUppercase && !password.Any(char.IsUpper))
            {
                errors.Add("Password must contain at least one uppercase letter");
            }

            // Check for non-alphanumeric
            if (_passwordSettings.RequireNonAlphanumeric && password.All(char.IsLetterOrDigit))
            {
                errors.Add("Password must contain at least one special character");
            }

            // Check for common patterns
            if (IsCommonPassword(password))
            {
                errors.Add("Password is too common. Please choose a more unique password");
            }

            if (errors.Any())
            {
                return Result<bool>.Failure(string.Join(". ", errors));
            }

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating password");
            return Result<bool>.Failure("Password validation error");
        }
    }

    public string GenerateRandomPassword(int length = 12)
    {
        try
        {
            const string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*";
            var random = new Random();
            var password = new StringBuilder();

            // Ensure at least one character from each required category
            if (_passwordSettings.RequireLowercase)
                password.Append("abcdefghijklmnopqrstuvwxyz"[random.Next(26)]);
            
            if (_passwordSettings.RequireUppercase)
                password.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZ"[random.Next(26)]);
            
            if (_passwordSettings.RequireDigit)
                password.Append("1234567890"[random.Next(10)]);
            
            if (_passwordSettings.RequireNonAlphanumeric)
                password.Append("!@#$%^&*"[random.Next(8)]);

            // Fill the rest randomly
            for (int i = password.Length; i < length; i++)
            {
                password.Append(validChars[random.Next(validChars.Length)]);
            }

            // Shuffle the password
            var passwordArray = password.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            return new string(passwordArray);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating random password");
            throw;
        }
    }

    private static bool IsCommonPassword(string password)
    {
        // List of common passwords to reject
        var commonPasswords = new[]
        {
            "password", "123456", "123456789", "12345678", "12345", "1234567",
            "password123", "admin", "qwerty", "abc123", "Password1", "welcome",
            "letmein", "monkey", "1234567890", "dragon", "111111", "baseball",
            "iloveyou", "trustno1", "sunshine", "master", "123123", "welcome123"
        };

        return commonPasswords.Contains(password, StringComparer.OrdinalIgnoreCase);
    }
}


using System;
using Isopoh.Cryptography.Argon2;

public static class PasswordHelper
{
    /// <summary>
    /// Hash a password using Argon2id
    /// </summary>
    public static string HashPassword(string password)
    {
        return Argon2.Hash(password);
    }

    /// <summary>
    /// Verify a password against a stored Argon2 hash
    /// </summary>
    public static bool VerifyPassword(string hashedPassword, string plainPassword)
    {
        return Argon2.Verify(hashedPassword, plainPassword);
    }
}

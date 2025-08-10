using System;
using System.Security.Cryptography;
using System.Text;

public static class PasswordHelper
{
    private const int SaltSize = 16; // 128 bit
    private const int HashSize = 20; // 160 bit
    private const int Iterations = 10000; // Secure iteration count

    // Hash password
    public static string HashPassword(string password)
    {
        // Generate a salt
        byte[] salt = new byte[SaltSize];
        using (var cryptoProvider = new RNGCryptoServiceProvider())
        {
            cryptoProvider.GetBytes(salt);
        }

        // Create the hash
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // Combine salt and hash
        byte[] hashBytes = new byte[SaltSize + HashSize];
        Array.Copy(salt, 0, hashBytes, 0, SaltSize);
        Array.Copy(hash, 0, hashBytes, SaltSize, HashSize);

        // Convert to base64
        return Convert.ToBase64String(hashBytes);
    }

     //Verify password
    public static bool VerifyPassword(string password, string storedHash)
    {
        byte[] hashBytes = Convert.FromBase64String(storedHash);

        // Get the salt
        byte[] salt = new byte[SaltSize];
        Array.Copy(hashBytes, 0, salt, 0, SaltSize);

        // Compute the hash on the password the user entered
        var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations);
        byte[] hash = pbkdf2.GetBytes(HashSize);

        // Compare the results
        for (int i = 0; i < HashSize; i++)
        {
            if (hashBytes[i + SaltSize] != hash[i])
                return false;
        }

        return true;
    }


    //public static bool VerifyPassword(string password, string storedHash)
    //{
    //    if (string.IsNullOrEmpty(storedHash))
    //        return false;

    //    byte[] hashBytes;
    //    try
    //    {
    //        hashBytes = Convert.FromBase64String(storedHash);
    //    }
    //    catch (FormatException)
    //    {
    //        // storedHash is not a valid base64 string
    //        return false;
    //    }

    //    if (hashBytes.Length < SaltSize + HashSize)
    //        return false;

    //    byte[] salt = new byte[SaltSize];
    //    Array.Copy(hashBytes, 0, salt, 0, SaltSize);

    //    using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
    //    {
    //        byte[] hash = pbkdf2.GetBytes(HashSize);

    //        for (int i = 0; i < HashSize; i++)
    //        {
    //            if (hashBytes[i + SaltSize] != hash[i])
    //                return false;
    //        }
    //    }

    //    return true;
    //}

}

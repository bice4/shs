using System.Security.Cryptography;
using System.Text;

namespace SecretStoreHys.Api.Services;

/// <summary>
/// Static class that provides encryption and decryption methods.
/// </summary>
internal static class SecretHelper
{
    private static readonly byte[] Iv = [
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16
    ];

    /// <summary>
    /// Encrypts the clear text using the passphrase.
    /// </summary>
    /// <param name="clearText">The text to encrypt.</param>
    /// <param name="passphrase">The passphrase to use for encryption.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A byte array representing the encrypted text.</returns>
    public static async Task<byte[]> EncryptAsync(string clearText, string passphrase, CancellationToken cancellationToken)
    {
        using var aes = Aes.Create();
        aes.Key = DeriveKeyFromPassword(passphrase);
        aes.IV = Iv;
        using MemoryStream output = new();
        await using CryptoStream cryptoStream = new(output, aes.CreateEncryptor(), CryptoStreamMode.Write);
        await cryptoStream.WriteAsync(Encoding.Unicode.GetBytes(clearText), cancellationToken);
        await cryptoStream.FlushFinalBlockAsync(cancellationToken);
        return output.ToArray();
    }

    /// <summary>
    /// Decrypts the encrypted text using the passphrase.
    /// </summary>
    /// <param name="encrypted">The encrypted array of bytes.</param>
    /// <param name="passphrase">The passphrase to use for decryption.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The decrypted text.</returns>
    public static async Task<string> DecryptAsync(byte[] encrypted, string passphrase, CancellationToken cancellationToken)
    {
        using var aes = Aes.Create();
        aes.Key = DeriveKeyFromPassword(passphrase);
        aes.IV = Iv;
        using MemoryStream input = new(encrypted);
        await using CryptoStream cryptoStream = new(input, aes.CreateDecryptor(), CryptoStreamMode.Read);
        using MemoryStream output = new();
        await cryptoStream.CopyToAsync(output, cancellationToken);
        return Encoding.Unicode.GetString(output.ToArray());
    }

    /// <summary>
    /// Derives a key from the password.
    /// </summary>
    /// <param name="password"></param>
    /// <returns></returns>
    private static byte[] DeriveKeyFromPassword(string password)
    {
        var emptySalt = Array.Empty<byte>();
        
        const int iterations = 1000;
        const int desiredKeyLength = 16;
        
        var hashMethod = HashAlgorithmName.SHA384;
        return Rfc2898DeriveBytes.Pbkdf2(Encoding.Unicode.GetBytes(password),
            emptySalt,
            iterations,
            hashMethod,
            desiredKeyLength);
    }
}
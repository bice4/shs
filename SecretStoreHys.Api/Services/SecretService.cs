using System.Text.Json;
using SecretStoreHys.Api.Models;

namespace SecretStoreHys.Api.Services;

/// <summary>
/// Service for managing secrets.
/// </summary>
public sealed class SecretService : ISecretService
{
    private const string SECRET_FOLDER = "Secrets";

    private readonly string _secretsFolder = Path.Combine(Directory.GetCurrentDirectory(), SECRET_FOLDER);

    /// <summary>
    /// Creates a new secret.
    /// </summary>
    /// <param name="content">The content of the secret.</param>
    /// <param name="expirationDate">The expiration date of the secret.</param>
    /// <param name="publicPin">The public pin of the secret.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="SecretModel"/> with the unique identifier of the secret.</returns>
    public async Task<SecretModel> CreateSecretAsync(string content, DateTimeOffset expirationDate,
        string publicPin, CancellationToken cancellationToken)
    {
        // Encrypt the content using the public pin
        var secretContent = await SecretHelper.EncryptAsync(content, publicPin, cancellationToken);
        // Create a new secret model, passing the encrypted content as a base64 string
        var secret = new SecretModel(Convert.ToBase64String(secretContent));

        if (!Directory.Exists(_secretsFolder))
            Directory.CreateDirectory(_secretsFolder);

        var uixTimeMilliseconds = expirationDate.ToUnixTimeMilliseconds();

        // Check if the expiration date is in the past and throw an exception
        if (SecretModel.IsExpired(uixTimeMilliseconds))
            throw new InvalidOperationException("Expiration date is in the past");

        // Create a unique name for the secret file using the secret id and expiration date
        var secretName = $"{secret.Id:N}-{expirationDate.ToUnixTimeMilliseconds()}.secret";

        // Write the secret to a file
        var secretPath = Path.Combine(_secretsFolder, secretName);
        await File.WriteAllTextAsync(secretPath, JsonSerializer.Serialize(secret), cancellationToken);
        return secret;
    }

    /// <summary>
    /// Retrieves a secret.
    /// </summary>
    /// <param name="id">The id of the secret.</param>
    /// <param name="pin">The pin of the secret.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content and description of the secret.</returns>
    public async Task<string> GetSecretAsync(Guid id, string pin,
        CancellationToken cancellationToken)
    {
        // Find the secret file based on the id
        var secretPath = Directory.GetFiles(_secretsFolder, "*.secret")
            .SingleOrDefault(f => f.Contains(id.ToString("N")));

        // Throw an exception if the secret file is not found
        if (!File.Exists(secretPath) || string.IsNullOrWhiteSpace(secretPath))
            throw new FileNotFoundException("Secret not found");

        // Get the expiration date from the secret file name
        var expirationDate = GetSecretIdAndExpDate(secretPath);

        // Check if the secret is expired and delete the secret file, then throw an exception
        if (SecretModel.IsExpired(expirationDate))
        {
            File.Delete(secretPath);
            throw new InvalidOperationException("Secret expired");
        }

        // Read the secret file and deserialize the content
        var secretJson = await File.ReadAllTextAsync(secretPath, cancellationToken);
        var secret = JsonSerializer.Deserialize<SecretModel>(secretJson);

        // If the secret is null, delete the secret file and throw an exception
        if (secret == null)
        {
            File.Delete(secretPath);
            throw new InvalidOperationException("Secret expired");
        }

        // Decrypt the content using the pin 
        var secretContent = Convert.FromBase64String(secret.Content);
        var content = await SecretHelper.DecryptAsync(secretContent, pin, cancellationToken);

        // Delete the secret file after reading the content
        File.Delete(secretPath);

        return content;
    }

    /// <summary>
    /// Cleans up expired secrets.
    /// </summary>
    public void CleanExpiredSecrets()
    {
        if (!Directory.Exists(_secretsFolder))
            return;

        var secretFiles = Directory.GetFiles(_secretsFolder, "*.secret");

        foreach (var secretFile in secretFiles)
        {
            var expirationDate = GetSecretIdAndExpDate(secretFile);

            if (SecretModel.IsExpired(expirationDate))
                File.Delete(secretFile);
        }
    }

    /// <summary>
    /// Retrieves the secret id and expiration date from the secret path.
    /// </summary>
    /// <param name="secretPath">The path of the secret.</param>
    /// <returns>The expiration date of the secret.</returns>
    private static long GetSecretIdAndExpDate(string secretPath)
    {
        var data = Path.GetFileNameWithoutExtension(secretPath).Split('-');
        return Int64.Parse(data[1]);
    }
}
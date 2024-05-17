using SecretStoreHys.Api.Models;

namespace SecretStoreHys.Api.Services;

public interface ISecretService
{
    
    /// <summary>
    /// Creates a new secret.
    /// </summary>
    /// <param name="content">The content of the secret.</param>
    /// <param name="expirationDate">The expiration date of the secret.</param>
    /// <param name="publicPin">The public pin of the secret.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="SecretModel"/> with the unique identifier of the secret.</returns>
    Task<SecretModel> CreateSecretAsync(string content, DateTimeOffset expirationDate,
        string publicPin, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves a secret.
    /// </summary>
    /// <param name="id">The id of the secret.</param>
    /// <param name="pin">The pin of the secret.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The content and description of the secret.</returns>
    Task<string> GetSecretAsync(Guid id, string pin, CancellationToken cancellationToken);

    /// <summary>
    /// Cleans up expired secrets.
    /// </summary>
    void CleanExpiredSecrets();
}
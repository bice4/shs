namespace SecretStoreHys.Api.Models.Requests;

/// <summary>
/// Represents a request to create a new secret.
/// </summary>
public class CreateSecretRequest
{
    /// <summary>
    /// Gets or sets the content of the secret.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Gets or sets the expiration date of the secret.
    /// </summary>
    public DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Gets or sets the public pin of the secret.
    /// </summary>
    public string? PublicPin { get; set; }

    public override string ToString()
        => $"ExpirationDate: {ExpirationDate}, PublicPin: ******, Content: ******";
}
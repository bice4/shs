namespace SecretStoreHys.Api.Models;

/// <summary>
/// Represents a secret model with a unique identifier and content.
/// </summary>
public class SecretModel
{
    /// <summary>
    /// Gets the unique identifier of the secret.
    /// </summary>
    public Guid Id { get; private set; }

    /// <summary>
    /// Gets the content of the secret.
    /// </summary>
    public string Content { get; private set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="SecretModel"/> class.
    /// </summary>
    /// <param name="content">The content of the secret.</param>
    public SecretModel(string content)
    {
        Id = Guid.NewGuid();
        Content = content;
    }

    /// <summary>
    /// Determines whether the secret is expired.
    /// </summary>
    /// <param name="milliseconds">The expiration time in milliseconds.</param>
    /// <returns>
    ///   <c>true</c> if the current time is greater than the expiration time; otherwise, <c>false</c>.
    /// </returns>
    public static bool IsExpired(long milliseconds)
        => DateTimeOffset.Now.ToUnixTimeMilliseconds() > milliseconds;
}
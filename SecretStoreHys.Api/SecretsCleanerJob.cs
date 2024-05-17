using SecretStoreHys.Api.Services;

namespace SecretStoreHys.Api;

/// <summary>
/// Represents a job that cleans up expired secrets.
/// </summary>
public sealed class SecretsCleanerJob : BackgroundService
{
    private readonly ISecretService _secretService;
    private readonly ILogger<SecretsCleanerJob> _logger;
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Represents a job that cleans up expired secrets.
    /// </summary>
    public SecretsCleanerJob(ISecretService secretService,
        ILogger<SecretsCleanerJob> logger,
        IConfiguration configuration)
    {
        _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    private const int DefaultCleanupIntervalSeconds = 40;

    /// <summary>
    /// Executes the job to clean up expired secrets.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token that can stop the job.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(async () =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _secretService.CleanExpiredSecrets();
                    await Task.Delay(GetCleanupInterval(), stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "An error occurred while cleaning secrets");
                }
            }
        }, stoppingToken);
    }

    /// <summary>
    /// Gets the interval for cleaning up secrets.
    /// </summary>
    /// <returns>The interval as a <see cref="TimeSpan"/>.</returns>
    private TimeSpan GetCleanupInterval()
    {
        var interval = _configuration.GetValue<int>("SecretsCleanupIntervalInSeconds");
        return interval > 0 ? TimeSpan.FromSeconds(interval) : TimeSpan.FromSeconds(DefaultCleanupIntervalSeconds);
    }
}
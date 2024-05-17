using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using SecretStoreHys.Api.Models.Requests;
using SecretStoreHys.Api.Services;

namespace SecretStoreHys.Api.Controllers;

/// <summary>
/// Controller for managing secrets.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public sealed class SecretController : ControllerBase
{
    private readonly ISecretService _secretService;
    private readonly ILogger<SecretController> _logger;

    public SecretController(ISecretService secretService, ILogger<SecretController> logger)
    {
        _secretService = secretService ?? throw new ArgumentNullException(nameof(secretService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Creates a new secret.
    /// </summary>
    /// <param name="request"><see cref="CreateSecretRequest"/> secret params.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The unique identifier of the secret.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<IActionResult> CreateSecretAsync([FromBody] CreateSecretRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate the request parameters and return a bad request if any of them are invalid
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest("Content is required");

            if (string.IsNullOrWhiteSpace(request.PublicPin))
                return BadRequest("Public pin is required");

            // Create a new secret
            var secret = await _secretService.CreateSecretAsync(request.Content,
                request.ExpirationDate, request.PublicPin, cancellationToken);

            return Ok(secret.Id.ToString("N"));
        }
        catch (InvalidOperationException ioe)
        {
            _logger.LogWarning(ioe, "An error occurred while creating the secret");
            return BadRequest("Expiration date is in the past");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "An error occurred while creating the secret");
            return StatusCode(500, "An error occurred while creating the secret");
        }
    }

    /// <summary>
    /// Gets the content of a secret.
    /// </summary>
    /// <param name="id">Unique identifier of the secret.</param>
    /// <param name="pin">The pin of the secret.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>The content of the secret.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(string), 200)]
    [ProducesResponseType(typeof(string), 400)]
    [ProducesResponseType(typeof(string), 404)]
    [ProducesResponseType(typeof(string), 500)]
    public async Task<IActionResult> GetSecretAsync(string id, [FromQuery] string pin,
        CancellationToken cancellationToken)
    {
        try
        {
            var secretId = Guid.Parse(id);

            var secretContent = await _secretService.GetSecretAsync(secretId, pin, cancellationToken);
            return Ok(new { secret = secretContent });
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("Secret not found");
            return NotFound("Secret not found");
        }
        catch (InvalidOperationException ioe)
        {
            _logger.LogWarning(ioe, "An error occurred while retrieving the secret");
            return BadRequest("Secret expired");
        }
        catch (FormatException fe)
        {
            _logger.LogWarning(fe, "Invalid secret id");
            return BadRequest("Invalid secret id");
        }
        catch (CryptographicException ce)
        {
            _logger.LogWarning(ce, "An error occurred while decrypting the secret");
            return BadRequest("Invalid pin");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occurred while retrieving the secret");
            return StatusCode(500, "An error occurred while retrieving the secret");
        }
    }
}
using Client.Models;

namespace Client.Services;

public sealed class InputHandlerService
{
    private readonly double _cooldownMilliseconds;
    private readonly ILogger<InputHandlerService> _logger;
    private double _lastAcceptedTimestamp = double.NaN;
    private bool _hasAcceptedInput;

    public InputHandlerService(double cooldownMilliseconds, ILogger<InputHandlerService> logger)
    {
        ArgumentNullException.ThrowIfNull(logger);
        _cooldownMilliseconds = cooldownMilliseconds;
        _logger = logger;
    }

    public event Action<InputType, double>? InputAccepted;

    public ValueTask<bool> HandleInputAsync(InputType inputType, double timestamp, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        bool withinCooldown = _hasAcceptedInput && (timestamp - _lastAcceptedTimestamp) < _cooldownMilliseconds;

        if (withinCooldown)
        {
            _logger.LogDebug("Ignoring input {InputType} at {Timestamp} due to cooldown.", inputType, timestamp);
            return ValueTask.FromResult(false);
        }

        _hasAcceptedInput = true;
        _lastAcceptedTimestamp = timestamp;

        try
        {
            InputAccepted?.Invoke(inputType, timestamp);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Input handler callback failed for {InputType} at {Timestamp}.", inputType, timestamp);
            throw;
        }

        return ValueTask.FromResult(true);
    }
}

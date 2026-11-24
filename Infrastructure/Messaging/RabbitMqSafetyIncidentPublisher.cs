using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using tracking_management_api.Dtos;
using tracking_management_api.Mappers;
using tracking_management_api.Services.SafetyIncidents;

namespace tracking_management_api.Infrastructure.Messaging;

public sealed class RabbitMqSafetyIncidentPublisher : ISafetyIncidentNotifier, IAsyncDisposable
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    private readonly RabbitMqConnectionProvider _connectionProvider;
    private readonly ISafetyIncidentMapper _mapper;
    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqSafetyIncidentPublisher> _logger;
    private readonly SemaphoreSlim _channelGate = new(1, 1);
    private IChannel? _channel;
    private bool _queueDeclared;

    public RabbitMqSafetyIncidentPublisher(
        RabbitMqConnectionProvider connectionProvider,
        ISafetyIncidentMapper mapper,
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqSafetyIncidentPublisher> logger)
    {
        ArgumentNullException.ThrowIfNull(connectionProvider);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _connectionProvider = connectionProvider;
        _mapper = mapper;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<Guid> NotifyAsync(SafetyIncidentDto incident, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(incident);

        var message = _mapper.ToMessage(incident);
        var body = JsonSerializer.SerializeToUtf8Bytes(message, SerializerOptions);

        await _channelGate.WaitAsync(cancellationToken);
        try
        {
            var channel = await EnsureChannelAsync(cancellationToken);
            var properties = new BasicProperties
            {
                Persistent = true,
                ContentType = "application/json",
                MessageId = message.MessageId.ToString(),
                Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds())
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.QueueName,
                mandatory: false,
                basicProperties: properties,
                body: body,
                cancellationToken: cancellationToken);

            _logger.LogInformation(
                "Published safety incident {IncidentId} as message {MessageId} to queue {QueueName}.",
                incident.IncidentId,
                message.MessageId,
                _options.QueueName);

            return message.MessageId;
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            await ResetChannelAsync();
            throw new SafetyIncidentPublishException(
                $"Failed to publish safety incident '{incident.IncidentId}'.",
                exception);
        }
        finally
        {
            _channelGate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await ResetChannelAsync();
        _channelGate.Dispose();
    }

    private async Task<IChannel> EnsureChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        await ResetChannelAsync();

        var connection = await _connectionProvider.GetConnectionAsync(cancellationToken);
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);
        _channel = await connection.CreateChannelAsync(channelOptions, cancellationToken);

        if (!_queueDeclared)
        {
            await _channel.QueueDeclareAsync(
                queue: _options.QueueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null,
                cancellationToken: cancellationToken);
            _queueDeclared = true;
        }

        return _channel;
    }

    private async Task ResetChannelAsync()
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            if (_channel.IsOpen)
            {
                await _channel.CloseAsync();
            }

            await _channel.DisposeAsync();
        }
        catch (Exception exception)
        {
            _logger.LogDebug(exception, "Error while resetting the RabbitMQ channel.");
        }
        finally
        {
            _channel = null;
            _queueDeclared = false;
        }
    }
}

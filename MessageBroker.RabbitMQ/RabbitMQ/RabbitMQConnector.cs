using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MessageBroker.RabbitMQ.RabbitMQ;

internal class RabbitMQConnector : IHostedService, IDisposable
{
    private readonly ILogger<RabbitMQConnector> _logger;
    private readonly RabbitMQClient _client;
    private Timer? _timer = null;

    public RabbitMQConnector(
        ILogger<RabbitMQConnector> logger,
        RabbitMQClient client)
    {
        _logger = logger;
        _client = client;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ connector is running.");

        _timer = new Timer(RefreshConnectionIfNeeded, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(5));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ connector is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }

    private void RefreshConnectionIfNeeded(object? state)
    {
        _client.RefreshConnectionIfNeeded();
        _client.RefreshChannelsIfNeeded();
    }
}
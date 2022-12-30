using System.Text;
using System.Text.Json;
using MediatR;
using MessageBroker.Abstractions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageBroker.RabbitMQ.RabbitMQ;

public class RabbitMQClient : IClient
{
    private readonly ILogger<RabbitMQClient> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMQConfiguration _configuration;
    private readonly Dictionary<(string exchange, string queue, Type expectedType), IModel?> _channels;
    private IConnection? _connection;

    public RabbitMQClient(
        ILogger<RabbitMQClient> logger, 
        RabbitMQConfiguration configuration, 
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _configuration = configuration;

        _channels = new Dictionary<(string exchange, string queue, Type expectedType), IModel?>();

        RefreshConnectionIfNeeded();
    }

    public bool Publish(string message, string exchange)
    {
        if (_connection == null || !_connection.IsOpen)
        {
            _logger.LogInformation("Unable to publish to RabbitMQ, connection is closed (Host:{host} - Port : {port})", _configuration.Host, _configuration.Port);
            return false;
        }

        using var channel = _connection.CreateModel();

        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout);

        var body = Encoding.UTF8.GetBytes(message);
        channel.BasicPublish(exchange: exchange,
            routingKey: "",
            basicProperties: null,
            body: body);

        _logger.LogInformation("Message published to RabbitMQ (Host:{host} - Port : {port})", _configuration.Host, _configuration.Port);
        return true;
    }

    public void Subscribe(string exchange, string queueName, Type expectedType)
    {
        if (_channels.ContainsKey((exchange, queueName, expectedType)))
        {
            _channels.Remove((exchange, queueName, expectedType));
        }

        if (_connection == null || !_connection.IsOpen)
        {
            _logger.LogInformation(
                "Unable to subscribe to RabbitMQ, connection is closed (Host:{host} - Port : {port})",
                _configuration.Host, _configuration.Port);

            _channels.Add((exchange, queueName, expectedType), null);

            return;
        }

        var channel = _connection.CreateModel();

        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Fanout);
        channel.QueueDeclare(queueName, true, false, false);
        channel.QueueBind(queue: queueName,
            exchange: exchange,
            routingKey: "");

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += async (_, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
                
            var @event = JsonSerializer.Deserialize(message, expectedType);
                
            _logger.LogInformation("RabbitMQ Message received : \r\n{message}", message);

            if (@event == null)
            {
                _logger.LogError("Failed to deserialize event from RabbitMQ.\r\n - Expected type : {expectedType}\r\n - Received message : {message}", expectedType.Name, message);
                return; 
            }

            using var scope = _serviceProvider.CreateScope();

            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(@event);
        };

        channel.BasicConsume(queue: queueName,
            autoAck: true,
            consumer: consumer);

        _channels.Add((exchange, queueName, expectedType), channel);
    }

    public void RefreshConnectionIfNeeded()
    {
        if (_connection == null || !_connection.IsOpen)
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration.Host,
                Port = _configuration.Port, 
                UserName = _configuration.Username, 
                Password = _configuration.Password
            };
            try
            {
                _connection = factory.CreateConnection();
                _logger.LogInformation("Connection open with RabbitMQ (Host:{host} - Port : {port})", _configuration.Host, _configuration.Port);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open connection with RabbitMQ (Host:{host} - Port : {port})", _configuration.Host, _configuration.Port);
            }
        }
    }

    public void RefreshChannelsIfNeeded()
    {
        foreach (var channel in _channels)
        {
            if (channel.Value == null || channel.Value.IsClosed)
            {
                Subscribe(channel.Key.exchange, channel.Key.queue, channel.Key.expectedType);
            }
        }
    }
}
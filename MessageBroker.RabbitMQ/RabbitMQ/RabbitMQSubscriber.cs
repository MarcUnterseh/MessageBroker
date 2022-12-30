using MessageBroker.Abstractions;

namespace MessageBroker.RabbitMQ.RabbitMQ;

public class RabbitMQSubscriber : ISubscriber
{
    private readonly RabbitMQClient _client;
    private readonly RabbitMQConfiguration _configuration;
        
    public RabbitMQSubscriber(RabbitMQClient client, RabbitMQConfiguration configuration)
    {
        _client = client;
        _configuration = configuration;
    }

    public void Subscribe<TEvent>() where TEvent : IEvent
    {
        var exchange = MessageBrokersHelper.GetTypeName<TEvent>();
        var queue = $"{_configuration.QueuePrefixe}.{exchange}";
        _client.Subscribe(exchange, queue, typeof(TEvent));
    }

    public void Subscribe(Type type)
    {
        var exchange = MessageBrokersHelper.GetTypeName(type);
        var queue = $"{_configuration.QueuePrefixe}.{exchange}";
        _client.Subscribe(exchange, queue, type);
    }
}
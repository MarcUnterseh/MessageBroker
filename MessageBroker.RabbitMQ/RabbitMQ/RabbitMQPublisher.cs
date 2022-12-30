using MessageBroker.Abstractions;
using Newtonsoft.Json;

namespace MessageBroker.RabbitMQ.RabbitMQ;

public class RabbitMQPublisher : IEventPublisher
{
    private readonly RabbitMQClient _client;

    public RabbitMQPublisher(RabbitMQClient client)
    {
        _client = client;
    }

    public bool Publish<TEvent>(TEvent @event) where TEvent : IEvent
    {
        if (@event == null)
        {
            throw new ArgumentNullException(nameof(@event), "Event can not be null.");
        }

        var typeName = MessageBrokersHelper.GetTypeName<TEvent>();
        var body = JsonConvert.SerializeObject(@event);

        return _client.Publish(body, typeName);
    }

    public bool Publish(string message, string type)
    {
        return _client.Publish(message, type);
    }
}
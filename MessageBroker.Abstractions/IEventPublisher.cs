namespace MessageBroker.Abstractions;

public interface IEventPublisher
{
    bool Publish<TEvent>(TEvent @event) where TEvent : IEvent;
    bool Publish(string message, string type);
}
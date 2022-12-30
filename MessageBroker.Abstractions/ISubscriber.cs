namespace MessageBroker.Abstractions;

public interface ISubscriber
{
    public void Subscribe<TEvent>() where TEvent : IEvent;
    public void Subscribe(Type type);
}
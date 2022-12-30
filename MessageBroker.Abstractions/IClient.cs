namespace MessageBroker.Abstractions;

public interface IClient
{
    bool Publish(string message, string exchange);

    void Subscribe(string exchange, string queueName, Type expectedType);

    void RefreshConnectionIfNeeded();

    void RefreshChannelsIfNeeded();
}
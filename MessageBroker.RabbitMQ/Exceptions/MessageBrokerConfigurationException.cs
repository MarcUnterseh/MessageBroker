namespace MessageBroker.RabbitMQ.Exceptions;

public class MessageBrokerConfigurationException : MessageBrokerException
{
    public MessageBrokerConfigurationException(string? message = null, Exception? innerException = null) : base(message, innerException)
    { }
}
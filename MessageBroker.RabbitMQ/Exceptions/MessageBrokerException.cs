namespace MessageBroker.RabbitMQ.Exceptions;

public class MessageBrokerException : Exception
{
    public MessageBrokerException(string? message = null, Exception? innerException = null) : base(message, innerException)
    { }
}
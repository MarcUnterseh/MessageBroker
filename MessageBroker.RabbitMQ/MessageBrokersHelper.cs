namespace MessageBroker.RabbitMQ;

public static class MessageBrokersHelper
{
    public static string GetTypeName(Type type)
    {
        var name = type.Name;

        return name;
    }

    public static string GetTypeName<T>()
    {
        return GetTypeName(typeof(T));
    }
}
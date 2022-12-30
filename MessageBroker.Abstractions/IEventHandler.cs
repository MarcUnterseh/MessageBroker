using MediatR;

namespace MessageBroker.Abstractions;

public interface IEventHandler<T> : INotificationHandler<T> where T : IEvent
{ }
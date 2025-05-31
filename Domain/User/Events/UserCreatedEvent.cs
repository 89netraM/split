using Mediator;

namespace Split.Domain.User.Events;

public record UserCreatedEvent(UserAggregate User) : INotification;

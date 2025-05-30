using Mediator;

namespace Split.Domain.User.Events;

public record UserRemovedEvent(UserAggregate User) : INotification;

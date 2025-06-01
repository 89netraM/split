using Mediator;

namespace Split.Domain.User.Events;

public record FriendshipCreatedEvent(UserAggregate Initiator, UserAggregate Target) : INotification;

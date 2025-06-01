using Mediator;

namespace Split.Domain.User.Events;

public record FriendshipRemovedEvent(UserAggregate Initiator, UserAggregate Target) : INotification;

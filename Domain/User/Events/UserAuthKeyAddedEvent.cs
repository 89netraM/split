using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record UserAuthKeyAddedEvent(UserAggregate User, AuthKeyId KeyId) : INotification;

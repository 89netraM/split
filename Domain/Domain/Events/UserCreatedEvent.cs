using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record UserCreatedEvent(UserId UserId) : INotification;

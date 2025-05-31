using System;
using System.Collections.Generic;
using Mediator;
using Split.Domain.Primitives;
using Split.Domain.User.Events;

namespace Split.Domain.User;

public class UserAggregate
{
    public UserId Id { get; }
    public string Name { get; }
    public PhoneNumber PhoneNumber { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RemovedAt { get; private set; }

    public IReadOnlyCollection<INotification> DomainEvents => domainEvents;
    private readonly List<INotification> domainEvents = [];

    public UserAggregate(UserId id, string name, PhoneNumber phoneNumber, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        PhoneNumber = phoneNumber;
        CreatedAt = createdAt;
        domainEvents.Add(new UserCreatedEvent(this));
    }

    public void Remove(DateTimeOffset removedAt)
    {
        if (RemovedAt.HasValue)
        {
            return;
        }

        RemovedAt = removedAt;
        domainEvents.Add(new UserRemovedEvent(this));
    }
}

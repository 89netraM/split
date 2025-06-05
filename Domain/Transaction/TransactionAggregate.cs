using System;
using System.Collections.Generic;
using Mediator;
using Split.Domain.Primitives;
using Split.Domain.Transaction.Events;
using Split.Utilities;

namespace Split.Domain.Transaction;

public class TransactionAggregate
{
    public TransactionId Id { get; }
    public string? Description { get; }
    public Money Amount { get; }
    public UserId SenderId { get; }
    public NonEmptyImmutableSet<UserId> RecipientIds { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RemovedAt { get; set; }

    private List<INotification> domainEvents = [];

    public TransactionAggregate(
        string? description,
        Money amount,
        UserId senderId,
        NonEmptyImmutableSet<UserId> recipientIds,
        DateTimeOffset createdAt
    )
    {
        Id = new(Guid.NewGuid());
        Description = description;
        Amount = amount;
        SenderId = senderId;
        RecipientIds = recipientIds;
        CreatedAt = createdAt;

        domainEvents.Add(new TransactionCreatedEvent(this));
    }

    public void Remove(DateTimeOffset removedAt)
    {
        if (RemovedAt.HasValue)
        {
            return;
        }

        RemovedAt = removedAt;
        domainEvents.Add(new TransactionRemovedEvent(this));
    }

    public IReadOnlyCollection<INotification> FlushDomainEvents()
    {
        var events = domainEvents;
        domainEvents = [];
        return events;
    }
}

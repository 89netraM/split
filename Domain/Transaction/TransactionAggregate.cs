using System;
using System.Collections.Generic;
using Mediator;
using Split.Domain.Primitives;
using Split.Domain.Transaction.Events;

namespace Split.Domain.Transaction;

public class TransactionAggregate
{
    public TransactionId Id { get; }
    public string? Description { get; }
    public Money Amount { get; }
    public UserId SenderId { get; }
    public IReadOnlyList<UserId> RecipientIds { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RemovedAt { get; set; }

    private List<INotification> domainEvents = [];

    public TransactionAggregate(
        string? description,
        Money amount,
        UserId senderId,
        IReadOnlyList<UserId> recipientIds,
        DateTimeOffset createdAt
    )
    {
        if (recipientIds is [])
        {
            throw new NoRecipientsException(nameof(recipientIds));
        }

        Id = new(Guid.CreateVersion7(createdAt));
        Description = description;
        Amount = amount;
        SenderId = senderId;
        RecipientIds = recipientIds;
        CreatedAt = createdAt;

        domainEvents.Add(new TransactionCreatedEvent(this));
    }

#nullable disable
    [Obsolete("For EF Core only", error: true)]
    public TransactionAggregate() { }

#nullable restore

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

public class NoRecipientsException(string paramName)
    : ArgumentException("Transaction must have at least one recipient", paramName);

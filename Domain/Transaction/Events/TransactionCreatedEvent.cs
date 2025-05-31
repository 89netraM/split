using Mediator;

namespace Split.Domain.Transaction.Events;

public record TransactionCreatedEvent(TransactionAggregate Transaction) : INotification;

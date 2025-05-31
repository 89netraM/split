using Mediator;

namespace Split.Domain.Transaction.Events;

public record TransactionRemovedEvent(TransactionAggregate Transaction) : INotification;

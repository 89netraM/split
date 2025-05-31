using Mediator;

namespace Split.Domain.Transaction.Events;

public record TransactionCreatedEvent(TransactionEntity Transaction) : INotification;

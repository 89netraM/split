using System;
using Split.Domain.Primitives;
using Split.Utilities;

namespace Split.Domain.Transaction;

public record TransactionEntity(
    TransactionId Id,
    string? Description,
    Money Amount,
    UserId SenderId,
    NonEmptyImmutableSet<UserId> RecipientIds,
    DateTimeOffset CreatedAt
)
{
    public DateTimeOffset? RemovedAt { get; set; }
}

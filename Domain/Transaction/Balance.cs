using Split.Domain.Primitives;

namespace Split.Domain.Transaction;

public record Balance(UserId From, UserId To, Money Amount);

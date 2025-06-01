using System;
using Split.Domain.Primitives;

namespace Split.Domain.User;

public record Friendship(UserId FriendId, DateTimeOffset CreatedAt)
{
    public DateTimeOffset? RemovedAt { get; init; }
}

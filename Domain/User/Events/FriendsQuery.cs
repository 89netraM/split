using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record FriendsQuery(UserId UserId) : IStreamQuery<FriendsQueryResult>;

public record FriendsQueryResult(UserAggregate Friend);

public class FriendsQueryHandler(UserService userService) : IStreamQueryHandler<FriendsQuery, FriendsQueryResult>
{
    public IAsyncEnumerable<FriendsQueryResult> Handle(FriendsQuery query, CancellationToken cancellationToken) =>
        userService.GetFriendsAsync(query.UserId, cancellationToken).Select(friend => new FriendsQueryResult(friend));
}

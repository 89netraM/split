using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record UserRelationshipQuery(UserId UserId) : IStreamQuery<UserRelationshipResult>;

public record UserRelationshipResult(UserAggregate User);

public class UserRelationshipQueryHandler(UserService userService)
    : IStreamQueryHandler<UserRelationshipQuery, UserRelationshipResult>
{
    public IAsyncEnumerable<UserRelationshipResult> Handle(
        UserRelationshipQuery query,
        CancellationToken cancellationToken
    ) => userService.GetRelatedUsersAsync(query.UserId, cancellationToken).Select(u => new UserRelationshipResult(u));
}

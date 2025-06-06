using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record UserQuery(UserId UserId) : IQuery<UserQueryResult>;

public record UserQueryResult(UserAggregate? User);

public class UserQueryHandler(UserService userService) : IQueryHandler<UserQuery, UserQueryResult>
{
    public async ValueTask<UserQueryResult> Handle(UserQuery query, CancellationToken cancellationToken) =>
        new(await userService.GetUserAsync(query.UserId, cancellationToken));
}

using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record AlternateUserQuery(AlternateUserId AlternateUserId) : IQuery<UserQueryResult>;

public record AlternateUserQueryResult(UserAggregate? User);

public class AlternateUserQueryHandler(UserService userService) : IQueryHandler<AlternateUserQuery, UserQueryResult>
{
    public async ValueTask<UserQueryResult> Handle(AlternateUserQuery query, CancellationToken cancellationToken) =>
        new(await userService.GetUserByAlternateIdAsync(query.AlternateUserId, cancellationToken));
}

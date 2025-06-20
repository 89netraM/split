using System.Collections.Generic;
using System.Threading;
using Split.Domain.Primitives;

namespace Split.Domain.User;

public interface IUserRelationshipRepository
{
    IAsyncEnumerable<UserAggregate> GetRelatedUsersAsync(UserId userId, CancellationToken cancellationToken);
}

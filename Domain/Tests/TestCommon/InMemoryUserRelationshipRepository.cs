using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Domain.Tests.TestCommon;

public class InMemoryUserRelationshipRepository(Dictionary<UserId, UserAggregate[]>? users = null)
    : IUserRelationshipRepository
{
    public IAsyncEnumerable<UserAggregate> GetRelatedUsersAsync(UserId userId, CancellationToken cancellationToken) =>
        (users?.GetValueOrDefault(userId) ?? []).ToAsyncEnumerable();
}

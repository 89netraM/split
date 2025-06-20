using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.EntityFrameworkCore;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Infrastructure.Repositories;

public class UserRelationshipRepository(SplitDbContext dbContext) : IUserRelationshipRepository
{
    public IAsyncEnumerable<UserAggregate> GetRelatedUsersAsync(UserId userId, CancellationToken cancellationToken) =>
        dbContext
            .Transactions.AsNoTracking()
            .Where(t => t.SenderId == userId || t.RecipientIds.Contains(userId))
            .OrderByDescending(t => t.CreatedAt)
            .SelectMany(t => t.RecipientIds.Concat(new[] { t.SenderId }))
            .Where(id => id != userId)
            .Distinct()
            .Join(dbContext.Users.AsNoTracking(), id => id, u => u.Id, (id, u) => u)
            .Where(u => u.RemovedAt == null)
            .AsAsyncEnumerable();
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Infrastructure.Repositories;

public class UserRelationshipRepository(SplitDbContext dbContext) : IUserRelationshipRepository
{
    public async IAsyncEnumerable<UserAggregate> GetRelatedUsersAsync(
        UserId userId,
        [EnumeratorCancellation] CancellationToken cancellationToken
    )
    {
        var relatedIds = await dbContext
            .Transactions.AsNoTracking()
            .Where(t => t.SenderId == userId || t.RecipientIds.Contains(userId))
            .OrderByDescending(t => t.CreatedAt)
            .SelectMany(t => t.RecipientIds.Concat(new[] { t.SenderId }))
            .Where(id => id != userId)
            .Distinct()
            .ToArrayAsync(cancellationToken);

        var users = dbContext
            .Users.AsNoTracking()
            .Where(u => relatedIds.Contains(u.Id) && u.RemovedAt == null)
            .AsAsyncEnumerable();

        await foreach (var user in users.WithCancellation(cancellationToken))
        {
            yield return user;
        }
    }
}

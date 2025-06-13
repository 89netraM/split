using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.EntityFrameworkCore;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Infrastructure.Repositories;

public class UserRepository(SplitDbContext dbContext, IMediator mediator) : IUserRepository
{
    public async Task<UserAggregate?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken) =>
        await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

    public async Task<UserAggregate?> GetUserByPhoneNumberAsync(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken
    ) => await dbContext.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, cancellationToken);

    public async Task SaveAsync(UserAggregate user, CancellationToken cancellationToken)
    {
        if (dbContext.Entry(user) is null or { State: EntityState.Detached })
        {
            dbContext.Add(user);
        }
        await dbContext.SaveChangesAsync(cancellationToken);

        await Task.WhenAll(
            user.FlushDomainEvents().Select(domainEvent => mediator.Publish(domainEvent, cancellationToken).AsTask())
        );
    }
}

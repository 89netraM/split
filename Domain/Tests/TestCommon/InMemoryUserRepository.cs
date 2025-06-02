using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Domain.Tests.TestCommon;

public class InMemoryUserRepository(params IEnumerable<UserAggregate> users) : IUserRepository
{
    private readonly Dictionary<UserId, UserAggregate> users = users.ToDictionary(user => user.Id);

    public Task<UserAggregate?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult(users.TryGetValue(userId, out var user) ? user : null);

    public Task<bool> IsPhoneNumberInUse(PhoneNumber phoneNumber, CancellationToken cancellationToken) =>
        Task.FromResult(users.Values.Any(user => user.PhoneNumber == phoneNumber));

    public Task SaveAsync(UserAggregate user, CancellationToken cancellationToken)
    {
        users[user.Id] = user;
        return Task.CompletedTask;
    }
}

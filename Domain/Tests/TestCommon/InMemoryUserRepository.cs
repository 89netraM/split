using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Split.Domain.Primitives;
using Split.Domain.User;

namespace Split.Domain.Tests.TestCommon;

public class InMemoryUserRepository(params IEnumerable<UserAggregate> users) : IUserRepository
{
    public Dictionary<UserId, UserAggregate> Users { get; } = users.ToDictionary(user => user.Id);

    public Task<UserAggregate?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken) =>
        Task.FromResult(Users.TryGetValue(userId, out var user) ? user : null);

    public Task<UserAggregate?> GetUserByPhoneNumberAsync(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken
    ) => Task.FromResult(Users.Values.FirstOrDefault(user => user.PhoneNumber == phoneNumber));

    public Task<bool> DoesAuthKeyIdExist(AuthKeyId authKeyId, CancellationToken cancellationToken) =>
        Task.FromResult(Users.Values.Any(u => u.AuthKeys.Any(k => k.Id == authKeyId)));

    public Task SaveAsync(UserAggregate user, CancellationToken cancellationToken)
    {
        Users[user.Id] = user;
        _ = user.FlushDomainEvents();
        return Task.CompletedTask;
    }
}

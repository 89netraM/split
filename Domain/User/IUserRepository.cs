using System.Threading;
using System.Threading.Tasks;
using Split.Domain.Primitives;

namespace Split.Domain.User;

public interface IUserRepository
{
    Task<UserAggregate?> GetUserByIdAsync(UserId userId, CancellationToken cancellationToken);
    Task<bool> IsPhoneNumberInUse(PhoneNumber phoneNumber, CancellationToken cancellationToken);
    Task SaveAsync(UserAggregate user, CancellationToken cancellationToken);
}

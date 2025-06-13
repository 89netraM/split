using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record PhoneNumberQuery(PhoneNumber PhoneNumber) : IQuery<PhoneNumberQueryResult>;

public record PhoneNumberQueryResult(UserAggregate? User);

public class PhoneNumberQueryHandler(UserService userService) : IQueryHandler<PhoneNumberQuery, PhoneNumberQueryResult>
{
    public async ValueTask<PhoneNumberQueryResult> Handle(
        PhoneNumberQuery query,
        CancellationToken cancellationToken
    ) => new(await userService.GetUserByPhoneNumberAsync(query.PhoneNumber, cancellationToken));
}

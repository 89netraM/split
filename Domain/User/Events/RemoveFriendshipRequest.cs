using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record RemoveFriendshipRequest(UserId InitiatorId, UserId TargetId) : IRequest<RemoveFriendshipResponse>;

public record RemoveFriendshipResponse();

public class RemoveFriendshipRequestHandler(UserService userService)
    : IRequestHandler<RemoveFriendshipRequest, RemoveFriendshipResponse>
{
    public async ValueTask<RemoveFriendshipResponse> Handle(
        RemoveFriendshipRequest request,
        CancellationToken cancellationToken
    )
    {
        await userService.RemoveFriendshipAsync(request.InitiatorId, request.TargetId, cancellationToken);

        return new RemoveFriendshipResponse();
    }
}

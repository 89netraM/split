using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record CreateFriendshipRequest(UserId InitiatorId, UserId TargetId) : IRequest<CreateFriendshipResponse>;

public record CreateFriendshipResponse();

public class CreateFriendshipRequestHandler(UserService userService)
    : IRequestHandler<CreateFriendshipRequest, CreateFriendshipResponse>
{
    public async ValueTask<CreateFriendshipResponse> Handle(
        CreateFriendshipRequest request,
        CancellationToken cancellationToken
    )
    {
        await userService.CreateFriendshipAsync(request.InitiatorId, request.TargetId, cancellationToken);

        return new CreateFriendshipResponse();
    }
}

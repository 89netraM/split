using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record RemoveUserRequest(UserId UserId) : IRequest<RemoveUserResponse>;

public record RemoveUserResponse();

public class RemoveUserRequestHandler(UserService userService) : IRequestHandler<RemoveUserRequest, RemoveUserResponse>
{
    public async ValueTask<RemoveUserResponse> Handle(RemoveUserRequest request, CancellationToken cancellationToken)
    {
        await userService.RemoveUserAsync(request.UserId, cancellationToken);

        return new RemoveUserResponse();
    }
}

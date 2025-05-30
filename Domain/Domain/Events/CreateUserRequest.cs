using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public sealed record CreateUserRequest(string Name, PhoneNumber PhoneNumber) : IRequest<CreateUserResponse>;

public sealed record CreateUserResponse(UserId UserId);

public class CreateUserRequestHandler(UserService userService) : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public async ValueTask<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var user = await userService.CreateUserAsync(request.Name, request.PhoneNumber, cancellationToken);

        return new CreateUserResponse(user.Id);
    }
}

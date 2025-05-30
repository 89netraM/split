using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public sealed record CreateUserRequest(string Name, PhoneNumber PhoneNumber) : IRequest<CreateUserResponse>;

public sealed record CreateUserResponse(UserId UserId);

public class CreateUserRequestHandler(ILogger<CreateUserRequestHandler> logger, UserService userService)
    : IRequestHandler<CreateUserRequest, CreateUserResponse>
{
    public async ValueTask<CreateUserResponse> Handle(CreateUserRequest request, CancellationToken cancellationToken)
    {
        logger.LogDebug("Handling CreateUserRequest for phone number: {PhoneNumber}", request.PhoneNumber);

        var user = await userService.CreateUserAsync(request.Name, request.PhoneNumber, cancellationToken);

        logger.LogDebug("User created with ID: {UserId}", user.Id);

        return new CreateUserResponse(user.Id);
    }
}

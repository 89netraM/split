using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;

namespace Split.Domain.User.Events;

public sealed record AssertFido2AssertionRequest(
    AuthenticatorAssertionRawResponse AssertionResponse,
    string AssertionContext
) : IRequest<AssertFido2AssertionResponse>;

public sealed record AssertFido2AssertionResponse();

public sealed class AssertFido2AssertionRequestHandler(AuthService authService)
    : IRequestHandler<AssertFido2AssertionRequest, AssertFido2AssertionResponse>
{
    public async ValueTask<AssertFido2AssertionResponse> Handle(
        AssertFido2AssertionRequest request,
        CancellationToken cancellationToken
    )
    {
        await authService.AssertAssertion(request.AssertionResponse, request.AssertionContext, cancellationToken);
        return new();
    }
}

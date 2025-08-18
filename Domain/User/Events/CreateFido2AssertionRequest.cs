using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public sealed record CreateFido2AssertionRequest(PhoneNumber PhoneNumber) : IRequest<CreateFido2AssertionResponse>;

public sealed record CreateFido2AssertionResponse(AssertionOptions Assertion, string AssertionContext);

public sealed class CreateFido2AssertionRequestHandler(AuthService authService)
    : IRequestHandler<CreateFido2AssertionRequest, CreateFido2AssertionResponse>
{
    public async ValueTask<CreateFido2AssertionResponse> Handle(
        CreateFido2AssertionRequest request,
        CancellationToken cancellationToken
    )
    {
        var assertion = await authService.CreateAssertion(request.PhoneNumber, cancellationToken);
        return new(assertion.Assertion, assertion.AssertionContext);
    }
}

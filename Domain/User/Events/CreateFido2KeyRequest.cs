using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Mediator;

namespace Split.Domain.User.Events;

public sealed record CreateFido2KeyRequest(AuthenticatorAttestationRawResponse Attestation, string ChallengeContext)
    : IRequest<CreateFido2KeyResponse>;

public sealed record CreateFido2KeyResponse();

public record CreateFido2KeyRequestHandler(AuthService authService)
    : IRequestHandler<CreateFido2KeyRequest, CreateFido2KeyResponse>
{
    public async ValueTask<CreateFido2KeyResponse> Handle(
        CreateFido2KeyRequest request,
        CancellationToken cancellationToken
    )
    {
        await authService.CreateAuthKey(request.Attestation, request.ChallengeContext, cancellationToken);
        return new();
    }
}

using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.User.Events;
using Split.Infrastructure.PhoneNumberVerifier.Services;

namespace Split.Infrastructure.PhoneNumberVerifier.Handlers;

public sealed class PhoneNumberVerificationRequestHandler(PhoneNumberVerificationService phoneNumberVerificationService)
    : IRequestHandler<PhoneNumberVerificationRequest, PhoneNumberVerificationResponse>
{
    public async ValueTask<PhoneNumberVerificationResponse> Handle(
        PhoneNumberVerificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var context = await phoneNumberVerificationService.CreateVerification(request.PhoneNumber, cancellationToken);
        return new(context.Context);
    }
}

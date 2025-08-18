using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Split.Domain.User.Events;
using Split.Infrastructure.PhoneNumberVerifier.Services;

namespace Split.Infrastructure.PhoneNumberVerifier.Handlers;

public sealed class VerifyPhoneNumberRequestHandler(PhoneNumberVerificationService phoneNumberVerificationService)
    : IRequestHandler<VerifyPhoneNumberRequest, VerifyPhoneNumberResponse>
{
    public ValueTask<VerifyPhoneNumberResponse> Handle(
        VerifyPhoneNumberRequest request,
        CancellationToken cancellationToken
    )
    {
        var result = phoneNumberVerificationService.VerifyPhoneNumber(
            new(request.Code),
            new(request.VerificationContext)
        );
        return ValueTask.FromResult(new VerifyPhoneNumberResponse(result.PhoneNumber));
    }
}

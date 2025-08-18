using System.Diagnostics.CodeAnalysis;
using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record VerifyPhoneNumberRequest(string Code, string VerificationContext) : IRequest<VerifyPhoneNumberResponse>;

public record VerifyPhoneNumberResponse(PhoneNumber? PhoneNumber)
{
    [MemberNotNullWhen(true, nameof(PhoneNumber))]
    public bool Success => PhoneNumber is not null;
}

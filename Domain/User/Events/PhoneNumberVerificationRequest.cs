using Mediator;
using Split.Domain.Primitives;

namespace Split.Domain.User.Events;

public record PhoneNumberVerificationRequest(PhoneNumber PhoneNumber) : IRequest<PhoneNumberVerificationResponse>;

public record PhoneNumberVerificationResponse(string VerificationContext);

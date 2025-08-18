using Split.Domain.Primitives;

namespace Split.Infrastructure.PhoneNumberVerifier.Services;

internal sealed record PhoneNumberVerificationResult(PhoneNumber? PhoneNumber);

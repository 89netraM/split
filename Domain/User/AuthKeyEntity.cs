using System;
using Split.Domain.Primitives;

namespace Split.Domain.User;

public class AuthKeyEntity(AuthKeyId id, byte[] key, uint signCount)
{
    public AuthKeyId Id { get; } = id;
    public byte[] Key { get; } = key;
    public uint SignCount { get; private set; } = signCount;

    public void IncreaseSignCount(uint signCount)
    {
        if (SignCount >= signCount)
        {
            throw new SignCountIncreaseException(SignCount, signCount);
        }
        SignCount = signCount;
    }
}

internal class SignCountIncreaseException(uint current, uint next)
    : Exception($"Signing key can only increase count, attempted from {current} to {next}.");

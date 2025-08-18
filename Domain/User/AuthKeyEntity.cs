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
        if (SignCount + 1 != signCount)
        {
            throw new SignCountIncreaseException(SignCount, signCount);
        }
        SignCount = signCount;
    }
}

internal class SignCountIncreaseException(uint current, uint next)
    : Exception($"Only sequential sign key increases are possible, attempted from {current} to {next}.");

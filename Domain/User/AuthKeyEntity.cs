using Split.Domain.Primitives;

namespace Split.Domain.User;

public record AuthKeyEntity(AuthKeyId Id, byte[] Key, uint SignCount);

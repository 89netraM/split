using System;
using Split.Domain.Primitives;

namespace Split.Domain.User;

public class UserAggregate(string name, PhoneNumber phoneNumber, DateTimeOffset createdAt)
{
    public UserId Id { get; } = new UserId(Guid.NewGuid());
    public string Name { get; } = name;
    public PhoneNumber PhoneNumber { get; } = phoneNumber;
    public DateTimeOffset CreatedAt { get; } = createdAt;
}

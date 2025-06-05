using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Mediator;
using Split.Domain.Primitives;
using Split.Domain.User.Events;

namespace Split.Domain.User;

public class UserAggregate
{
    public UserId Id { get; }
    public string Name { get; }
    public PhoneNumber PhoneNumber { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? RemovedAt { get; private set; }

    public IEnumerable<Friendship> Friendships => friendships.Where(f => !f.RemovedAt.HasValue);
    private readonly List<Friendship> friendships = [];

    private List<INotification> domainEvents = [];

    public UserAggregate(UserId id, string name, PhoneNumber phoneNumber, DateTimeOffset createdAt)
    {
        Id = id;
        Name = name;
        PhoneNumber = phoneNumber;
        CreatedAt = createdAt;
        domainEvents.Add(new UserCreatedEvent(this));
    }

    public void Remove(DateTimeOffset removedAt)
    {
        if (RemovedAt.HasValue)
        {
            return;
        }

        RemovedAt = removedAt;
        domainEvents.Add(new UserRemovedEvent(this));
    }

    public void CreateFriendship(UserAggregate friend, DateTimeOffset createdAt)
    {
        if (Friendships.Any(f => f.FriendId == friend.Id))
        {
            return;
        }

        if (friend.Id == Id)
        {
            throw new AutoFriendshipException();
        }

        friendships.Add(new(friend.Id, createdAt));
        friend.friendships.Add(new(Id, createdAt));

        domainEvents.Add(new FriendshipCreatedEvent(this, friend));
    }

    public void RemoveFriendship(UserAggregate friend, DateTimeOffset removedAt)
    {
        var friendship = Friendships.FirstOrDefault(f => f.FriendId == friend.Id);
        if (friendship is null)
        {
            return;
        }

        var friendshipIndex = friendships.IndexOf(friendship);
        friendships[friendshipIndex] = friendship with { RemovedAt = removedAt };

        var friendsFriendship = friend.Friendships.First(f => f.FriendId == Id);
        var friendsFriendshipIndex = friend.friendships.IndexOf(friendsFriendship);
        friend.friendships[friendsFriendshipIndex] = friend.friendships[friendsFriendshipIndex] with
        {
            RemovedAt = removedAt,
        };

        domainEvents.Add(new FriendshipRemovedEvent(this, friend));
    }

    public IReadOnlyCollection<INotification> FlushDomainEvents()
    {
        var events = domainEvents;
        domainEvents = [];
        return events;
    }
}

public class AutoFriendshipException() : Exception("Cannot create a friendship with oneself");

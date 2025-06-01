using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;

namespace Split.Domain.User;

public class UserService(ILogger<UserService> logger, TimeProvider timeProvider, IUserRepository userRepository)
{
    public async Task<UserAggregate> CreateUserAsync(
        UserId userId,
        string name,
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken
    )
    {
        logger.LogDebug("Creating user with id {UserId}", userId);

        var existingUser = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (existingUser is not null)
        {
            if (existingUser.Name == name && existingUser.PhoneNumber == phoneNumber)
            {
                logger.LogInformation("User with id {UserId} already exists with the same properties", userId);
                return existingUser;
            }
            else
            {
                throw new UserAlreadyExistsException(userId);
            }
        }

        if (await userRepository.IsPhoneNumberInUse(phoneNumber, cancellationToken))
        {
            throw new PhoneNumberInUseException(phoneNumber);
        }

        var newUser = new UserAggregate(userId, name, phoneNumber, timeProvider.GetUtcNow());
        await userRepository.SaveAsync(newUser, cancellationToken);

        logger.LogDebug("Successfully created new user with ID: {UserId}", newUser.Id);
        return newUser;
    }

    public async Task RemoveUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Removing user with ID: {UserId}", userId);

        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            logger.LogDebug("Cannot remove user ({UserId}) that does not exist", userId);
            return;
        }

        user.Remove(timeProvider.GetUtcNow());
        await userRepository.SaveAsync(user, cancellationToken);

        logger.LogDebug("Successfully removed user with ID: {UserId}", user.Id);
    }

    public async Task CreateFriendshipAsync(UserId initiatorId, UserId targetId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Creating friendship between {InitiatorId} and {TargetId}", initiatorId, targetId);

        var initiator =
            await userRepository.GetUserByIdAsync(initiatorId, cancellationToken)
            ?? throw new UserNotFoundException(initiatorId);
        var target =
            await userRepository.GetUserByIdAsync(targetId, cancellationToken)
            ?? throw new UserNotFoundException(targetId);

        target.CreateFriendship(initiator, timeProvider.GetUtcNow());

        await userRepository.SaveAsync(initiator, cancellationToken);
        await userRepository.SaveAsync(target, cancellationToken);

        logger.LogDebug(
            "Successfully created friendship between {InitiatorId} and {TargetId}",
            initiator.Id,
            target.Id
        );
    }
}

public class UserAlreadyExistsException(UserId userId) : Exception($"A user with this id {userId} already exists");

public class PhoneNumberInUseException(PhoneNumber phoneNumber)
    : Exception($"A user with this phone number {phoneNumber} already exists");

public class UserNotFoundException(UserId userId) : Exception($"A user with this id {userId} does not exist");

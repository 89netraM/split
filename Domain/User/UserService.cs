using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;

namespace Split.Domain.User;

public class UserService(
    ILogger<UserService> logger,
    TimeProvider timeProvider,
    IUserRepository userRepository,
    IUserRelationshipRepository userRelationshipRepository
)
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

        var existingPhoneUser = await userRepository.GetUserByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (existingPhoneUser is { RemovedAt: null })
        {
            throw new PhoneNumberInUseException(phoneNumber);
        }

        var newUser = new UserAggregate(userId, name, phoneNumber, timeProvider.GetUtcNow());
        await userRepository.SaveAsync(newUser, cancellationToken);

        logger.LogDebug("Successfully created new user with ID: {UserId}", newUser.Id);
        return newUser;
    }

    public async Task<UserAggregate?> GetUserAsync(UserId userId, CancellationToken cancellationToken)
    {
        logger.LogDebug("Retrieving user with ID: {UserId}", userId);

        var user = await userRepository.GetUserByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            logger.LogDebug("User with ID: {UserId} does not exist", userId);
            return null;
        }
        if (user.RemovedAt.HasValue)
        {
            logger.LogDebug("User with ID: {UserId} has been removed", userId);
            return null;
        }

        logger.LogDebug("Successfully retrieved user with ID: {UserId}", user.Id);
        return user;
    }

    public async Task<UserAggregate?> GetUserByPhoneNumberAsync(
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken
    )
    {
        logger.LogDebug("Retrieving user with phone number: {PhoneNumber}", phoneNumber);

        var user = await userRepository.GetUserByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (user is null)
        {
            logger.LogDebug("User with phone number: {PhoneNumber} does not exist", phoneNumber);
            return null;
        }
        if (user.RemovedAt.HasValue)
        {
            logger.LogDebug("User with phone number: {PhoneNumber} has been removed", phoneNumber);
            return null;
        }

        logger.LogDebug("Successfully retrieved user with phone number: {PhoneNumber}", phoneNumber);
        return user;
    }

    public IAsyncEnumerable<UserAggregate> GetRelatedUsersAsync(UserId userId, CancellationToken cancellationToken) =>
        userRelationshipRepository.GetRelatedUsersAsync(userId, cancellationToken);

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
}

public class UserAlreadyExistsException(UserId userId) : Exception($"A user with this id {userId} already exists");

public class PhoneNumberInUseException(PhoneNumber phoneNumber)
    : Exception($"A user with this phone number {phoneNumber} already exists");

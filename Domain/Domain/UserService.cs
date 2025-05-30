using System;
using System.Threading;
using System.Threading.Tasks;
using Mediator;
using Microsoft.Extensions.Logging;
using Split.Domain.Primitives;
using Split.Domain.User.Events;

namespace Split.Domain.User;

public class UserService(
    ILogger<UserService> logger,
    IMediator mediator,
    TimeProvider timeProvider,
    IUserRepository userRepository
)
{
    public async Task<UserAggregate> CreateUserAsync(
        string name,
        PhoneNumber phoneNumber,
        CancellationToken cancellationToken
    )
    {
        logger.LogDebug("Creating user with name: {Name} and phone number: {PhoneNumber}", name, phoneNumber);

        var existingUser = await userRepository.GetUserByPhoneNumberAsync(phoneNumber, cancellationToken);
        if (existingUser is not null)
        {
            if (existingUser.Name == name)
            {
                logger.LogInformation(
                    "User with phone number {PhoneNumber} already exists with the same name",
                    phoneNumber
                );
                return existingUser;
            }
            else
            {
                throw new PhoneNumberAlreadyExistsException(phoneNumber);
            }
        }

        var newUser = new UserAggregate(name, phoneNumber, timeProvider.GetUtcNow());
        await userRepository.SaveAsync(newUser, cancellationToken);
        await mediator.Publish(new UserCreatedEvent(newUser.Id), cancellationToken);

        logger.LogDebug("Successfully created new user with ID: {UserId}", newUser.Id);
        return newUser;
    }
}

public class PhoneNumberAlreadyExistsException(PhoneNumber phoneNumber)
    : Exception($"A user with this phone number {phoneNumber} already exists");

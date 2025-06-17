using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Split.Domain.Tests.TestCommon;
using Split.Domain.Transaction;
using Split.Domain.Transaction.Events;

namespace Split.Domain.Tests.Transaction.Events.RemoveTransactionRequestTests;

[TestClass]
public class HandleShould
{
    [TestMethod]
    public async Task SuccessfullyRemoveATransaction()
    {
        // Arrange
        var handler = new RemoveTransactionRequestHandler(
            new TransactionService(
                new NullLogger<TransactionService>(),
                new FakeTimeProvider(),
                new InMemoryTransactionRepository(),
                new InMemoryUserRepository()
            )
        );
        var request = new RemoveTransactionRequest(new(new("01977ea7-1757-7606-aef1-0ec2880255ed")));

        // Act & Assert
        await handler.Handle(request, CancellationToken.None);
    }
}

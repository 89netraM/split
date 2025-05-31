using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Time.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
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
                Substitute.For<ITransactionRepository>()
            )
        );
        var request = new RemoveTransactionRequest(new(new("33a1cb86-a2cd-4853-ae65-2cecdd099f2d")));

        // Act & Assert
        await handler.Handle(request, CancellationToken.None);
    }
}

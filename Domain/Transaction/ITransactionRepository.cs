using System.Threading;
using System.Threading.Tasks;

namespace Split.Domain.Transaction;

public interface ITransactionRepository
{
    Task SaveAsync(TransactionEntity transaction, CancellationToken cancellationToken);
}

using AccountingCLI.Data;
using AccountingCLI.Models;

namespace AccountingCLI.Services;

public interface ITransactionService
{
    public void AddTransaction(Transaction transaction);

    public IEnumerable<Transaction> GetTransactions();

    public IEnumerable<Transaction> GetTransactions(DateTime start, DateTime end);

    public IEnumerable<TrialBalance> GetTrialBalanceItems();
    
    public void GenerateSeedData();
}
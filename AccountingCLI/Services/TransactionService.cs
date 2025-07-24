using AccountingCLI.Data;
using AccountingCLI.Models;

namespace AccountingCLI.Services;

public class TransactionService : ITransactionService
{
    public ITransactionDbContext DbContext { get; }

    public TransactionService(ITransactionDbContext dbContext)
    {
        DbContext = dbContext;
    }

    public void AddTransaction(Transaction transaction)
    {
        var transactions = DbContext.Load();
        
        // Generate unique ID
        transaction.Id = transactions.Count > 0 ? transactions.Max(t => t.Id) + 1 : 1;
        
        transactions.Add(transaction);
        DbContext.Save(transactions);
    }

    public IEnumerable<Transaction> GetTransactions()
    {
        return DbContext.Load();
    }

    public IEnumerable<Transaction> GetTransactions(DateTime start, DateTime end)
    {
        var transactions = DbContext.Load();
        return transactions
        .Where(t => t.Date >= start && t.Date <= end)
        .OrderBy(t => t.Date)
        .ToList();
    }

    public IEnumerable<TrialBalance> GetTrialBalanceItems()
    {
        var transactions = DbContext.Load();
        return transactions.SelectMany(t => new[] {
            new { Account = t.DebitAccountNumber, Debit = t.Amount, Credit = 0m },
            new { Account = t.CreditAccountNumber, Debit = 0m, Credit = t.Amount }
        })
        .GroupBy(x => x.Account)
        .Select(g => new TrialBalance
        {
            AccountNumber = g.Key,
            DebitTotal = g.Sum(x => x.Debit),
            CreditTotal = g.Sum(x => x.Credit),
            Balance = g.Sum(x => x.Debit - x.Credit)
        });
    }

    public void GenerateSeedData()
    {
        var existingTransactions = DbContext.Load();
        if (existingTransactions.Count > 0)
        {
            Console.WriteLine("Seed data already exists. Skipping generation.");
            return;
        }

        var seedTransactions = new List<Transaction>
        {
            new() { DebitAccountNumber = 101, CreditAccountNumber = 301, Amount = 50000m, Date = DateTime.Parse("2024-01-15"), Description = "Початковий капітал" },
            new() { DebitAccountNumber = 201, CreditAccountNumber = 101, Amount = 25000m, Date = DateTime.Parse("2024-01-20"), Description = "Покупка обладнання" },
            new() { DebitAccountNumber = 102, CreditAccountNumber = 701, Amount = 15000m, Date = DateTime.Parse("2024-02-01"), Description = "Продаж товарів" },
            new() { DebitAccountNumber = 631, CreditAccountNumber = 102, Amount = 8000m, Date = DateTime.Parse("2024-02-05"), Description = "Витрати на рекламу" },
            new() { DebitAccountNumber = 311, CreditAccountNumber = 102, Amount = 12000m, Date = DateTime.Parse("2024-02-10"), Description = "Покупка матеріалів" },
            new() { DebitAccountNumber = 102, CreditAccountNumber = 701, Amount = 22000m, Date = DateTime.Parse("2024-02-15"), Description = "Продаж послуг" },
            new() { DebitAccountNumber = 661, CreditAccountNumber = 102, Amount = 3500m, Date = DateTime.Parse("2024-02-20"), Description = "Банківські комісії" },
            new() { DebitAccountNumber = 102, CreditAccountNumber = 375, Amount = 5000m, Date = DateTime.Parse("2024-02-25"), Description = "Податок на прибуток" },
            new() { DebitAccountNumber = 685, CreditAccountNumber = 102, Amount = 2800m, Date = DateTime.Parse("2024-03-01"), Description = "Витрати на зв'язок" },
            new() { DebitAccountNumber = 102, CreditAccountNumber = 701, Amount = 18500m, Date = DateTime.Parse("2024-03-05"), Description = "Продаж товарів" }
        };

        foreach (var transaction in seedTransactions)
        {
            AddTransaction(transaction);
        }

        Console.WriteLine($"Generated {seedTransactions.Count} seed transactions.");
    }

}
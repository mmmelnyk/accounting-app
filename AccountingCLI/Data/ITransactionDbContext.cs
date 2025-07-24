using System.Text.Json;
using AccountingCLI.Models;

namespace AccountingCLI.Data;

public interface ITransactionDbContext
{
    public List<Transaction> Load();

    public void Save(List<Transaction> transactions);
}
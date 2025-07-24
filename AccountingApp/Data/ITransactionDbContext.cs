using System.Text.Json;
using AccountingApp.Models;

namespace AccountingApp.Data;

public interface ITransactionDbContext
{
    public List<Transaction> Load();

    public void Save(List<Transaction> transactions);
}
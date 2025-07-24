using System.Text.Json;
using AccountingApp.Models;

namespace AccountingApp.Data;

public class TransactionDbContext : ITransactionDbContext
{
    private readonly string _filePath;

    public TransactionDbContext() : this("transactions.json") { }
    
    public TransactionDbContext(string filePath)
    {
        _filePath = filePath;
    }

    public List<Transaction> Load()
    {
        if (!File.Exists(_filePath)) return new List<Transaction>();

        var json = File.ReadAllText(_filePath);
        return JsonSerializer.Deserialize<List<Transaction>>(json) ?? new();
    }

    public void Save(List<Transaction> transactions)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        
        var json = JsonSerializer.Serialize(transactions, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
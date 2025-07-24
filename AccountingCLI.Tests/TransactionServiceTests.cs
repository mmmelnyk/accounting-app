using AccountingCLI.Models;
using AccountingCLI.Services;
using AccountingCLI.Data;
using Moq;

namespace AccountingApp.Tests;

public class TransactionServiceTests
{
    private Mock<ITransactionDbContext> _mockDbContext;
    private TransactionService _service;
    private List<Transaction> _testTransactions;

    [SetUp]
    public void Setup()
    {
        _mockDbContext = new Mock<ITransactionDbContext>();
        _service = new TransactionService(_mockDbContext.Object);
        
        _testTransactions = new List<Transaction>
        {
            new() { Id = 1, DebitAccountNumber = "101", CreditAccountNumber = "301", 
                   Amount = 50000m, Date = DateTime.Parse("2024-01-01"), Description = "Initial capital" },
            new() { Id = 2, DebitAccountNumber = "201", CreditAccountNumber = "101", 
                   Amount = 25000m, Date = DateTime.Parse("2024-01-15"), Description = "Equipment purchase" },
            new() { Id = 3, DebitAccountNumber = "102", CreditAccountNumber = "701", 
                   Amount = 15000m, Date = DateTime.Parse("2024-02-01"), Description = "Sales revenue" }
        };
    }

    [Test]
    public void AddTransaction_EmptyList_AddsFirstTransactionWithId1()
    {
        // Arrange
        var emptyList = new List<Transaction>();
        _mockDbContext.Setup(x => x.Load()).Returns(emptyList);
        
        var newTransaction = new Transaction
        {
            DebitAccountNumber = "101",
            CreditAccountNumber = "301",
            Amount = 1000m,
            Date = DateTime.Now,
            Description = "Test transaction"
        };

        // Act
        _service.AddTransaction(newTransaction);

        // Assert
        Assert.That(newTransaction.Id, Is.EqualTo(1));
        _mockDbContext.Verify(x => x.Save(It.Is<List<Transaction>>(list => 
            list.Count == 1 && list[0].Id == 1)), Times.Once);
    }

    [Test]
    public void AddTransaction_ExistingTransactions_AssignsNextId()
    {
        // Arrange
        _mockDbContext.Setup(x => x.Load()).Returns(_testTransactions);
        
        var newTransaction = new Transaction
        {
            DebitAccountNumber = "102",
            CreditAccountNumber = "701",
            Amount = 2000m,
            Date = DateTime.Now,
            Description = "New transaction"
        };

        // Act
        _service.AddTransaction(newTransaction);

        // Assert
        Assert.That(newTransaction.Id, Is.EqualTo(4)); // Next ID after 3
        _mockDbContext.Verify(x => x.Save(It.Is<List<Transaction>>(list => 
            list.Count == 4 && list.Any(t => t.Id == 4))), Times.Once);
    }

    [Test]
    public void GetTransactions_ReturnsAllTransactions()
    {
        // Arrange
        _mockDbContext.Setup(x => x.Load()).Returns(_testTransactions);

        // Act
        var result = _service.GetTransactions().ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(3));
        Assert.That(result, Is.EqualTo(_testTransactions));
    }

    [Test]
    public void GetTransactions_EmptyDatabase_ReturnsEmptyCollection()
    {
        // Arrange
        _mockDbContext.Setup(x => x.Load()).Returns(new List<Transaction>());

        // Act
        var result = _service.GetTransactions().ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetTransactions_ValidDateRange_ReturnsFilteredResults()
    {
        // Arrange
        _mockDbContext.Setup(x => x.Load()).Returns(_testTransactions);
        var startDate = DateTime.Parse("2024-01-01");
        var endDate = DateTime.Parse("2024-01-31");

        // Act
        var result = _service.GetTransactions(startDate, endDate).ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2)); // Should return first 2 transactions
        Assert.That(result.All(t => t.Date >= startDate && t.Date <= endDate), Is.True);
    }

    [Test]
    public void GetTransactions_StartDateAfterEndDate_ReturnsEmpty()
    {
        // Arrange
        _mockDbContext.Setup(x => x.Load()).Returns(_testTransactions);
        var startDate = DateTime.Parse("2024-12-01");
        var endDate = DateTime.Parse("2024-01-01");

        // Act
        var result = _service.GetTransactions(startDate, endDate).ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public void GetTransactions_ResultsOrderedByDate()
    {
        // Arrange
        var unorderedTransactions = new List<Transaction>
        {
            new() { Id = 1, Date = DateTime.Parse("2024-03-01"), Amount = 100m, DebitAccountNumber = "101", CreditAccountNumber = "201" },
            new() { Id = 2, Date = DateTime.Parse("2024-01-01"), Amount = 200m, DebitAccountNumber = "102", CreditAccountNumber = "202" },
            new() { Id = 3, Date = DateTime.Parse("2024-02-01"), Amount = 300m, DebitAccountNumber = "103", CreditAccountNumber = "203" }
        };
        _mockDbContext.Setup(x => x.Load()).Returns(unorderedTransactions);

        // Act
        var result = _service.GetTransactions(DateTime.Parse("2024-01-01"), DateTime.Parse("2024-12-31")).ToList();

        // Assert
        Assert.That(result[0].Date, Is.EqualTo(DateTime.Parse("2024-01-01")));
        Assert.That(result[1].Date, Is.EqualTo(DateTime.Parse("2024-02-01")));
        Assert.That(result[2].Date, Is.EqualTo(DateTime.Parse("2024-03-01")));
    }

    [Test]
    public void GetTrialBalanceItems_SingleTransaction_CorrectBalance()
    {
        // Arrange
        var singleTransaction = new List<Transaction>
        {
            new() { Id = 1, DebitAccountNumber = "101", CreditAccountNumber = "201", Amount = 1000m, Date = DateTime.Now }
        };
        _mockDbContext.Setup(x => x.Load()).Returns(singleTransaction);

        // Act
        var result = _service.GetTrialBalanceItems().ToList();

        // Assert
        Assert.That(result.Count, Is.EqualTo(2)); // Two accounts affected
        
        var account101 = result.First(x => x.AccountNumber == "101");
        Assert.That(account101.DebitTotal, Is.EqualTo(1000m));
        Assert.That(account101.CreditTotal, Is.EqualTo(0m));
        Assert.That(account101.Balance, Is.EqualTo(1000m));
        
        var account201 = result.First(x => x.AccountNumber == "201");
        Assert.That(account201.DebitTotal, Is.EqualTo(0m));
        Assert.That(account201.CreditTotal, Is.EqualTo(1000m));
        Assert.That(account201.Balance, Is.EqualTo(-1000m));
    }

    [Test]
    public void GetTrialBalanceItems_MultipleTransactionsSameAccount_AggregatesCorrectly()
    {
        // Arrange
        var transactions = new List<Transaction>
        {
            new() { Id = 1, DebitAccountNumber = "101", CreditAccountNumber = "201", Amount = 1000m, Date = DateTime.Now },
            new() { Id = 2, DebitAccountNumber = "101", CreditAccountNumber = "202", Amount = 500m, Date = DateTime.Now },
            new() { Id = 3, DebitAccountNumber = "203", CreditAccountNumber = "101", Amount = 300m, Date = DateTime.Now }
        };
        _mockDbContext.Setup(x => x.Load()).Returns(transactions);

        // Act
        var result = _service.GetTrialBalanceItems().ToList();

        // Assert
        var account101 = result.First(x => x.AccountNumber == "101");
        Assert.That(account101.DebitTotal, Is.EqualTo(1500m)); // 1000 + 500
        Assert.That(account101.CreditTotal, Is.EqualTo(300m)); // 300
        Assert.That(account101.Balance, Is.EqualTo(1200m)); // 1500 - 300
    }

    [Test]
    public void GetTrialBalanceItems_VerifyDoubleEntryPrinciple()
    {
        // Arrange
        _mockDbContext.Setup(x => x.Load()).Returns(_testTransactions);

        // Act
        var result = _service.GetTrialBalanceItems().ToList();

        // Assert - Total debits should equal total credits
        var totalDebits = result.Sum(x => x.DebitTotal);
        var totalCredits = result.Sum(x => x.CreditTotal);
        Assert.That(totalDebits, Is.EqualTo(totalCredits));
    }

    [Test]
    public void GetTrialBalanceItems_EmptyTransactions_ReturnsEmpty()
    {
        // Arrange
        _mockDbContext.Setup(x => x.Load()).Returns(new List<Transaction>());

        // Act
        var result = _service.GetTrialBalanceItems().ToList();

        // Assert
        Assert.That(result, Is.Empty);
    }
}

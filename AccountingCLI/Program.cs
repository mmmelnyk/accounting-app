using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AccountingCLI.Data;
using AccountingCLI.Services;
using AccountingCLI.Models;
using AccountingCLI.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .Build();

var appSettings = new AppSettings();
configuration.GetSection("AppSettings").Bind(appSettings);

// Set Ukrainian culture for currency formatting
var cultureInfo = new CultureInfo(appSettings.CultureName);
cultureInfo.NumberFormat.CurrencySymbol = appSettings.CurrencySymbol;
Thread.CurrentThread.CurrentCulture = cultureInfo;
Thread.CurrentThread.CurrentUICulture = cultureInfo;

var serviceProvider = new ServiceCollection()
    .AddSingleton(appSettings)
    .AddSingleton<ITransactionDbContext>(provider => new TransactionDbContext(appSettings.DataFilePath))
    .AddTransient<ITransactionService, TransactionService>()
    .BuildServiceProvider();

var transactionService = serviceProvider.GetRequiredService<ITransactionService>();

var assembly = Assembly.GetExecutingAssembly();
var version = assembly.GetName().Version?.ToString() ?? "1.0.0";
Console.WriteLine($"Облік фінансових операцій v{version}");

while (true)
{
    Console.WriteLine("------------------------");
    Console.WriteLine("Оберіть дію:");
    Console.WriteLine("1. Додати операцію");
    Console.WriteLine("2. Переглянути операції");
    Console.WriteLine("3. Показати оборотно-сальдову відомість");
    Console.WriteLine("4. Фільтрувати операції за датою");
    Console.WriteLine("5. Згенерувати тестові дані");
    Console.WriteLine("6. Вийти");
    Console.Write("Введіть номер дії: ");
    var input = Console.ReadLine();

    switch (input)
    {
        case "1":
            // Додати операцію
            AddTransaction();
            break;
        case "2":
            // Переглянути операції
            ShowAllTransactions();
            break;
        case "3":
            // Показати оборотно-сальдову відомість
            ShowTrialBalance();
            break;
        case "4":
            // Показати операції за період
            ShowTransactionsForDateRange();
            break;
        case "5":
            // Згенерувати тестові дані
            transactionService.GenerateSeedData();
            break;
        case "6":
            return;
        default:
            Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
            break;
    }
}

void AddTransaction()
{
    Console.Write("Введіть номер дебетового рахунку: ");
    var debitAccountNumber = int.TryParse(Console.ReadLine(), out var debit) ? debit : 0;
    Console.Write("Введіть номер кредитового рахунку: ");
    var CreditAccountNumber = int.TryParse(Console.ReadLine(), out var credit) ? credit : 0;
    Console.Write("Введіть дату (yyyy-MM-dd): ");
    var dateInput = Console.ReadLine();
    var date = DateTime.TryParse(dateInput, out var parsedDate) ? parsedDate : DateTime.Now;
    Console.Write("Введіть суму: ");
    var amount = decimal.TryParse(Console.ReadLine(), out var parsedAmount) ? parsedAmount : 0;
    Console.Write("Введіть опис: ");
    var description = Console.ReadLine() ?? string.Empty;
    var transaction = new Transaction
    {
        DebitAccountNumber = debitAccountNumber,
        CreditAccountNumber = CreditAccountNumber,
        Date = date,
        Amount = amount,
        Description = description
    };

    // Валідація моделі
    var errors = new List<ValidationResult>();
    Validator.TryValidateObject(transaction, new ValidationContext(transaction), errors, true);
    if (errors.Count > 0)
    {
        Console.WriteLine("Помилки валідації:");
        foreach (var error in errors)
        {
            Console.WriteLine($" - {error.ErrorMessage}");
        }
    }
    else
    {
        transactionService.AddTransaction(transaction);
        Console.WriteLine("Операцію додано.");
    }
}

void ShowAllTransactions()
{
    Console.WriteLine("Id | Дата | Рахунок дебету | Рахунок кредиту | Сума | Коментар");
    Console.WriteLine(new string('-', 80));
    foreach (var transaction in transactionService.GetTransactions())
    {
        Console.WriteLine($"{transaction.Id,3} | {transaction.Date:yyyy-MM-dd} | {transaction.DebitAccountNumber,13} | {transaction.CreditAccountNumber,15} | {transaction.Amount,10:C} | {transaction.Description}");
    }
    Console.WriteLine("------------------------");
}

void ShowTransactionsForDateRange()
{
    Console.Write("Початкова дата (yyyy-MM-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out var startDate))
    {
        Console.WriteLine("Некоректна початкова дата.");
        return;
    }

    Console.Write("Кінцева дата (yyyy-MM-dd): ");
    if (!DateTime.TryParse(Console.ReadLine(), out var endDate))
    {
        Console.WriteLine("Некоректна кінцева дата.");
        return;
    }

    if (endDate < startDate)
    {
        Console.WriteLine("Кінцева дата не може бути раніше за початкову.");
        return;
    }

    var transactionsInRange = transactionService.GetTransactions(startDate, endDate);
    if (!transactionsInRange.Any())
    {
        Console.WriteLine("Немає проводок у вказаному періоді.");
        return;
    }

    Console.WriteLine("Id | Дата | Рахунок дебету | Рахунок кредиту | Сума | Коментар");
    Console.WriteLine(new string('-', 80));
    foreach (var transaction in transactionsInRange)
    {
        Console.WriteLine($"{transaction.Id,3} | {transaction.Date:yyyy-MM-dd} | {transaction.DebitAccountNumber,13} | {transaction.CreditAccountNumber,15} | {transaction.Amount,10:C} | {transaction.Description}");
    }
    Console.WriteLine("------------------------");
}


void ShowTrialBalance()
{
    Console.WriteLine("Рахунок | Дебетовий оборот | Кредитовий оборот | Сальдо");
    Console.WriteLine(new string('-', 70));
    foreach (var acc in transactionService.GetTrialBalanceItems())
    {
        Console.WriteLine($"{acc.AccountNumber,7} | {acc.DebitTotal,15:C} | {acc.CreditTotal,17:C} | {acc.Balance,10:C}");
    }
    Console.WriteLine("------------------------");
}   
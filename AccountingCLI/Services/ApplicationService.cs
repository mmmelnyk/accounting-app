using AccountingCLI.Models;
using AccountingCLI.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AccountingCLI.Services;

public class ApplicationService : IApplicationService
{
    private readonly ITransactionService _transactionService;
    private readonly AppSettings _appSettings;

    public ApplicationService(ITransactionService transactionService, AppSettings appSettings)
    {
        _transactionService = transactionService;
        _appSettings = appSettings;
    }

    public void Run()
    {
        ShowWelcomeMessage();

        while (true)
        {
            ShowMainMenu();
            var input = Console.ReadLine();

            switch (input)
            {
                case "1":
                    AddTransaction();
                    break;
                case "2":
                    ShowAllTransactions();
                    break;
                case "3":
                    ShowTrialBalance();
                    break;
                case "4":
                    ShowTransactionsForDateRange();
                    break;
                case "5":
                    _transactionService.GenerateSeedData();
                    break;
                case "6":
                    Console.WriteLine("До побачення!");
                    return;
                default:
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                    break;
            }

            Console.WriteLine("\nНатисніть будь-яку клавішу для продовження...");
            Console.ReadKey();
            Console.Clear();
        }
    }

    private void ShowWelcomeMessage()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        Console.WriteLine($"Облік фінансових операцій v{version}");
        Console.WriteLine();
    }

    private void ShowMainMenu()
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
    }

    private void AddTransaction()
    {
        Console.Clear();
        Console.WriteLine("=== Додавання операції ===");
        
        Console.Write("Введіть номер дебетового рахунку: ");
        var debitAccountNumber = int.TryParse(Console.ReadLine(), out var debit) ? debit : 0;
        
        Console.Write("Введіть номер кредитового рахунку: ");
        var creditAccountNumber = int.TryParse(Console.ReadLine(), out var credit) ? credit : 0;
        
        Console.Write($"Введіть дату ({_appSettings.DateFormat}): ");
        var dateInput = Console.ReadLine();
        var date = DateTime.TryParse(dateInput, out var parsedDate) ? parsedDate : DateTime.Now;
        
        Console.Write("Введіть суму: ");
        var amount = decimal.TryParse(Console.ReadLine(), out var parsedAmount) ? parsedAmount : 0;
        
        Console.Write("Введіть опис: ");
        var description = Console.ReadLine() ?? string.Empty;
        
        var transaction = new Transaction
        {
            DebitAccountNumber = debitAccountNumber,
            CreditAccountNumber = creditAccountNumber,
            Date = date,
            Amount = amount,
            Description = description
        };

        // Валідація моделі
        var errors = new List<ValidationResult>();
        Validator.TryValidateObject(transaction, new ValidationContext(transaction), errors, true);
        
        if (errors.Count > 0)
        {
            Console.WriteLine("\nПомилки валідації:");
            foreach (var error in errors)
            {
                Console.WriteLine($" - {error.ErrorMessage}");
            }
        }
        else
        {
            _transactionService.AddTransaction(transaction);
            Console.WriteLine("\nОперацію успішно додано!");
        }
    }

    private void ShowAllTransactions()
    {
        Console.Clear();
        Console.WriteLine("=== Всі операції ===");
        
        var transactions = _transactionService.GetTransactions().ToList();
        
        if (transactions.Count == 0)
        {
            Console.WriteLine("Немає операцій для відображення.");
            return;
        }

        Console.WriteLine("Id | Дата       | Рахунок дебету | Рахунок кредиту | Сума        | Коментар");
        Console.WriteLine(new string('-', 80));
        
        foreach (var transaction in transactions)
        {
            Console.WriteLine($"{transaction.Id,3} | {transaction.Date:yyyy-MM-dd} | {transaction.DebitAccountNumber,13} | {transaction.CreditAccountNumber,15} | {transaction.Amount,10:C} | {transaction.Description}");
        }
    }

    private void ShowTrialBalance()
    {
        Console.Clear();
        Console.WriteLine("=== Оборотно-сальдова відомість ===");
        
        var balanceItems = _transactionService.GetTrialBalanceItems().ToList();
        
        if (balanceItems.Count == 0)
        {
            Console.WriteLine("Немає даних для відображення.");
            return;
        }

        Console.WriteLine("Рахунок | Дебетовий оборот | Кредитовий оборот | Сальдо");
        Console.WriteLine(new string('-', 70));
        
        foreach (var item in balanceItems)
        {
            Console.WriteLine($"{item.AccountNumber,7} | {item.DebitTotal,15:C} | {item.CreditTotal,17:C} | {item.Balance,10:C}");
        }
    }

    private void ShowTransactionsForDateRange()
    {
        Console.Clear();
        Console.WriteLine("=== Фільтрація операцій за датою ===");
        
        Console.Write($"Початкова дата ({_appSettings.DateFormat}): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var startDate))
        {
            Console.WriteLine("Некоректна початкова дата.");
            return;
        }

        Console.Write($"Кінцева дата ({_appSettings.DateFormat}): ");
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

        var transactionsInRange = _transactionService.GetTransactions(startDate, endDate).ToList();
        
        if (transactionsInRange.Count == 0)
        {
            Console.WriteLine("Немає проводок у вказаному періоді.");
            return;
        }

        Console.WriteLine("\nId | Дата       | Рахунок дебету | Рахунок кредиту | Сума        | Коментар");
        Console.WriteLine(new string('-', 80));
        
        foreach (var transaction in transactionsInRange)
        {
            Console.WriteLine($"{transaction.Id,3} | {transaction.Date:yyyy-MM-dd} | {transaction.DebitAccountNumber,13} | {transaction.CreditAccountNumber,15} | {transaction.Amount,10:C} | {transaction.Description}");
        }
    }
}

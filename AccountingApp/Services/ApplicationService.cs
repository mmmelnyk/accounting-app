using AccountingApp.Models;
using AccountingApp.Configuration;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AccountingApp.Services;

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
                    return;
                default:
                    Console.WriteLine("Невірний вибір. Спробуйте ще раз.");
                    break;
            }

            Console.WriteLine(); // Just add some spacing instead of clearing
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
        Console.WriteLine("\n=== Додавання операції ===");
        
        Console.Write("Введіть номер дебетового рахунку: ");
        var debitAccountNumber = Console.ReadLine() ?? string.Empty;
        
        Console.Write("Введіть номер кредитового рахунку: ");
        var creditAccountNumber = Console.ReadLine()  ?? string.Empty;
        
        Console.Write($"Введіть дату ({_appSettings.DateFormat}) або залиште порожнім для поточної дати: ");
        var dateInput = Console.ReadLine();
        
        DateTime date;
        
        // If date input is null or empty, use current date
        if (string.IsNullOrWhiteSpace(dateInput))
        {
            date = DateTime.Now;
        }
        else
        {
            // Validate date format using regex - only yyyy-MM-dd format
            var dateRegex = new System.Text.RegularExpressions.Regex(@"^\d{4}-\d{2}-\d{2}$");
            if (!dateRegex.IsMatch(dateInput))
            {
                Console.WriteLine($"Помилка: Дата повинна бути у форматі {_appSettings.DateFormat}");
                return;
            }
            
            if (!DateTime.TryParse(dateInput, out date))
            {
                Console.WriteLine($"Помилка: '{dateInput}' не є дійсною датою.");
                return;
            }
        }
        
        Console.Write("Введіть суму: ");
        var amountInput = Console.ReadLine();
        if (!decimal.TryParse(amountInput, System.Globalization.NumberStyles.Number, 
                             System.Globalization.CultureInfo.InvariantCulture, out var amount) &&
            !decimal.TryParse(amountInput, System.Globalization.NumberStyles.Number, 
                             System.Globalization.CultureInfo.CurrentCulture, out amount))
        {
            Console.WriteLine($"Помилка: '{amountInput}' не є дійсною сумою. Використовуйте формат: 0.01 або 0,01");
            return;
        }
        
        Console.Write("Введіть коментар: ");
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
        Console.WriteLine("\n=== Всі операції ===");
        
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
        Console.WriteLine("\n=== Оборотно-сальдова відомість ===");
        
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
        Console.WriteLine("\n=== Фільтрація операцій за датою ===");
        
        Console.Write($"Початкова дата ({_appSettings.DateFormat}): ");
        if (!DateTime.TryParse(Console.ReadLine(), out var startDate))
        {
            Console.WriteLine("Некоректна початкова дата.");
            return;
        }

        // Check if start date is in the future
        if (startDate.Date > DateTime.Now.Date)
        {
            Console.WriteLine("Початкова дата не може бути в майбутньому.");
            return;
        }

        Console.Write($"Кінцева дата ({_appSettings.DateFormat}) або залиште порожнім для поточної дати: ");
        var endDateInput = Console.ReadLine();
        
        DateTime endDate;
        if (string.IsNullOrWhiteSpace(endDateInput))
        {
            endDate = DateTime.Now;
        }
        else
        {
            if (!DateTime.TryParse(endDateInput, out endDate))
            {
                Console.WriteLine("Некоректна кінцева дата.");
                return;
            }
            
            // Check if end date is in the future
            if (endDate.Date > DateTime.Now.Date)
            {
                Console.WriteLine("Кінцева дата не може бути в майбутньому.");
                return;
            }
        }

        if (endDate < startDate)
        {
            Console.WriteLine("Кінцева дата не може бути раніше за початкову.");
            return;
        }

        var transactionsInRange = _transactionService.GetTransactions(startDate, endDate).ToList();
        
        if (transactionsInRange.Count == 0)
        {
            Console.WriteLine($"Немає проводок у періоді з {startDate:yyyy-MM-dd} по {endDate:yyyy-MM-dd}");
            return;
        }
        Console.WriteLine($"\nПроводки в períоді з {startDate:yyyy-MM-dd} по {endDate:yyyy-MM-dd}");
        Console.WriteLine("Id | Дата       | Рахунок дебету | Рахунок кредиту | Сума        | Коментар");
        Console.WriteLine(new string('-', 80));
        
        foreach (var transaction in transactionsInRange)
        {
            Console.WriteLine($"{transaction.Id,3} | {transaction.Date:yyyy-MM-dd} | {transaction.DebitAccountNumber,13} | {transaction.CreditAccountNumber,15} | {transaction.Amount,10:C} | {transaction.Description}");
        }
    }
}

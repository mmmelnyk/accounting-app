using System.ComponentModel.DataAnnotations;

namespace AccountingCLI.Models;

public class Transaction
{
    public int Id { get; set; }
    [Required(ErrorMessage = "ID дебетового рахунку є обов'язковим")]
    [Range(1, int.MaxValue, ErrorMessage = "ID дебетового рахунку має бути більшим за 0")]
    public int DebitAccountNumber { get; set; }
    [Required(ErrorMessage = "ID кредитового рахунку є обов'язковим")]
    [Range(1, int.MaxValue, ErrorMessage = "ID кредитового рахунку має бути більшим за 0")]
    public int CreditAccountNumber { get; set; }
    [Required(ErrorMessage = "Дата є обов'язковою")]
    public DateTime Date { get; set; }
    [Required(ErrorMessage = "Сума операції є обов'язковою")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Сума операції повинна бути більшою за 0")]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; } // Використовуємо decimal для точності фінансових розрахунків 
    public string? Description { get; set; }
}

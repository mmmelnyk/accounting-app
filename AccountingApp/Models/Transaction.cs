using System.ComponentModel.DataAnnotations;

namespace AccountingApp.Models;

public class Transaction
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Номер дебетового рахунку є обов'язковим")]
    [StringLength(29, MinimumLength = 1, ErrorMessage = "Номер дебетового рахунку повинен містити від 1 до 29 символів")]
    public required string DebitAccountNumber { get; set; }
    [Required(ErrorMessage = "Номер кредитового рахунку є обов'язковим")]
    [StringLength(29, MinimumLength = 1, ErrorMessage = "Номер кредитового рахунку повинен містити від 1 до 29 символів")]
    public required string CreditAccountNumber { get; set; }
    [Required(ErrorMessage = "Дата є обов'язковою")]
    public DateTime Date { get; set; }
    [Required(ErrorMessage = "Сума операції є обов'язковою")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Сума операції повинна бути більшою чи рівною 0.01")]
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; } // Використовуємо decimal для точності фінансових розрахунків 
    public string? Description { get; set; }
}

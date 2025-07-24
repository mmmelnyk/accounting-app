namespace AccountingApp.Models;

public class TrialBalance
{
    public required string AccountNumber { get; set; }
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
    public decimal Balance { get; set; }
}

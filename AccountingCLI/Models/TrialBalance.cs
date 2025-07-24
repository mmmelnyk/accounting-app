namespace AccountingCLI.Models;

public class TrialBalance
{
    public int AccountNumber { get; set; }
    public decimal DebitTotal { get; set; }
    public decimal CreditTotal { get; set; }
    public decimal Balance { get; set; }
}

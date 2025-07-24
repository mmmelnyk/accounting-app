namespace AccountingApp.Configuration;

public class AppSettings
{
    public string DataFilePath { get; set; } = "transactions.json";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public string CurrencySymbol { get; set; } = "₴";
    public string CultureName { get; set; } = "uk-UA";
}

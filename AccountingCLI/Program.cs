using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using AccountingCLI.Data;
using AccountingCLI.Services;
using AccountingCLI.Configuration;
using System.Globalization;

// Load configuration
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
    .Build();

var appSettings = new AppSettings();
configuration.GetSection("AppSettings").Bind(appSettings);

// Set culture for currency formatting
var cultureInfo = new CultureInfo(appSettings.CultureName);
cultureInfo.NumberFormat.CurrencySymbol = appSettings.CurrencySymbol;
Thread.CurrentThread.CurrentCulture = cultureInfo;

// Configure dependency injection
var serviceProvider = new ServiceCollection()
    .AddSingleton(appSettings)
    .AddSingleton<ITransactionDbContext>(provider => new TransactionDbContext(appSettings.DataFilePath))
    .AddTransient<ITransactionService, TransactionService>()
    .AddTransient<IApplicationService, ApplicationService>()
    .BuildServiceProvider();

// Run the application
var app = serviceProvider.GetRequiredService<IApplicationService>();
app.Run();   
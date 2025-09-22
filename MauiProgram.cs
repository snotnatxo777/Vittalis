using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Vittalis.Data;
using Vittalis.Services;
using Vittalis.ViewModels;
using Vittalis.Views;
using CommunityToolkit.Maui;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace Vittalis;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        builder.Services.AddLogging(configure => configure.AddDebug());

        // Configurar banco de dados SQLite
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "vittalis.db");
        builder.Services.AddDbContext<VittalisDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        // Registrar serviços
        builder.Services.AddScoped<ITransactionService, TransactionService>();
        builder.Services.AddScoped<IReportService, ReportService>();
        builder.Services.AddScoped<IInvestmentService, InvestmentService>();
        builder.Services.AddScoped<IAccountService, AccountService>();
        builder.Services.AddScoped<IRecurringTransactionService, RecurringTransactionService>();
        builder.Services.AddScoped<ISpendingGoalService, SpendingGoalService>();
        builder.Services.AddTransient<ComparisonViewModel>();
        builder.Services.AddScoped<ICreditCardService, CreditCardService>();
        builder.Services.AddScoped<IAdvancedReportService, AdvancedReportService>();

        // Registrar ViewModels
        builder.Services.AddTransient<TransactionListViewModel>();
        builder.Services.AddTransient<DashboardViewModel>();
        builder.Services.AddTransient<ReportsViewModel>();
        builder.Services.AddTransient<InvestmentViewModel>();
        builder.Services.AddTransient<AccountsViewModel>();
        builder.Services.AddTransient<RecurringTransactionsViewModel>();
        builder.Services.AddTransient<SpendingGoalsViewModel>();
        builder.Services.AddTransient<CreditCardViewModel>();
        builder.Services.AddTransient<AdvancedReportsViewModel>();

        // Registrar Views
        builder.Services.AddTransient<TransactionListPage>();
        builder.Services.AddTransient<AddTransactionPage>();
        builder.Services.AddTransient<DashboardPage>();
        builder.Services.AddTransient<ReportsPage>();
        builder.Services.AddTransient<InvestmentPage>();
        builder.Services.AddTransient<AddTradePage>();
        builder.Services.AddTransient<AccountsPage>();
        builder.Services.AddTransient<RecurringTransactionsPage>();
        builder.Services.AddTransient<AddRecurringTransactionPage>();
        builder.Services.AddTransient<SpendingGoalsPage>();
        builder.Services.AddTransient<AddSpendingGoalPage>();
        builder.Services.AddTransient<ComparisonPage>();
        builder.Services.AddTransient<CreditCardsPage>();
        builder.Services.AddTransient<AddCreditCardPage>();
        builder.Services.AddTransient<AddCreditCardTransactionPage>();
        builder.Services.AddTransient<AdvancedReportsPage>();

        return builder.Build();
    }
}
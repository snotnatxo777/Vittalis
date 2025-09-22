using Vittalis.Views;
using Vittalis.ViewModels;
using Vittalis.Services;

namespace Vittalis;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private async void OnDashboardClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var dashboardPage = serviceProvider?.GetService<DashboardPage>();

            if (dashboardPage != null)
            {
                await Navigation.PushAsync(dashboardPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar dashboard", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro de navegação: {ex.Message}", "OK");
        }
    }

    private async void OnTransactionsClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var transactionPage = serviceProvider?.GetService<TransactionListPage>();

            if (transactionPage != null)
            {
                await Navigation.PushAsync(transactionPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar transações", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro de navegação: {ex.Message}", "OK");
        }
    }

    private async void OnReportsClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var reportsPage = serviceProvider?.GetService<ReportsPage>();

            if (reportsPage != null)
            {
                await Navigation.PushAsync(reportsPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar relatórios", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar relatórios: {ex.Message}", "OK");
        }
    }

    private async void OnInvestmentsClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var investmentPage = serviceProvider?.GetService<InvestmentPage>();

            if (investmentPage != null)
            {
                await Navigation.PushAsync(investmentPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar investimentos", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar investimentos: {ex.Message}", "OK");
        }
    }

    private async void OnAccountsClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var accountsPage = serviceProvider?.GetService<AccountsPage>();

            if (accountsPage != null)
            {
                await Navigation.PushAsync(accountsPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar contas", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar contas: {ex.Message}", "OK");
        }
    }

    private async void OnRecurringTransactionsClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var recurringPage = serviceProvider?.GetService<RecurringTransactionsPage>();

            if (recurringPage != null)
            {
                await Navigation.PushAsync(recurringPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar transações recorrentes", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar transações recorrentes: {ex.Message}", "OK");
        }
    }

    private async void OnSpendingGoalsClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var spendingGoalsPage = serviceProvider?.GetService<SpendingGoalsPage>();

            if (spendingGoalsPage != null)
            {
                await Navigation.PushAsync(spendingGoalsPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar metas de gastos", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar metas de gastos: {ex.Message}", "OK");
        }
    }

    private async void OnComparisonClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var comparisonPage = serviceProvider?.GetService<ComparisonPage>();

            if (comparisonPage != null)
            {
                await Navigation.PushAsync(comparisonPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar comparativos", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar comparativos: {ex.Message}", "OK");
        }
    }

    private async void OnCreditCardsClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var creditCardsPage = serviceProvider?.GetService<CreditCardsPage>();

            if (creditCardsPage != null)
            {
                await Navigation.PushAsync(creditCardsPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar cartões de crédito", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar cartões de crédito: {ex.Message}", "OK");
        }
    }

    // ✨ NOVO MÉTODO PARA RELATÓRIOS AVANÇADOS
    private async void OnAdvancedReportsClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var advancedReportsPage = serviceProvider?.GetService<AdvancedReportsPage>();

            if (advancedReportsPage != null)
            {
                await Navigation.PushAsync(advancedReportsPage);
            }
            else
            {
                await DisplayAlert("Erro", "Erro ao carregar relatórios avançados", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro de navegação: {ex.Message}", "OK");
        }
    }
}
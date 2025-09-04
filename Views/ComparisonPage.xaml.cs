using Vittalis.ViewModels;
using Vittalis.Models;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class ComparisonPage : ContentPage
{
    private readonly ComparisonViewModel _viewModel;

    public ComparisonPage(ComparisonViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        var previousMonth = _viewModel.SelectedDate.AddMonths(-1);
        await _viewModel.ChangePeriodAsync(previousMonth);
    }

    private async void OnNextMonthClicked(object sender, EventArgs e)
    {
        var nextMonth = _viewModel.SelectedDate.AddMonths(1);

        // Permitir navegar até o mês atual
        if (nextMonth <= DateTime.Now)
        {
            await _viewModel.ChangePeriodAsync(nextMonth);
        }
    }

    private async void OnSelectAccountClicked(object sender, EventArgs e)
    {
        var options = new List<string> { "Todas as Contas" };
        var accounts = new List<Account?> { null };

        foreach (var account in _viewModel.Accounts)
        {
            var icon = AccountHelper.GetAccountTypeIcon(account.Type);
            options.Add($"{icon} {account.Name}");
            accounts.Add(account);
        }

        var result = await DisplayActionSheet(
            "Selecionar Conta",
            "Cancelar",
            null,
            options.ToArray());

        if (!string.IsNullOrEmpty(result) && result != "Cancelar")
        {
            var selectedIndex = options.IndexOf(result);
            if (selectedIndex >= 0)
            {
                await _viewModel.ChangeAccountFilterAsync(accounts[selectedIndex]);
            }
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync();
    }
}
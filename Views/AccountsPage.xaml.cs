using Vittalis.ViewModels;
using Vittalis.Models;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class AccountsPage : ContentPage
{
    private readonly AccountsViewModel _viewModel;

    public AccountsPage(AccountsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnAddAccountClicked(object sender, EventArgs e)
    {
        // Mostrar dialog para criar nova conta
        var accountTypes = Enum.GetValues<AccountType>()
            .Select(t => AccountHelper.GetAccountTypeName(t))
            .ToArray();

        var selectedType = await DisplayActionSheet(
            "Tipo da Conta",
            "Cancelar",
            null,
            accountTypes);

        if (string.IsNullOrEmpty(selectedType) || selectedType == "Cancelar")
            return;

        // Obter o tipo selecionado
        var accountType = Enum.GetValues<AccountType>()
            .FirstOrDefault(t => AccountHelper.GetAccountTypeName(t) == selectedType);

        // Solicitar nome da conta
        var accountName = await DisplayPromptAsync(
            "Nova Conta",
            "Digite o nome da conta:",
            "OK",
            "Cancelar",
            placeholder: $"Ex: {selectedType} Principal");

        if (string.IsNullOrWhiteSpace(accountName))
            return;

        // Solicitar saldo inicial
        var initialBalanceText = await DisplayPromptAsync(
            "Saldo Inicial",
            "Digite o saldo inicial (opcional):",
            "OK",
            "Cancelar",
            placeholder: "0,00",
            keyboard: Keyboard.Numeric);

        decimal.TryParse(initialBalanceText, out decimal initialBalance);

        // Criar a conta
        var newAccount = new Account
        {
            Name = accountName.Trim(),
            Type = accountType,
            InitialBalance = initialBalance,
            IsActive = true
        };

        await _viewModel.AddAccountAsync(newAccount);
        await DisplayAlert("Sucesso", "Conta criada com sucesso!", "OK");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync();
    }

    private async void OnManageCreditCardsClicked(object sender, EventArgs e)
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

    private async void OnCreditCardsClicked(object sender, EventArgs e)
    {
        var serviceProvider = Handler?.MauiContext?.Services;
        var creditCardsPage = serviceProvider?.GetService<CreditCardsPage>();

        if (creditCardsPage != null)
        {
            await Navigation.PushAsync(creditCardsPage);
        }
    }
}
using Vittalis.Models;
using Vittalis.Services;
using Vittalis.ViewModels;

namespace Vittalis.Views;

public partial class CreditCardsPage : ContentPage
{
    private readonly CreditCardViewModel _viewModel;

    public CreditCardsPage(CreditCardViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnAddCreditCardClicked(object sender, EventArgs e)
    {
        var serviceProvider = Handler?.MauiContext?.Services;
        var addPage = serviceProvider?.GetService<AddCreditCardPage>();

        if (addPage != null)
        {
            await Navigation.PushAsync(addPage);
        }
        else
        {
            await DisplayAlert("Erro", "Erro ao carregar página de novo cartão", "OK");
        }
    }

    private async void OnAddTransactionClicked(object sender, EventArgs e)
    {
        if (!_viewModel.CreditCards.Any())
        {
            await DisplayAlert("Aviso", "Cadastre um cartão de crédito primeiro", "OK");
            return;
        }

        var serviceProvider = Handler?.MauiContext?.Services;
        var addPage = serviceProvider?.GetService<AddCreditCardTransactionPage>();

        if (addPage != null)
        {
            await Navigation.PushAsync(addPage);
        }
        else
        {
            await DisplayAlert("Erro", "Erro ao carregar página de nova compra", "OK");
        }
    }

    private async void OnCardMenuClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is CreditCard creditCard)
        {
            var action = await DisplayActionSheet(
                $"Cartão: {creditCard.Name}",
                "Cancelar",
                null,
                "Ver Fatura",
                "Editar Limite",
                "Parcelar Compra",
                "Excluir Cartão");

            switch (action)
            {
                case "Ver Fatura":
                    await ViewBill(creditCard);
                    break;
                case "Editar Limite":
                    await EditCreditLimit(creditCard);
                    break;
                case "Parcelar Compra":
                    await AddInstallment(creditCard);
                    break;
                case "Excluir Cartão":
                    await DeleteCreditCard(creditCard);
                    break;
            }
        }
    }

    private async Task ViewBill(CreditCard creditCard)
    {
        await DisplayAlert("Em desenvolvimento", "Visualização de fatura será implementada em breve", "OK");
    }

    private async Task EditCreditLimit(CreditCard creditCard)
    {
        var currentLimit = creditCard.CreditLimit.ToString("F2");
        var newLimitText = await DisplayPromptAsync(
            "Editar Limite",
            $"Novo limite para {creditCard.Name}:",
            "OK",
            "Cancelar",
            placeholder: currentLimit,
            keyboard: Keyboard.Numeric);

        if (!string.IsNullOrWhiteSpace(newLimitText) &&
            decimal.TryParse(newLimitText, out decimal newLimit) &&
            newLimit > 0)
        {
            creditCard.CreditLimit = newLimit;

            var serviceProvider = Handler?.MauiContext?.Services;
            var creditCardService = serviceProvider?.GetService<ICreditCardService>();

            if (creditCardService != null)
            {
                await creditCardService.UpdateCreditCardAsync(creditCard);
                await _viewModel.RefreshAsync();
                await DisplayAlert("Sucesso", "Limite atualizado com sucesso!", "OK");
            }
        }
    }

    private async Task AddInstallment(CreditCard creditCard)
    {
        await DisplayAlert("Em desenvolvimento", "Funcionalidade de parcelamento será implementada em breve", "OK");
    }

    private async Task DeleteCreditCard(CreditCard creditCard)
    {
        var result = await DisplayAlert(
            "Confirmar Exclusão",
            $"Deseja realmente excluir o cartão '{creditCard.Name}'?",
            "Sim",
            "Não");

        if (result)
        {
            await _viewModel.DeleteCreditCardAsync(creditCard.Id);
            await DisplayAlert("Sucesso", "Cartão excluído com sucesso!", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync();
    }
}
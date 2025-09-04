using Vittalis.ViewModels;
using Vittalis.Models;

namespace Vittalis.Views;

public partial class RecurringTransactionsPage : ContentPage
{
    private readonly RecurringTransactionsViewModel _viewModel;

    public RecurringTransactionsPage(RecurringTransactionsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnAddRecurringTransactionClicked(object sender, EventArgs e)
    {
        var serviceProvider = Handler?.MauiContext?.Services;
        var addPage = serviceProvider?.GetService<AddRecurringTransactionPage>();

        if (addPage != null)
        {
            await Navigation.PushAsync(addPage);
        }
        else
        {
            await DisplayAlert("Erro", "Erro ao carregar página de nova recorrência", "OK");
        }
    }

    private async void OnProcessPendingClicked(object sender, EventArgs e)
    {
        if (_viewModel.PendingCount == 0)
        {
            await DisplayAlert("Informação", "Não há transações pendentes para processar", "OK");
            return;
        }

        var result = await DisplayAlert(
            "Confirmar Processamento",
            $"Deseja processar {_viewModel.PendingCount} transação(ões) pendente(s)? Isso criará as transações automaticamente.",
            "Sim",
            "Não");

        if (result)
        {
            await _viewModel.ProcessPendingTransactionsAsync();
            await DisplayAlert("Sucesso", "Transações processadas com sucesso!", "OK");
        }
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is RecurringTransaction recurringTransaction)
        {
            var action = await DisplayActionSheet(
                $"Ações para: {recurringTransaction.Description}",
                "Cancelar",
                null,
                "Editar",
                "Desativar",
                "Excluir");

            switch (action)
            {
                case "Editar":
                    await EditRecurringTransaction(recurringTransaction);
                    break;
                case "Desativar":
                    await DeactivateRecurringTransaction(recurringTransaction);
                    break;
                case "Excluir":
                    await DeleteRecurringTransaction(recurringTransaction);
                    break;
            }
        }
    }

    private async Task EditRecurringTransaction(RecurringTransaction recurringTransaction)
    {
        await DisplayAlert("Em desenvolvimento", "Funcionalidade de edição será implementada em breve", "OK");
    }

    private async Task DeactivateRecurringTransaction(RecurringTransaction recurringTransaction)
    {
        var result = await DisplayAlert(
            "Confirmar Desativação",
            $"Deseja desativar a transação recorrente '{recurringTransaction.Description}'? Ela não será mais processada automaticamente.",
            "Sim",
            "Não");

        if (result)
        {
            await _viewModel.DeleteRecurringTransactionAsync(recurringTransaction.Id);
            await DisplayAlert("Sucesso", "Transação recorrente desativada!", "OK");
        }
    }

    private async Task DeleteRecurringTransaction(RecurringTransaction recurringTransaction)
    {
        var result = await DisplayAlert(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a transação recorrente '{recurringTransaction.Description}' de {recurringTransaction.Amount:C}?",
            "Sim",
            "Não");

        if (result)
        {
            await _viewModel.DeleteRecurringTransactionAsync(recurringTransaction.Id);
            await DisplayAlert("Sucesso", "Transação recorrente excluída!", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync();
    }
}
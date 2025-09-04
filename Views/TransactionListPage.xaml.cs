using Vittalis.ViewModels;
using Vittalis.Views;
using Vittalis.Models;

namespace Vittalis.Views;

public partial class TransactionListPage : ContentPage
{
    private readonly TransactionListViewModel _viewModel;
    private async void OnSearchButtonPressed(object sender, EventArgs e)
    {
        if (sender is SearchBar searchBar)
        {
            await _viewModel.SearchTransactionsAsync(searchBar.Text ?? string.Empty);
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (string.IsNullOrEmpty(e.NewTextValue))
        {
            await _viewModel.SearchTransactionsAsync(string.Empty);
        }
    }

    private async void OnFilterChanged(object sender, EventArgs e)
    {
        if (sender is Picker picker && picker.SelectedItem is string filterType)
        {
            await _viewModel.FilterByTypeAsync(filterType);
        }
    }
    public TransactionListPage(TransactionListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnAddTransactionClicked(object sender, EventArgs e)
    {
        var addPage = Handler?.MauiContext?.Services.GetService<AddTransactionPage>();
        if (addPage != null)
        {
            await Navigation.PushAsync(addPage);
        }
    }

    private async void OnTransactionTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Transaction transaction)
        {
            var action = await DisplayActionSheet(
                $"Transação: {transaction.Description}",
                "Cancelar",
                null,
                "Editar",
                "Excluir"
            );

            switch (action)
            {
                case "Editar":
                    await EditTransaction(transaction);
                    break;
                case "Excluir":
                    await DeleteTransaction(transaction);
                    break;
            }
        }
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is Transaction transaction)
        {
            var action = await DisplayActionSheet(
                $"Ações para: {transaction.Description}",
                "Cancelar",
                null,
                "Editar",
                "Excluir"
            );

            switch (action)
            {
                case "Editar":
                    await EditTransaction(transaction);
                    break;
                case "Excluir":
                    await DeleteTransaction(transaction);
                    break;
            }
        }
    }

    private async Task EditTransaction(Transaction transaction)
    {
        var serviceProvider = Handler?.MauiContext?.Services;
        if (serviceProvider != null)
        {
            var transactionService = serviceProvider.GetService<Vittalis.Services.ITransactionService>();
            if (transactionService != null)
            {
                var editPage = new EditTransactionPage(transactionService, transaction);
                await Navigation.PushAsync(editPage);
            }
        }
    }

    private async Task DeleteTransaction(Transaction transaction)
    {
        var result = await DisplayAlert(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a transação '{transaction.Description}' de {transaction.Amount:C}?",
            "Sim",
            "Não"
        );

        if (result)
        {
            await _viewModel.DeleteTransactionAsync(transaction.Id);
            await DisplayAlert("Sucesso", "Transação excluída com sucesso!", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync();
    }
}
using Vittalis.ViewModels;

namespace Vittalis.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnViewAllTransactionsClicked(object sender, EventArgs e)
    {
        var transactionPage = Handler?.MauiContext?.Services.GetService<TransactionListPage>();
        if (transactionPage != null)
        {
            await Navigation.PushAsync(transactionPage);
        }
    }

    private async void OnAddTransactionClicked(object sender, EventArgs e)
    {
        var addPage = Handler?.MauiContext?.Services.GetService<AddTransactionPage>();
        if (addPage != null)
        {
            await Navigation.PushAsync(addPage);
        }
    }
}
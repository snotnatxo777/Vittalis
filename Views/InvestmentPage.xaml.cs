using Microsoft.Extensions.DependencyInjection;
using Vittalis.Services;
using Vittalis.ViewModels;
using System.Globalization;

namespace Vittalis.Views;

public partial class InvestmentPage : ContentPage
{
    private readonly InvestmentViewModel _viewModel;

    public InvestmentPage(InvestmentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        // Adicionar os conversores como recursos da página
        Resources.Add("ProfitLossToBoolean", new ProfitLossToBooleanConverter());
    }

    private async void OnAddTradeClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            if (serviceProvider != null)
            {
                var investmentService = serviceProvider.GetService<IInvestmentService>();
                if (investmentService != null)
                {
                    // Criar e navegar para AddTradePage
                    var addTradePage = new AddTradePage(investmentService);
                    await Navigation.PushAsync(addTradePage);
                }
                else
                {
                    await DisplayAlert("Erro", "Serviço de investimentos não disponível", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao abrir página de trade: {ex.Message}", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Atualizar dados quando a página aparecer
            await _viewModel.RefreshAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
    }
}

// Converter para determinar se um valor é positivo ou negativo (para cores)
public class ProfitLossToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue >= 0;
        }

        if (value is double doubleValue)
        {
            return doubleValue >= 0;
        }

        if (value is float floatValue)
        {
            return floatValue >= 0;
        }

        if (value is int intValue)
        {
            return intValue >= 0;
        }

        return true; // Default para verdadeiro (verde)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
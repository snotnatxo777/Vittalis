using Vittalis.Models;
using Vittalis.Services;

namespace Vittalis.Views;

public partial class AddTradePage : ContentPage
{
    private readonly IInvestmentService _investmentService;
    private List<Asset> _assets = new();

    private Picker _assetPicker;
    private Picker _typePicker;
    private Entry _quantityEntry;
    private Entry _priceEntry;
    private Entry _feesEntry;
    private DatePicker _datePicker;
    private Entry _notesEntry;

    public AddTradePage(IInvestmentService investmentService)
    {
        _investmentService = investmentService;
        CreateUI();
        _ = LoadAssetsAsync();
    }

    private void CreateUI()
    {
        Title = "Nova Operação";

        _assetPicker = new Picker { Title = "Selecione o ativo" };
        _typePicker = new Picker
        {
            Title = "Tipo de operação",
            ItemsSource = new[] { "Compra", "Venda" }
        };
        _quantityEntry = new Entry { Placeholder = "Quantidade", Keyboard = Keyboard.Numeric };
        _priceEntry = new Entry { Placeholder = "Preço unitário", Keyboard = Keyboard.Numeric };
        _feesEntry = new Entry { Placeholder = "Taxas (opcional)", Keyboard = Keyboard.Numeric };
        _datePicker = new DatePicker { Date = DateTime.Now };
        _notesEntry = new Entry { Placeholder = "Observações (opcional)" };

        var saveButton = new Button
        {
            Text = "Salvar",
            BackgroundColor = Colors.Green,
            TextColor = Colors.White
        };
        saveButton.Clicked += OnSaveClicked;

        var cancelButton = new Button
        {
            Text = "Cancelar",
            BackgroundColor = Colors.Gray,
            TextColor = Colors.White
        };
        cancelButton.Clicked += OnCancelClicked;

        Content = new ScrollView
        {
            Content = new StackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    new Label { Text = "Nova Operação", FontSize = 24, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center },
                    new Label { Text = "Ativo:", FontSize = 16 },
                    _assetPicker,
                    new Label { Text = "Tipo:", FontSize = 16 },
                    _typePicker,
                    new Label { Text = "Quantidade:", FontSize = 16 },
                    _quantityEntry,
                    new Label { Text = "Preço:", FontSize = 16 },
                    _priceEntry,
                    new Label { Text = "Taxas:", FontSize = 16 },
                    _feesEntry,
                    new Label { Text = "Data:", FontSize = 16 },
                    _datePicker,
                    new Label { Text = "Observações:", FontSize = 16 },
                    _notesEntry,
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Center,
                        Spacing = 20,
                        Children = { saveButton, cancelButton }
                    }
                }
            }
        };
    }

    private async Task LoadAssetsAsync()
    {
        try
        {
            _assets = await _investmentService.GetAllAssetsAsync();
            var assetNames = _assets.Select(a => $"{a.Symbol} - {a.Name}").ToList();
            _assetPicker.ItemsSource = assetNames;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar ativos: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (_assetPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Selecione um ativo", "OK");
                return;
            }

            if (_typePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Selecione o tipo de operação", "OK");
                return;
            }

            if (!int.TryParse(_quantityEntry.Text, out int quantity) || quantity <= 0)
            {
                await DisplayAlert("Erro", "Informe uma quantidade válida", "OK");
                return;
            }

            if (!decimal.TryParse(_priceEntry.Text, out decimal price) || price <= 0)
            {
                await DisplayAlert("Erro", "Informe um preço válido", "OK");
                return;
            }

            decimal.TryParse(_feesEntry.Text, out decimal fees);

            var selectedAsset = _assets[_assetPicker.SelectedIndex];
            var tradeType = _typePicker.SelectedIndex == 0 ? TradeType.Buy : TradeType.Sell;

            var trade = new Trade
            {
                AssetId = selectedAsset.Id,
                Type = tradeType,
                Quantity = quantity,
                Price = price,
                Fees = fees,
                Date = _datePicker.Date,
                Notes = _notesEntry.Text
            };

            await _investmentService.AddTradeAsync(trade);
            await DisplayAlert("Sucesso", "Operação salva com sucesso!", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
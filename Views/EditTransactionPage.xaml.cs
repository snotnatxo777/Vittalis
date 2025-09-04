using Vittalis.Models;
using Vittalis.Services;

namespace Vittalis.Views;

public partial class EditTransactionPage : ContentPage
{
    private readonly ITransactionService _transactionService;
    private readonly Transaction _transaction;

    private Entry _descriptionEntry;
    private Entry _amountEntry;
    private Picker _typePicker;
    private DatePicker _datePicker;

    public EditTransactionPage(ITransactionService transactionService, Transaction transaction)
    {
        _transactionService = transactionService;
        _transaction = transaction;

        CreateUI();
        LoadTransactionData();
    }

    private void CreateUI()
    {
        Title = "Editar Transação";

        _descriptionEntry = new Entry { Placeholder = "Descrição" };
        _amountEntry = new Entry { Placeholder = "0,00", Keyboard = Keyboard.Numeric };
        _typePicker = new Picker
        {
            Title = "Selecione o tipo",
            ItemsSource = new[] { "Receita", "Despesa" }
        };
        _datePicker = new DatePicker();

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

        var buttonStack = new StackLayout
        {
            Orientation = StackOrientation.Horizontal,
            HorizontalOptions = LayoutOptions.Center,
            Spacing = 20,
            Children = { saveButton, cancelButton }
        };

        Content = new ScrollView
        {
            Content = new StackLayout
            {
                Padding = 20,
                Spacing = 20,
                Children =
                {
                    new Label { Text = "Editar Transação", FontSize = 24, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center },
                    new Label { Text = "Descrição:", FontSize = 16 },
                    _descriptionEntry,
                    new Label { Text = "Valor:", FontSize = 16 },
                    _amountEntry,
                    new Label { Text = "Tipo:", FontSize = 16 },
                    _typePicker,
                    new Label { Text = "Data:", FontSize = 16 },
                    _datePicker,
                    buttonStack
                }
            }
        };
    }

    private void LoadTransactionData()
    {
        _descriptionEntry.Text = _transaction.Description;
        _amountEntry.Text = _transaction.Amount.ToString();
        _typePicker.SelectedIndex = _transaction.Type == TransactionType.Income ? 0 : 1;
        _datePicker.Date = _transaction.Date;
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_descriptionEntry.Text))
            {
                await DisplayAlert("Erro", "Por favor, informe a descrição", "OK");
                return;
            }

            if (!decimal.TryParse(_amountEntry.Text, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("Erro", "Por favor, informe um valor válido", "OK");
                return;
            }

            if (_typePicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione o tipo", "OK");
                return;
            }

            _transaction.Description = _descriptionEntry.Text.Trim();
            _transaction.Amount = amount;
            _transaction.Date = _datePicker.Date;
            _transaction.Type = _typePicker.SelectedIndex == 0 ? TransactionType.Income : TransactionType.Expense;

            await _transactionService.UpdateTransactionAsync(_transaction);

            await DisplayAlert("Sucesso", "Transação atualizada com sucesso!", "OK");
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
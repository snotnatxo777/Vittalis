using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class AddTransactionPage : ContentPage
{
    private readonly ITransactionService _transactionService;
    private readonly IAccountService _accountService;
    private List<Account> _accounts = new();

    private Entry _descriptionEntry;
    private Entry _amountEntry;
    private Picker _typePicker;
    private Picker _categoryPicker;
    private Picker _accountPicker;
    private DatePicker _datePicker;

    public AddTransactionPage(ITransactionService transactionService, IAccountService accountService)
    {
        _transactionService = transactionService;
        _accountService = accountService;
        CreateUI();
        _ = LoadAccountsAsync();
    }

    private void CreateUI()
    {
        Title = "Nova Transação";

        _descriptionEntry = new Entry { Placeholder = "Ex: Supermercado, Gasolina..." };
        _amountEntry = new Entry { Placeholder = "0,00", Keyboard = Keyboard.Numeric };

        _typePicker = new Picker
        {
            Title = "Selecione o tipo",
            ItemsSource = new[] { "Receita", "Despesa" }
        };
        _typePicker.SelectedIndexChanged += OnTypeChanged;

        _categoryPicker = new Picker
        {
            Title = "Selecione a categoria"
        };

        _accountPicker = new Picker
        {
            Title = "Selecione a conta"
        };

        _datePicker = new DatePicker { Date = DateTime.Now };

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
                    new Label { Text = "Nova Transação", FontSize = 24, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center },
                    new Label { Text = "Descrição:", FontSize = 16 },
                    _descriptionEntry,
                    new Label { Text = "Valor:", FontSize = 16 },
                    _amountEntry,
                    new Label { Text = "Tipo:", FontSize = 16 },
                    _typePicker,
                    new Label { Text = "Categoria:", FontSize = 16 },
                    _categoryPicker,
                    new Label { Text = "Conta:", FontSize = 16 },
                    _accountPicker,
                    new Label { Text = "Data:", FontSize = 16 },
                    _datePicker,
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

    private async Task LoadAccountsAsync()
    {
        try
        {
            _accounts = await _accountService.GetAllAccountsAsync();
            var accountNames = _accounts.Select(a => $"{AccountHelper.GetAccountTypeIcon(a.Type)} {a.Name}").ToList();
            _accountPicker.ItemsSource = accountNames;

            // Selecionar primeira conta por padrão
            if (_accounts.Any())
            {
                _accountPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar contas: {ex.Message}", "OK");
        }
    }

    private void OnTypeChanged(object sender, EventArgs e)
    {
        if (_typePicker.SelectedIndex != -1)
        {
            var selectedType = _typePicker.SelectedIndex == 0 ? TransactionType.Income : TransactionType.Expense;
            var categories = CategoryHelper.GetCategoriesByType(selectedType);
            var categoryNames = categories.Select(c => CategoryHelper.GetCategoryName(c)).ToList();

            _categoryPicker.ItemsSource = categoryNames;
            _categoryPicker.SelectedIndex = -1;
        }
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

            if (_categoryPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione a categoria", "OK");
                return;
            }

            if (_accountPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione a conta", "OK");
                return;
            }

            var transactionType = _typePicker.SelectedIndex == 0 ? TransactionType.Income : TransactionType.Expense;
            var categories = CategoryHelper.GetCategoriesByType(transactionType);
            var selectedCategory = categories[_categoryPicker.SelectedIndex];
            var selectedAccount = _accounts[_accountPicker.SelectedIndex];

            var transaction = new Transaction
            {
                Description = _descriptionEntry.Text.Trim(),
                Amount = amount,
                Date = _datePicker.Date,
                Type = transactionType,
                Category = selectedCategory,
                AccountId = selectedAccount.Id
            };

            await _transactionService.AddTransactionAsync(transaction);

            await DisplayAlert("Sucesso", "Transação salva com sucesso!", "OK");
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
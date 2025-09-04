using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class AddRecurringTransactionPage : ContentPage
{
    private readonly IRecurringTransactionService _recurringTransactionService;
    private readonly IAccountService _accountService;
    private List<Account> _accounts = new();

    private Entry _descriptionEntry;
    private Entry _amountEntry;
    private Picker _typePicker;
    private Picker _categoryPicker;
    private Picker _accountPicker;
    private Picker _frequencyPicker;
    private DatePicker _startDatePicker;
    private DatePicker _endDatePicker;
    private CheckBox _hasEndDateCheckBox;

    public AddRecurringTransactionPage(IRecurringTransactionService recurringTransactionService, IAccountService accountService)
    {
        _recurringTransactionService = recurringTransactionService;
        _accountService = accountService;
        CreateUI();
        _ = LoadAccountsAsync();
    }

    private void CreateUI()
    {
        Title = "Nova Transação Recorrente";

        _descriptionEntry = new Entry { Placeholder = "Ex: Salário, Aluguel, Netflix..." };
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

        _frequencyPicker = new Picker
        {
            Title = "Selecione a frequência",
            ItemsSource = new[] { "Semanal", "Mensal", "Trimestral", "Anual" }
        };

        _startDatePicker = new DatePicker { Date = DateTime.Now };
        _endDatePicker = new DatePicker { Date = DateTime.Now.AddYears(1), IsEnabled = false };

        _hasEndDateCheckBox = new CheckBox();
        _hasEndDateCheckBox.CheckedChanged += OnEndDateCheckChanged;

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
                    new Label { Text = "Nova Transação Recorrente", FontSize = 24, FontAttributes = FontAttributes.Bold, HorizontalOptions = LayoutOptions.Center },

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

                    new Label { Text = "Frequência:", FontSize = 16 },
                    _frequencyPicker,

                    new Label { Text = "Data de Início:", FontSize = 16 },
                    _startDatePicker,

                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            _hasEndDateCheckBox,
                            new Label { Text = "Definir data de fim", VerticalOptions = LayoutOptions.Center }
                        }
                    },

                    new Label { Text = "Data de Fim:", FontSize = 16 },
                    _endDatePicker,

                    new Frame
                    {
                        BackgroundColor = Colors.LightBlue,
                        CornerRadius = 5,
                        Padding = 10,
                        Content = new Label
                        {
                            Text = "💡 Dica: Transações recorrentes são processadas automaticamente nas datas programadas. Você pode gerenciá-las na lista de recorrências.",
                            FontSize = 12,
                            TextColor = Colors.DarkBlue
                        }
                    },

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

    private void OnEndDateCheckChanged(object sender, CheckedChangedEventArgs e)
    {
        _endDatePicker.IsEnabled = e.Value;
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

            if (_frequencyPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione a frequência", "OK");
                return;
            }

            var transactionType = _typePicker.SelectedIndex == 0 ? TransactionType.Income : TransactionType.Expense;
            var categories = CategoryHelper.GetCategoriesByType(transactionType);
            var selectedCategory = categories[_categoryPicker.SelectedIndex];
            var selectedAccount = _accounts[_accountPicker.SelectedIndex];

            var frequency = _frequencyPicker.SelectedIndex switch
            {
                0 => RecurrenceFrequency.Weekly,
                1 => RecurrenceFrequency.Monthly,
                2 => RecurrenceFrequency.Quarterly,
                3 => RecurrenceFrequency.Yearly,
                _ => RecurrenceFrequency.Monthly
            };

            var recurringTransaction = new RecurringTransaction
            {
                Description = _descriptionEntry.Text.Trim(),
                Amount = amount,
                Type = transactionType,
                Category = selectedCategory,
                AccountId = selectedAccount.Id,
                Frequency = frequency,
                StartDate = _startDatePicker.Date,
                EndDate = _hasEndDateCheckBox.IsChecked ? _endDatePicker.Date : null,
                IsActive = true
            };

            await _recurringTransactionService.AddRecurringTransactionAsync(recurringTransaction);

            await DisplayAlert("Sucesso", "Transação recorrente criada com sucesso!", "OK");
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
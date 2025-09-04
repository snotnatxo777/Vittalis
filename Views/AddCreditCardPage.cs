using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class AddCreditCardTransactionPage : ContentPage
{
    private readonly ICreditCardService _creditCardService;
    private List<CreditCard> _creditCards = new();

    private Entry _descriptionEntry;
    private Entry _amountEntry;
    private Picker _creditCardPicker;
    private Picker _categoryPicker;
    private DatePicker _datePicker;
    private CheckBox _isInstallmentCheckBox;
    private Entry _installmentsEntry;

    public AddCreditCardTransactionPage(ICreditCardService creditCardService)
    {
        _creditCardService = creditCardService;
        CreateUI();
        _ = LoadCreditCardsAsync();
    }

    private void CreateUI()
    {
        Title = "Nova Compra no Cartão";

        _descriptionEntry = new Entry { Placeholder = "Ex: Supermercado, Combustível..." };
        _amountEntry = new Entry { Placeholder = "0,00", Keyboard = Keyboard.Numeric };

        _creditCardPicker = new Picker { Title = "Selecione o cartão" };

        _categoryPicker = new Picker
        {
            Title = "Selecione a categoria"
        };

        _datePicker = new DatePicker { Date = DateTime.Now };

        _isInstallmentCheckBox = new CheckBox();
        _isInstallmentCheckBox.CheckedChanged += OnInstallmentCheckChanged;

        _installmentsEntry = new Entry
        {
            Placeholder = "2",
            Keyboard = Keyboard.Numeric,
            IsEnabled = false
        };

        var saveButton = new Button
        {
            Text = "Registrar Compra",
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
                    new Label
                    {
                        Text = "Nova Compra no Cartão",
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },

                    new Label { Text = "Descrição:", FontSize = 16 },
                    _descriptionEntry,

                    new Label { Text = "Valor:", FontSize = 16 },
                    _amountEntry,

                    new Label { Text = "Cartão:", FontSize = 16 },
                    _creditCardPicker,

                    new Label { Text = "Categoria:", FontSize = 16 },
                    _categoryPicker,

                    new Label { Text = "Data da Compra:", FontSize = 16 },
                    _datePicker,

                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            _isInstallmentCheckBox,
                            new Label
                            {
                                Text = "Compra parcelada",
                                VerticalOptions = LayoutOptions.Center
                            }
                        }
                    },

                    new Label { Text = "Número de Parcelas:", FontSize = 16 },
                    _installmentsEntry,

                    new Frame
                    {
                        BackgroundColor = Colors.LightBlue,
                        CornerRadius = 5,
                        Padding = 10,
                        Content = new StackLayout
                        {
                            Children =
                            {
                                new Label
                                {
                                    Text = "Como funciona:",
                                    FontAttributes = FontAttributes.Bold,
                                    FontSize = 14
                                },
                                new Label
                                {
                                    Text = "• A compra aparece como transação no extrato",
                                    FontSize = 12
                                },
                                new Label
                                {
                                    Text = "• Se parcelada, mostra '(2x)' na descrição",
                                    FontSize = 12
                                },
                                new Label
                                {
                                    Text = "• Pagamento da fatura é transação separada",
                                    FontSize = 12
                                }
                            }
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

        LoadExpenseCategories();
    }

    private async Task LoadCreditCardsAsync()
    {
        try
        {
            _creditCards = await _creditCardService.GetAllCreditCardsAsync();
            var cardNames = _creditCards.Select(c => $"{c.Name} (*{c.LastFourDigits})").ToList();
            _creditCardPicker.ItemsSource = cardNames;

            if (_creditCards.Any())
            {
                _creditCardPicker.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar cartões: {ex.Message}", "OK");
        }
    }

    private void LoadExpenseCategories()
    {
        var expenseCategories = CategoryHelper.GetCategoriesByType(TransactionType.Expense);
        var categoryNames = expenseCategories.Select(c => CategoryHelper.GetCategoryName(c)).ToList();
        _categoryPicker.ItemsSource = categoryNames;
    }

    private void OnInstallmentCheckChanged(object sender, CheckedChangedEventArgs e)
    {
        _installmentsEntry.IsEnabled = e.Value;
        if (!e.Value)
        {
            _installmentsEntry.Text = string.Empty;
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

            if (_creditCardPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione um cartão", "OK");
                return;
            }

            if (_categoryPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione uma categoria", "OK");
                return;
            }

            var selectedCard = _creditCards[_creditCardPicker.SelectedIndex];
            var expenseCategories = CategoryHelper.GetCategoriesByType(TransactionType.Expense);
            var selectedCategory = expenseCategories[_categoryPicker.SelectedIndex];

            // Determinar conta para a transação
            int accountId = selectedCard.LinkedAccountId ?? await GetDefaultAccountAsync();

            // Preparar descrição
            string description = _descriptionEntry.Text.Trim();
            if (_isInstallmentCheckBox.IsChecked)
            {
                if (!int.TryParse(_installmentsEntry.Text, out int installments) || installments < 2)
                {
                    await DisplayAlert("Erro", "Por favor, informe um número válido de parcelas (mínimo 2)", "OK");
                    return;
                }
                description += $" ({installments}x)";
            }

            // 1. Criar transação normal (aparece no extrato)
            var normalTransaction = new Transaction
            {
                Description = description,
                Amount = amount,
                Date = _datePicker.Date,
                Type = TransactionType.Expense,
                Category = selectedCategory,
                AccountId = accountId
            };

            var serviceProvider = Handler?.MauiContext?.Services;
            var transactionService = serviceProvider?.GetService<ITransactionService>();

            if (transactionService != null)
            {
                await transactionService.AddTransactionAsync(normalTransaction);
            }

            // 2. Registrar no controle do cartão (para limite)
            var creditCardTransaction = new CreditCardTransaction
            {
                CreditCardId = selectedCard.Id,
                Description = description,
                Amount = amount,
                TransactionDate = _datePicker.Date,
                Category = selectedCategory
            };

            await _creditCardService.AddTransactionAsync(creditCardTransaction);

            await DisplayAlert("Sucesso", "Compra registrada com sucesso!\nAparece no extrato e no controle do cartão.", "OK");
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao salvar: {ex.Message}", "OK");
        }
    }

    private async Task<int> GetDefaultAccountAsync()
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var accountService = serviceProvider?.GetService<IAccountService>();

            if (accountService != null)
            {
                var accounts = await accountService.GetAllAccountsAsync();
                return accounts.FirstOrDefault()?.Id ?? 1;
            }
        }
        catch
        {
            // Fallback to ID 1
        }
        return 1;
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
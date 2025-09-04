using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class AddCreditCardPage : ContentPage
{
    private readonly ICreditCardService _creditCardService;
    private readonly IAccountService _accountService;
    private List<Account> _accounts = new();

    private Entry _nameEntry;
    private Entry _lastFourDigitsEntry;
    private Entry _creditLimitEntry;
    private Picker _closingDayPicker;
    private Picker _dueDayPicker;
    private Picker _linkedAccountPicker;

    public AddCreditCardPage(ICreditCardService creditCardService, IAccountService accountService)
    {
        _creditCardService = creditCardService;
        _accountService = accountService;
        CreateUI();
        _ = LoadAccountsAsync();
    }

    private void CreateUI()
    {
        Title = "Novo Cartão de Crédito";

        _nameEntry = new Entry { Placeholder = "Ex: Cartão Nubank, Visa Itaú..." };
        _lastFourDigitsEntry = new Entry { Placeholder = "1234", Keyboard = Keyboard.Numeric, MaxLength = 4 };
        _creditLimitEntry = new Entry { Placeholder = "5000,00", Keyboard = Keyboard.Numeric };

        _linkedAccountPicker = new Picker { Title = "Conta vinculada (opcional)" };

        // Picker para dia de fechamento (1-31)
        var closingDays = Enumerable.Range(1, 31).Select(d => d.ToString()).ToList();
        _closingDayPicker = new Picker
        {
            Title = "Dia do fechamento",
            ItemsSource = closingDays,
            SelectedIndex = 4 // Dia 5 como padrão
        };

        // Picker para dia de vencimento (1-31)
        var dueDays = Enumerable.Range(1, 31).Select(d => d.ToString()).ToList();
        _dueDayPicker = new Picker
        {
            Title = "Dia do vencimento",
            ItemsSource = dueDays,
            SelectedIndex = 9 // Dia 10 como padrão
        };

        var saveButton = new Button
        {
            Text = "Cadastrar Cartão",
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
                        Text = "Novo Cartão de Crédito",
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },

                    new Label { Text = "Nome do Cartão:", FontSize = 16 },
                    _nameEntry,

                    new Label { Text = "Conta Bancária Vinculada:", FontSize = 16 },
                    _linkedAccountPicker,

                    new Label { Text = "Últimos 4 Dígitos:", FontSize = 16 },
                    _lastFourDigitsEntry,

                    new Label { Text = "Limite de Crédito:", FontSize = 16 },
                    _creditLimitEntry,

                    new Label { Text = "Dia do Fechamento da Fatura:", FontSize = 16 },
                    _closingDayPicker,

                    new Label { Text = "Dia do Vencimento:", FontSize = 16 },
                    _dueDayPicker,

                    new Frame
                    {
                        BackgroundColor = Colors.LightYellow,
                        CornerRadius = 5,
                        Padding = 10,
                        Content = new StackLayout
                        {
                            Children =
                            {
                                new Label
                                {
                                    Text = "Dicas:",
                                    FontAttributes = FontAttributes.Bold,
                                    FontSize = 14
                                },
                                new Label
                                {
                                    Text = "• Vincule a uma conta para facilitar pagamentos",
                                    FontSize = 12
                                },
                                new Label
                                {
                                    Text = "• Fechamento: dia que a fatura é calculada",
                                    FontSize = 12
                                },
                                new Label
                                {
                                    Text = "• Vencimento: prazo para pagamento",
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
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            _accounts = await _accountService.GetAllAccountsAsync();
            var accountOptions = new List<string> { "Nenhuma conta vinculada" };

            foreach (var account in _accounts)
            {
                var icon = AccountHelper.GetAccountTypeIcon(account.Type);
                accountOptions.Add($"{icon} {account.Name}");
            }

            _linkedAccountPicker.ItemsSource = accountOptions;
            _linkedAccountPicker.SelectedIndex = 0;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar contas: {ex.Message}", "OK");
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(_nameEntry.Text))
            {
                await DisplayAlert("Erro", "Por favor, informe o nome do cartão", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(_lastFourDigitsEntry.Text) || _lastFourDigitsEntry.Text.Length != 4)
            {
                await DisplayAlert("Erro", "Por favor, informe os últimos 4 dígitos", "OK");
                return;
            }

            if (!decimal.TryParse(_creditLimitEntry.Text, out decimal creditLimit) || creditLimit <= 0)
            {
                await DisplayAlert("Erro", "Por favor, informe um limite válido", "OK");
                return;
            }

            if (_closingDayPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione o dia de fechamento", "OK");
                return;
            }

            if (_dueDayPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione o dia de vencimento", "OK");
                return;
            }

            var closingDay = _closingDayPicker.SelectedIndex + 1;
            var dueDay = _dueDayPicker.SelectedIndex + 1;

            int? linkedAccountId = null;
            if (_linkedAccountPicker.SelectedIndex > 0) // 0 = "Nenhuma conta vinculada"
            {
                linkedAccountId = _accounts[_linkedAccountPicker.SelectedIndex - 1].Id;
            }

            var creditCard = new CreditCard
            {
                Name = _nameEntry.Text.Trim(),
                LastFourDigits = _lastFourDigitsEntry.Text.Trim(),
                CreditLimit = creditLimit,
                ClosingDay = closingDay,
                DueDay = dueDay,
                LinkedAccountId = linkedAccountId,
                IsActive = true
            };

            await _creditCardService.AddCreditCardAsync(creditCard);

            await DisplayAlert("Sucesso", "Cartão cadastrado com sucesso!", "OK");
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
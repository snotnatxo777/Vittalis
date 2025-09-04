using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class AddSpendingGoalPage : ContentPage
{
    private readonly ISpendingGoalService _spendingGoalService;
    private readonly IAccountService _accountService;
    private List<Account> _accounts = new();
    private DateTime _selectedDate = DateTime.Now;

    private Picker _categoryPicker;
    private Entry _limitEntry;
    private Picker _accountPicker;
    private CheckBox _specificAccountCheckBox;
    private Label _periodLabel;

    public AddSpendingGoalPage(ISpendingGoalService spendingGoalService, IAccountService accountService)
    {
        _spendingGoalService = spendingGoalService;
        _accountService = accountService;
        CreateUI();
        _ = LoadAccountsAsync();
    }

    public void SetSelectedDate(DateTime date)
    {
        _selectedDate = date;
        _periodLabel.Text = $"Período: {date:MMMM yyyy}";
    }

    private void CreateUI()
    {
        Title = "Nova Meta de Gastos";

        _categoryPicker = new Picker
        {
            Title = "Selecione a categoria"
        };

        _limitEntry = new Entry
        {
            Placeholder = "Ex: 500,00",
            Keyboard = Keyboard.Numeric
        };

        _accountPicker = new Picker
        {
            Title = "Selecione a conta",
            IsEnabled = false
        };

        _specificAccountCheckBox = new CheckBox();
        _specificAccountCheckBox.CheckedChanged += OnSpecificAccountChanged;

        _periodLabel = new Label
        {
            Text = $"Período: {_selectedDate:MMMM yyyy}",
            FontSize = 16,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        };

        var saveButton = new Button
        {
            Text = "Salvar Meta",
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
                        Text = "Nova Meta de Gastos",
                        FontSize = 24,
                        FontAttributes = FontAttributes.Bold,
                        HorizontalOptions = LayoutOptions.Center
                    },

                    _periodLabel,

                    new Label { Text = "Categoria:", FontSize = 16 },
                    _categoryPicker,

                    new Label { Text = "Limite Mensal:", FontSize = 16 },
                    _limitEntry,

                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        Children =
                        {
                            _specificAccountCheckBox,
                            new Label
                            {
                                Text = "Meta específica para uma conta",
                                VerticalOptions = LayoutOptions.Center
                            }
                        }
                    },

                    new Label { Text = "Conta (opcional):", FontSize = 16 },
                    _accountPicker,

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
                                    Text = "Como funcionam as metas:",
                                    FontAttributes = FontAttributes.Bold,
                                    FontSize = 14
                                },
                                new Label
                                {
                                    Text = "• Defina um limite mensal para cada categoria",
                                    FontSize = 12
                                },
                                new Label
                                {
                                    Text = "• Acompanhe seu progresso em tempo real",
                                    FontSize = 12
                                },
                                new Label
                                {
                                    Text = "• Receba alertas quando estiver próximo do limite",
                                    FontSize = 12
                                },
                                new Label
                                {
                                    Text = "• Opcionalmente, limite apenas uma conta específica",
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

    private void LoadExpenseCategories()
    {
        // Carregar apenas categorias de despesa
        var expenseCategories = CategoryHelper.GetCategoriesByType(TransactionType.Expense);
        var categoryNames = expenseCategories.Select(c => CategoryHelper.GetCategoryName(c)).ToList();
        _categoryPicker.ItemsSource = categoryNames;
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            _accounts = await _accountService.GetAllAccountsAsync();
            var accountNames = _accounts.Select(a => $"{AccountHelper.GetAccountTypeIcon(a.Type)} {a.Name}").ToList();
            _accountPicker.ItemsSource = accountNames;
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar contas: {ex.Message}", "OK");
        }
    }

    private void OnSpecificAccountChanged(object sender, CheckedChangedEventArgs e)
    {
        _accountPicker.IsEnabled = e.Value;
        if (!e.Value)
        {
            _accountPicker.SelectedIndex = -1;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        try
        {
            if (_categoryPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione uma categoria", "OK");
                return;
            }

            if (!decimal.TryParse(_limitEntry.Text, out decimal limit) || limit <= 0)
            {
                await DisplayAlert("Erro", "Por favor, informe um limite válido", "OK");
                return;
            }

            if (_specificAccountCheckBox.IsChecked && _accountPicker.SelectedIndex == -1)
            {
                await DisplayAlert("Erro", "Por favor, selecione uma conta ou desmarque a opção", "OK");
                return;
            }

            var expenseCategories = CategoryHelper.GetCategoriesByType(TransactionType.Expense);
            var selectedCategory = expenseCategories[_categoryPicker.SelectedIndex];

            int? accountId = null;
            if (_specificAccountCheckBox.IsChecked && _accountPicker.SelectedIndex >= 0)
            {
                accountId = _accounts[_accountPicker.SelectedIndex].Id;
            }

            // Verificar se já existe uma meta para esta categoria/período/conta
            var existingGoal = await _spendingGoalService.GetSpendingGoalAsync(
                selectedCategory, _selectedDate.Year, _selectedDate.Month, accountId);

            if (existingGoal != null)
            {
                await DisplayAlert("Aviso", "Já existe uma meta para esta categoria no período selecionado", "OK");
                return;
            }

            var spendingGoal = new SpendingGoal
            {
                Category = selectedCategory,
                MonthlyLimit = limit,
                Year = _selectedDate.Year,
                Month = _selectedDate.Month,
                AccountId = accountId,
                IsActive = true
            };

            await _spendingGoalService.AddSpendingGoalAsync(spendingGoal);

            await DisplayAlert("Sucesso", "Meta criada com sucesso!", "OK");
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
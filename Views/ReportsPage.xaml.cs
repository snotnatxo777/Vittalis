using Microcharts;
using SkiaSharp;
using Vittalis.ViewModels;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class ReportsPage : ContentPage
{
    private readonly ReportsViewModel _viewModel;

    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Subscribir para atualizar gráficos quando dados mudarem
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (!_viewModel.IsLoading &&
            (e.PropertyName == nameof(ReportsViewModel.ExpensesByCategory) ||
             e.PropertyName == nameof(ReportsViewModel.IncomesByCategory) ||
             e.PropertyName == nameof(ReportsViewModel.IsLoading)))
        {
            UpdateCharts();
        }
    }

    private void UpdateCharts()
    {
        try
        {
            // Atualizar gráfico de despesas
            if (_viewModel.ExpensesByCategory?.Any() == true)
            {
                var expenseEntries = _viewModel.ExpensesByCategory
                    .Take(8) // Limitar a 8 categorias para melhor visualização
                    .Select((item, index) => new ChartEntry((float)item.Amount)
                    {
                        Label = item.CategoryName.Length > 10
                            ? item.CategoryName.Substring(0, 10) + "..."
                            : item.CategoryName,
                        ValueLabel = item.Amount.ToString("C0"),
                        Color = GetExpenseColorForIndex(index)
                    }).ToArray();

                ExpenseChart.Chart = new DonutChart
                {
                    Entries = expenseEntries,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 24,
                    HoleRadius = 0.3f,
                    LabelColor = SKColors.Black,
                    IsAnimated = true,
                    AnimationDuration = TimeSpan.FromSeconds(1.2)
                };
            }

            // Atualizar gráfico de receitas
            if (_viewModel.IncomesByCategory?.Any() == true)
            {
                var incomeEntries = _viewModel.IncomesByCategory
                    .Take(6) // Limitar a 6 categorias para receitas
                    .Select((item, index) => new ChartEntry((float)item.Amount)
                    {
                        Label = item.CategoryName.Length > 10
                            ? item.CategoryName.Substring(0, 10) + "..."
                            : item.CategoryName,
                        ValueLabel = item.Amount.ToString("C0"),
                        Color = GetIncomeColorForIndex(index)
                    }).ToArray();

                IncomeChart.Chart = new DonutChart
                {
                    Entries = incomeEntries,
                    BackgroundColor = SKColors.Transparent,
                    LabelTextSize = 24,
                    HoleRadius = 0.3f,
                    LabelColor = SKColors.Black,
                    IsAnimated = true,
                    AnimationDuration = TimeSpan.FromSeconds(1.2)
                };
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao atualizar gráficos: {ex.Message}");
        }
    }

    private SKColor GetExpenseColorForIndex(int index)
    {
        var colors = new string[]
        {
            "#FF6B6B", // Vermelho - Alimentação
            "#4ECDC4", // Teal - Transporte  
            "#45B7D1", // Azul - Moradia
            "#FFA07A", // Salmão - Saúde
            "#98D8C8", // Verde claro - Educação
            "#FFD93D", // Amarelo - Lazer
            "#FF8C94", // Rosa - Compras
            "#A8E6CF"  // Verde menta - Outros
        };

        return SKColor.Parse(colors[index % colors.Length]);
    }

    private SKColor GetIncomeColorForIndex(int index)
    {
        var colors = new string[]
        {
            "#6BCF7F", // Verde claro - Salário
            "#4D9DE0", // Azul claro - Freelance
            "#51CF66", // Verde - Investimentos
            "#FFD43B", // Amarelo claro - Presentes
            "#74C0FC", // Azul céu - Outros
            "#8CE99A"  // Verde menta - Extras
        };

        return SKColor.Parse(colors[index % colors.Length]);
    }

    // Eventos do seletor de período
    private async void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        var previousMonth = _viewModel.SelectedDate.AddMonths(-1);
        await _viewModel.ChangePeriodAsync(previousMonth);
    }

    private async void OnNextMonthClicked(object sender, EventArgs e)
    {
        var nextMonth = _viewModel.SelectedDate.AddMonths(1);

        // Não permitir ir para o futuro além do mês atual
        if (nextMonth <= DateTime.Now)
        {
            await _viewModel.ChangePeriodAsync(nextMonth);
        }
    }

    private async void OnSelectDateClicked(object sender, EventArgs e)
    {
        // Mostrar um action sheet com opções de meses recentes
        var currentDate = DateTime.Now;
        var options = new List<string>();
        var dates = new List<DateTime>();

        // Adicionar últimos 12 meses
        for (int i = 0; i < 12; i++)
        {
            var date = currentDate.AddMonths(-i);
            options.Add($"{date:MMMM yyyy}");
            dates.Add(date);
        }

        var result = await DisplayActionSheet(
            "Selecionar Período",
            "Cancelar",
            null,
            options.ToArray());

        if (!string.IsNullOrEmpty(result) && result != "Cancelar")
        {
            var selectedIndex = options.IndexOf(result);
            if (selectedIndex >= 0)
            {
                await _viewModel.ChangePeriodAsync(dates[selectedIndex]);
            }
        }
    }

    private async void OnSelectAccountClicked(object sender, EventArgs e)
    {
        if (!_viewModel.Accounts.Any())
        {
            await DisplayAlert("Aviso", "Nenhuma conta encontrada", "OK");
            return;
        }

        var accountOptions = _viewModel.Accounts
            .Select(a => $"{AccountHelper.GetAccountTypeIcon(a.Type)} {a.Name}")
            .ToArray();

        var result = await DisplayActionSheet(
            "Selecionar Conta",
            "Cancelar",
            null,
            accountOptions);

        if (!string.IsNullOrEmpty(result) && result != "Cancelar")
        {
            var selectedIndex = Array.IndexOf(accountOptions, result);
            if (selectedIndex >= 0)
            {
                var selectedAccount = _viewModel.Accounts[selectedIndex];
                await _viewModel.ChangeAccountFilterAsync(selectedAccount);
            }
        }
    }

    private async void OnClearAccountFilterClicked(object sender, EventArgs e)
    {
        await _viewModel.ChangeAccountFilterAsync(null);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync();
    }
}
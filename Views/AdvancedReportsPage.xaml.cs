using Vittalis.ViewModels;
using Vittalis.Models;
using Microcharts;
using SkiaSharp;

namespace Vittalis.Views;

public partial class AdvancedReportsPage : ContentPage
{
    private readonly AdvancedReportsViewModel _viewModel;

    public AdvancedReportsPage(AdvancedReportsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;

        // Subscrever mudanças no ViewModel para atualizar gráficos
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Atualizar gráficos quando dados mudarem
        switch (e.PropertyName)
        {
            case nameof(AdvancedReportsViewModel.FinancialProjections):
                UpdateProjectionsChart();
                break;
            case nameof(AdvancedReportsViewModel.IncomeTrends):
                UpdateIncomeTrendsChart();
                break;
            case nameof(AdvancedReportsViewModel.ExpenseTrends):
                UpdateExpenseTrendsChart();
                break;
            case nameof(AdvancedReportsViewModel.CashFlowPeriods):
                UpdateCashFlowChart();
                break;
        }
    }

    private void UpdateProjectionsChart()
    {
        if (_viewModel.FinancialProjections?.Any() != true) return;

        var entries = _viewModel.FinancialProjections
            .Take(12)
            .Select((projection, index) => new ChartEntry((float)projection.ProjectedSavings)
            {
                Label = projection.Date.ToString("MMM"),
                ValueLabel = projection.ProjectedSavings.ToString("C0"),
                Color = projection.ProjectedSavings >= 0 ? SKColor.Parse("#4CAF50") : SKColor.Parse("#F44336")
            })
            .ToArray();

        var chart = new LineChart
        {
            Entries = entries,
            LineMode = LineMode.Straight,
            LineSize = 8,
            PointMode = PointMode.Square,
            PointSize = 18,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 40,
            ValueLabelTextSize = 35
        };

        ProjectionsChart.Chart = chart;
    }

    private void UpdateIncomeTrendsChart()
    {
        if (_viewModel.IncomeTrends?.Any() != true) return;

        var entries = _viewModel.IncomeTrends
            .Take(12)
            .Select((trend, index) => new ChartEntry((float)trend.Value)
            {
                Label = trend.Period.Length > 3 ? trend.Period.Substring(0, 3) : trend.Period,
                ValueLabel = trend.Value.ToString("C0"),
                Color = GetTrendColor(trend.Direction)
            })
            .ToArray();

        var chart = new LineChart
        {
            Entries = entries,
            LineMode = LineMode.Spline,
            LineSize = 6,
            PointMode = PointMode.Circle,
            PointSize = 15,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 30,
            ValueLabelTextSize = 25
        };

        IncomeTrendsChart.Chart = chart;
    }

    private void UpdateExpenseTrendsChart()
    {
        if (_viewModel.ExpenseTrends?.Any() != true) return;

        var entries = _viewModel.ExpenseTrends
            .Take(12)
            .Select((trend, index) => new ChartEntry((float)trend.Value)
            {
                Label = trend.Period.Length > 3 ? trend.Period.Substring(0, 3) : trend.Period,
                ValueLabel = trend.Value.ToString("C0"),
                Color = GetTrendColor(trend.Direction, true) // true = é despesa
            })
            .ToArray();

        var chart = new LineChart
        {
            Entries = entries,
            LineMode = LineMode.Spline,
            LineSize = 6,
            PointMode = PointMode.Circle,
            PointSize = 15,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 30,
            ValueLabelTextSize = 25
        };

        ExpenseTrendsChart.Chart = chart;
    }

    private void UpdateCashFlowChart()
    {
        if (_viewModel.CashFlowPeriods?.Any() != true) return;

        var entries = _viewModel.CashFlowPeriods
            .Take(12)
            .Select((period, index) => new ChartEntry((float)period.AccumulatedFlow)
            {
                Label = period.PeriodName.Length > 6 ? period.PeriodName.Substring(0, 6) : period.PeriodName,
                ValueLabel = period.AccumulatedFlow.ToString("C0"),
                Color = period.AccumulatedFlow >= 0 ? SKColor.Parse("#4CAF50") : SKColor.Parse("#F44336")
            })
            .ToArray();

        // Usar LineChart ao invés de AreaChart para evitar erro
        var chart = new LineChart
        {
            Entries = entries,
            LineMode = LineMode.Straight,
            LineSize = 6,
            PointMode = PointMode.Circle,
            PointSize = 15,
            BackgroundColor = SKColors.Transparent,
            LabelTextSize = 35,
            ValueLabelTextSize = 30
        };

        CashFlowChart.Chart = chart;
    }

    private SKColor GetTrendColor(TrendDirection direction, bool isExpense = false)
    {
        if (isExpense)
        {
            // Para despesas: diminuir é bom (verde), aumentar é ruim (vermelho)
            return direction switch
            {
                TrendDirection.Up => SKColor.Parse("#F44336"), // Vermelho
                TrendDirection.Down => SKColor.Parse("#4CAF50"), // Verde
                _ => SKColor.Parse("#9E9E9E") // Cinza
            };
        }
        else
        {
            // Para receitas: aumentar é bom (verde), diminuir é ruim (vermelho)
            return direction switch
            {
                TrendDirection.Up => SKColor.Parse("#4CAF50"), // Verde
                TrendDirection.Down => SKColor.Parse("#F44336"), // Vermelho
                _ => SKColor.Parse("#9E9E9E") // Cinza
            };
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        await _viewModel.RefreshCurrentReportAsync();
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        try
        {
            var action = await DisplayActionSheet(
                "Exportar Relatório",
                "Cancelar",
                null,
                "PDF",
                "Excel",
                "CSV"
            );

            if (action != null && action != "Cancelar")
            {
                await _viewModel.ExportReportAsync(action);
                await DisplayAlert("Sucesso", $"Relatório exportado como {action}!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao exportar: {ex.Message}", "OK");
        }
    }

    private async void OnCustomReportClicked(object sender, EventArgs e)
    {
        try
        {
            // Funcionalidade em desenvolvimento - por enquanto mostrar mensagem
            await DisplayAlert("Info", "Funcionalidade de relatórios personalizados em desenvolvimento!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao abrir relatório personalizado: {ex.Message}", "OK");
        }
    }

    private async void OnScheduleClicked(object sender, EventArgs e)
    {
        try
        {
            var frequency = await DisplayActionSheet(
                "Agendar Relatório",
                "Cancelar",
                null,
                "Semanal",
                "Mensal",
                "Trimestral"
            );

            if (frequency != null && frequency != "Cancelar")
            {
                await _viewModel.ScheduleReportAsync(frequency);
                await DisplayAlert("Sucesso", $"Relatório agendado para frequência {frequency}!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao agendar: {ex.Message}", "OK");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Limpar subscriptions para evitar memory leaks
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.ViewModels
{
    public class AdvancedReportsViewModel : INotifyPropertyChanged
    {
        private readonly IAdvancedReportService _advancedReportService;
        private readonly IAccountService _accountService;
        private bool _isLoading;
        private string _selectedReportType = "FinancialHealth";
        private FinancialHealthIndicators? _healthIndicators;
        private List<Account> _accounts = new();
        private Account? _selectedAccount = null;
        private List<string> _accountDisplayNames = new();
        private string _selectedAccountName = "Todas as Contas";

        // Collections para diferentes tipos de relatórios
        public ObservableCollection<FinancialProjection> FinancialProjections { get; set; }
        public ObservableCollection<TrendAnalysis> IncomeTrends { get; set; }
        public ObservableCollection<TrendAnalysis> ExpenseTrends { get; set; }
        public ObservableCollection<CategoryAnalysis> CategoryAnalyses { get; set; }
        public ObservableCollection<CashFlowPeriod> CashFlowPeriods { get; set; }

        // Propriedades principais
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string SelectedReportType
        {
            get => _selectedReportType;
            set { _selectedReportType = value; OnPropertyChanged(); _ = LoadSelectedReportAsync(); }
        }

        public FinancialHealthIndicators? HealthIndicators
        {
            get => _healthIndicators;
            set { _healthIndicators = value; OnPropertyChanged(); }
        }

        public List<Account> Accounts
        {
            get => _accounts;
            set { _accounts = value; OnPropertyChanged(); }
        }

        public Account? SelectedAccount
        {
            get => _selectedAccount;
            set { _selectedAccount = value; OnPropertyChanged(); _ = RefreshCurrentReportAsync(); }
        }

        public List<string> AccountDisplayNames
        {
            get => _accountDisplayNames;
            set { _accountDisplayNames = value; OnPropertyChanged(); }
        }

        public string SelectedAccountName
        {
            get => _selectedAccountName;
            set
            {
                _selectedAccountName = value;
                OnPropertyChanged();

                // Converter nome selecionado de volta para Account
                if (value == "Todas as Contas")
                {
                    SelectedAccount = null;
                }
                else
                {
                    SelectedAccount = Accounts.FirstOrDefault(a => $"{AccountHelper.GetAccountTypeIcon(a.Type)} {a.Name}" == value);
                }
            }
        }

        // Propriedades para resumos rápidos
        private decimal _projectedSavings;
        public decimal ProjectedSavings
        {
            get => _projectedSavings;
            set { _projectedSavings = value; OnPropertyChanged(); }
        }

        private string _healthSummary = string.Empty;
        public string HealthSummary
        {
            get => _healthSummary;
            set { _healthSummary = value; OnPropertyChanged(); }
        }

        private string _topGrowingCategory = string.Empty;
        public string TopGrowingCategory
        {
            get => _topGrowingCategory;
            set { _topGrowingCategory = value; OnPropertyChanged(); }
        }

        private decimal _predictedCashFlow;
        public decimal PredictedCashFlow
        {
            get => _predictedCashFlow;
            set { _predictedCashFlow = value; OnPropertyChanged(); }
        }

        public AdvancedReportsViewModel(IAdvancedReportService advancedReportService, IAccountService accountService)
        {
            _advancedReportService = advancedReportService;
            _accountService = accountService;

            FinancialProjections = new ObservableCollection<FinancialProjection>();
            IncomeTrends = new ObservableCollection<TrendAnalysis>();
            ExpenseTrends = new ObservableCollection<TrendAnalysis>();
            CategoryAnalyses = new ObservableCollection<CategoryAnalysis>();
            CashFlowPeriods = new ObservableCollection<CashFlowPeriod>();

            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                IsLoading = true;

                // Carregar contas
                await LoadAccountsAsync();

                // Carregar relatório padrão (Saúde Financeira)
                await LoadSelectedReportAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados iniciais: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                Accounts = await _accountService.GetAllAccountsAsync();

                // Criar lista de nomes para exibição
                var displayNames = new List<string> { "Todas as Contas" };
                displayNames.AddRange(Accounts.Select(a => $"{AccountHelper.GetAccountTypeIcon(a.Type)} {a.Name}"));

                AccountDisplayNames = displayNames;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar contas: {ex.Message}");
            }
        }

        private async Task LoadSelectedReportAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                switch (SelectedReportType)
                {
                    case "FinancialHealth":
                        await LoadFinancialHealthAsync();
                        break;
                    case "Projections":
                        await LoadProjectionsAsync();
                        break;
                    case "Trends":
                        await LoadTrendsAsync();
                        break;
                    case "Categories":
                        await LoadCategoriesAsync();
                        break;
                    case "CashFlow":
                        await LoadCashFlowAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar relatório {SelectedReportType}: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadFinancialHealthAsync()
        {
            var accountId = SelectedAccount?.Id;
            HealthIndicators = await _advancedReportService.GetFinancialHealthAsync(accountId);

            if (HealthIndicators != null)
            {
                HealthSummary = HealthIndicators.Summary;
            }
        }

        private async Task LoadProjectionsAsync()
        {
            var accountId = SelectedAccount?.Id;
            var projections = await _advancedReportService.GetFinancialProjectionsAsync(12, accountId);

            FinancialProjections.Clear();

            // Carregar apenas projeções realistas para o gráfico principal
            var realisticProjections = projections.Where(p => p.ProjectionType == "Realistic").ToList();
            foreach (var projection in realisticProjections)
            {
                FinancialProjections.Add(projection);
            }

            // Calcular poupança projetada para o próximo ano
            ProjectedSavings = realisticProjections.Sum(p => p.ProjectedSavings);
        }

        private async Task LoadTrendsAsync()
        {
            // Carregar tendências de receitas e despesas
            var incomeTrends = await _advancedReportService.GetIncomeTrendsAsync(12);
            var expenseTrends = await _advancedReportService.GetExpenseTrendsAsync(12);

            IncomeTrends.Clear();
            ExpenseTrends.Clear();

            foreach (var trend in incomeTrends.Take(12))
            {
                IncomeTrends.Add(trend);
            }

            foreach (var trend in expenseTrends.Take(12))
            {
                ExpenseTrends.Add(trend);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            var categoryAnalyses = await _advancedReportService.GetCategoryAnalysisAsync(12, TransactionType.Expense);

            CategoryAnalyses.Clear();
            foreach (var analysis in categoryAnalyses.Take(10)) // Top 10 categorias
            {
                CategoryAnalyses.Add(analysis);
            }

            // Encontrar categoria com maior crescimento
            var topGrowing = categoryAnalyses
                .Where(c => c.GrowthRate > 0)
                .OrderByDescending(c => c.GrowthRate)
                .FirstOrDefault();

            TopGrowingCategory = topGrowing != null ?
                $"{topGrowing.CategoryName} (+{topGrowing.GrowthRate:F1}%)" :
                "Nenhuma categoria em crescimento";
        }

        private async Task LoadCashFlowAsync()
        {
            var startDate = DateTime.Now.AddMonths(-12);
            var endDate = DateTime.Now;

            var cashFlowAnalysis = await _advancedReportService.GetCashFlowAnalysisAsync(startDate, endDate);

            CashFlowPeriods.Clear();
            foreach (var period in cashFlowAnalysis.Periods)
            {
                CashFlowPeriods.Add(period);
            }

            PredictedCashFlow = cashFlowAnalysis.PredictedCashFlow;
        }

        public async Task RefreshCurrentReportAsync()
        {
            await LoadSelectedReportAsync();
        }

        public async Task ExportReportAsync(string format = "PDF")
        {
            try
            {
                IsLoading = true;

                // Implementar exportação baseada no formato
                switch (format.ToUpper())
                {
                    case "PDF":
                        await ExportToPdfAsync();
                        break;
                    case "EXCEL":
                        await ExportToExcelAsync();
                        break;
                    case "CSV":
                        await ExportToCsvAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao exportar relatório: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ExportToPdfAsync()
        {
            await Task.Delay(1000); // Simular processamento
        }

        private async Task ExportToExcelAsync()
        {
            await Task.Delay(1000); // Simular processamento
        }

        private async Task ExportToCsvAsync()
        {
            await Task.Delay(1000); // Simular processamento
        }

        public async Task ScheduleReportAsync(string frequency = "Monthly")
        {
            try
            {
                await Task.Delay(500); // Simular salvamento
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao agendar relatório: {ex.Message}");
            }
        }

        public async Task GenerateCustomAnalysisAsync(List<TransactionCategory> categories, DateTime startDate, DateTime endDate)
        {
            try
            {
                IsLoading = true;
                await Task.Delay(1000); // Simular processamento
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao gerar análise personalizada: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ComparePeriodsAsync(DateTime period1Start, DateTime period1End, DateTime period2Start, DateTime period2End)
        {
            try
            {
                IsLoading = true;
                await Task.Delay(1000); // Simular processamento
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao comparar períodos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Propriedades calculadas para dashboard rápido
        public string HealthScoreText => HealthIndicators?.OverallScore switch
        {
            FinancialHealthScore.Excellent => "Excelente",
            FinancialHealthScore.Good => "Boa",
            FinancialHealthScore.Fair => "Regular",
            FinancialHealthScore.Poor => "Ruim",
            FinancialHealthScore.Critical => "Crítica",
            _ => "N/A"
        };

        public Color HealthScoreColor => HealthIndicators?.OverallScore switch
        {
            FinancialHealthScore.Excellent => Colors.Green,
            FinancialHealthScore.Good => Colors.LimeGreen,
            FinancialHealthScore.Fair => Colors.Orange,
            FinancialHealthScore.Poor => Colors.OrangeRed,
            FinancialHealthScore.Critical => Colors.Red,
            _ => Colors.Gray
        };

        public int AlertCount => HealthIndicators?.Alerts?.Count ?? 0;

        public bool HasCriticalAlerts => HealthIndicators?.Alerts?.Any(a => a.Severity == AlertSeverity.Critical) ?? false;

        public string ProjectedSavingsText => ProjectedSavings >= 0 ?
            $"Poupança projetada: {ProjectedSavings:C}" :
            $"Déficit projetado: {Math.Abs(ProjectedSavings):C}";

        public Color ProjectedSavingsColor => ProjectedSavings >= 0 ? Colors.Green : Colors.Red;

        // Lista de tipos de relatório disponíveis
        public List<string> AvailableReportTypes => new()
        {
            "FinancialHealth",
            "Projections",
            "Trends",
            "Categories",
            "CashFlow"
        };

        public Dictionary<string, string> ReportTypeNames => new()
        {
            { "FinancialHealth", "Saúde Financeira" },
            { "Projections", "Projeções" },
            { "Trends", "Tendências" },
            { "Categories", "Análise de Categorias" },
            { "CashFlow", "Fluxo de Caixa" }
        };

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
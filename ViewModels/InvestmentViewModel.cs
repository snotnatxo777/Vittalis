using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;

namespace Vittalis.ViewModels
{
    public class InvestmentViewModel : INotifyPropertyChanged
    {
        private readonly IInvestmentService _investmentService;
        private bool _isLoading;
        private string _selectedTab = "Portfolio";
        private PortfolioSummary? _portfolioSummary;
        private string _searchText = string.Empty;

        // Collections para diferentes abas
        public ObservableCollection<PortfolioPosition> Portfolio { get; set; }
        public ObservableCollection<Trade> RecentTrades { get; set; }
        public ObservableCollection<Asset> Assets { get; set; }
        public ObservableCollection<Dividend> Dividends { get; set; }
        public ObservableCollection<InvestmentGoal> InvestmentGoals { get; set; }
        public ObservableCollection<WatchList> WatchLists { get; set; }
        public ObservableCollection<PriceAlert> PriceAlerts { get; set; }
        public ObservableCollection<Broker> Brokers { get; set; }

        // Propriedades principais
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(); _ = LoadTabDataAsync(); }
        }

        public PortfolioSummary? PortfolioSummary
        {
            get => _portfolioSummary;
            set { _portfolioSummary = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); _ = SearchAssetsAsync(); }
        }

        // Propriedades calculadas para resumo rápido
        public decimal TotalPortfolioValue => PortfolioSummary?.CurrentValue ?? 0;
        public decimal TotalInvested => PortfolioSummary?.TotalInvested ?? 0;
        public decimal TotalProfitLoss => PortfolioSummary?.TotalProfitLoss ?? 0;
        public decimal TotalDividends => PortfolioSummary?.TotalDividends ?? 0;
        public decimal TotalReturn => PortfolioSummary?.TotalReturn ?? 0;
        public decimal TotalReturnPercentage => PortfolioSummary?.TotalReturnPercentage ?? 0;

        public string PerformanceText => TotalReturnPercentage >= 0 ?
            $"+{TotalReturnPercentage:F2}%" :
            $"{TotalReturnPercentage:F2}%";

        public Color PerformanceColor => TotalReturnPercentage >= 0 ? Colors.Green : Colors.Red;

        // Diversificação
        public Dictionary<AssetType, decimal> AllocationByType => PortfolioSummary?.AllocationByType ?? new();
        public Dictionary<string, decimal> AllocationBySector => PortfolioSummary?.AllocationBySector ?? new();

        // Lista de abas disponíveis
        public List<string> AvailableTabs => new()
        {
            "Portfolio",
            "Assets",
            "Trades",
            "Dividends",
            "Goals",
            "WatchLists",
            "Alerts",
            "Brokers"
        };

        public Dictionary<string, string> TabNames => new()
        {
            { "Portfolio", "📊 Portfolio" },
            { "Assets", "📈 Ativos" },
            { "Trades", "💹 Operações" },
            { "Dividends", "💰 Dividendos" },
            { "Goals", "🎯 Metas" },
            { "WatchLists", "👁️ Watch Lists" },
            { "Alerts", "🔔 Alertas" },
            { "Brokers", "🏢 Corretoras" }
        };

        public InvestmentViewModel(IInvestmentService investmentService)
        {
            _investmentService = investmentService;

            Portfolio = new ObservableCollection<PortfolioPosition>();
            RecentTrades = new ObservableCollection<Trade>();
            Assets = new ObservableCollection<Asset>();
            Dividends = new ObservableCollection<Dividend>();
            InvestmentGoals = new ObservableCollection<InvestmentGoal>();
            WatchLists = new ObservableCollection<WatchList>();
            PriceAlerts = new ObservableCollection<PriceAlert>();
            Brokers = new ObservableCollection<Broker>();

            _ = LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            try
            {
                IsLoading = true;

                // Carregar resumo do portfolio
                await LoadPortfolioSummaryAsync();

                // Carregar dados da aba atual
                await LoadTabDataAsync();
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

        private async Task LoadPortfolioSummaryAsync()
        {
            try
            {
                PortfolioSummary = await _investmentService.GetPortfolioSummaryAsync();

                // Notificar mudanças nas propriedades calculadas
                OnPropertyChanged(nameof(TotalPortfolioValue));
                OnPropertyChanged(nameof(TotalInvested));
                OnPropertyChanged(nameof(TotalProfitLoss));
                OnPropertyChanged(nameof(TotalDividends));
                OnPropertyChanged(nameof(TotalReturn));
                OnPropertyChanged(nameof(TotalReturnPercentage));
                OnPropertyChanged(nameof(PerformanceText));
                OnPropertyChanged(nameof(PerformanceColor));
                OnPropertyChanged(nameof(AllocationByType));
                OnPropertyChanged(nameof(AllocationBySector));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar resumo do portfolio: {ex.Message}");
            }
        }

        private async Task LoadTabDataAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;

                switch (SelectedTab)
                {
                    case "Portfolio":
                        await LoadPortfolioAsync();
                        break;
                    case "Assets":
                        await LoadAssetsAsync();
                        break;
                    case "Trades":
                        await LoadTradesAsync();
                        break;
                    case "Dividends":
                        await LoadDividendsAsync();
                        break;
                    case "Goals":
                        await LoadInvestmentGoalsAsync();
                        break;
                    case "WatchLists":
                        await LoadWatchListsAsync();
                        break;
                    case "Alerts":
                        await LoadPriceAlertsAsync();
                        break;
                    case "Brokers":
                        await LoadBrokersAsync();
                        break;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados da aba {SelectedTab}: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadPortfolioAsync()
        {
            var portfolio = await _investmentService.GetPortfolioAsync();

            Portfolio.Clear();
            foreach (var position in portfolio)
            {
                Portfolio.Add(position);
            }
        }

        private async Task LoadAssetsAsync()
        {
            var assets = await _investmentService.GetAllAssetsAsync();

            Assets.Clear();
            foreach (var asset in assets)
            {
                Assets.Add(asset);
            }
        }

        private async Task LoadTradesAsync()
        {
            var trades = await _investmentService.GetAllTradesAsync();

            RecentTrades.Clear();
            foreach (var trade in trades.Take(50)) // Últimas 50 operações
            {
                RecentTrades.Add(trade);
            }
        }

        private async Task LoadDividendsAsync()
        {
            var dividends = await _investmentService.GetDividendsAsync();

            Dividends.Clear();
            foreach (var dividend in dividends.Take(100)) // Últimos 100 dividendos
            {
                Dividends.Add(dividend);
            }
        }

        private async Task LoadInvestmentGoalsAsync()
        {
            var goals = await _investmentService.GetInvestmentGoalsAsync();

            InvestmentGoals.Clear();
            foreach (var goal in goals)
            {
                InvestmentGoals.Add(goal);
            }
        }

        private async Task LoadWatchListsAsync()
        {
            var watchLists = await _investmentService.GetWatchListsAsync();

            WatchLists.Clear();
            foreach (var watchList in watchLists)
            {
                WatchLists.Add(watchList);
            }
        }

        private async Task LoadPriceAlertsAsync()
        {
            var alerts = await _investmentService.GetPriceAlertsAsync();

            PriceAlerts.Clear();
            foreach (var alert in alerts)
            {
                PriceAlerts.Add(alert);
            }
        }

        private async Task LoadBrokersAsync()
        {
            var brokers = await _investmentService.GetAllBrokersAsync();

            Brokers.Clear();
            foreach (var broker in brokers)
            {
                Brokers.Add(broker);
            }
        }

        private async Task SearchAssetsAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadAssetsAsync();
                return;
            }

            try
            {
                var searchResults = await _investmentService.SearchAssetsAsync(SearchText);

                Assets.Clear();
                foreach (var asset in searchResults)
                {
                    Assets.Add(asset);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar ativos: {ex.Message}");
            }
        }

        // Métodos para adicionar/editar/excluir

        public async Task AddAssetAsync(Asset asset)
        {
            try
            {
                await _investmentService.AddAssetAsync(asset);

                if (SelectedTab == "Assets")
                {
                    await LoadAssetsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar ativo: {ex.Message}");
            }
        }

        public async Task AddTradeAsync(Trade trade)
        {
            try
            {
                await _investmentService.AddTradeAsync(trade);

                // Atualizar dados relevantes
                await LoadPortfolioSummaryAsync();

                if (SelectedTab == "Trades")
                {
                    await LoadTradesAsync();
                }
                else if (SelectedTab == "Portfolio")
                {
                    await LoadPortfolioAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar operação: {ex.Message}");
            }
        }

        public async Task DeleteTradeAsync(int tradeId)
        {
            try
            {
                await _investmentService.DeleteTradeAsync(tradeId);

                // Atualizar dados relevantes
                await LoadPortfolioSummaryAsync();
                await RefreshCurrentTabAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir operação: {ex.Message}");
            }
        }

        public async Task AddDividendAsync(Dividend dividend)
        {
            try
            {
                await _investmentService.AddDividendAsync(dividend);

                await LoadPortfolioSummaryAsync();

                if (SelectedTab == "Dividends")
                {
                    await LoadDividendsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar dividendo: {ex.Message}");
            }
        }

        public async Task AddInvestmentGoalAsync(InvestmentGoal goal)
        {
            try
            {
                await _investmentService.AddInvestmentGoalAsync(goal);

                if (SelectedTab == "Goals")
                {
                    await LoadInvestmentGoalsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar meta: {ex.Message}");
            }
        }

        public async Task DeleteInvestmentGoalAsync(int goalId)
        {
            try
            {
                await _investmentService.DeleteInvestmentGoalAsync(goalId);
                await RefreshCurrentTabAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir meta: {ex.Message}");
            }
        }

        public async Task AddWatchListAsync(WatchList watchList)
        {
            try
            {
                await _investmentService.AddWatchListAsync(watchList);

                if (SelectedTab == "WatchLists")
                {
                    await LoadWatchListsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar watch list: {ex.Message}");
            }
        }

        public async Task DeleteWatchListAsync(int watchListId)
        {
            try
            {
                await _investmentService.DeleteWatchListAsync(watchListId);
                await RefreshCurrentTabAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir watch list: {ex.Message}");
            }
        }

        public async Task AddPriceAlertAsync(PriceAlert alert)
        {
            try
            {
                await _investmentService.AddPriceAlertAsync(alert);

                if (SelectedTab == "Alerts")
                {
                    await LoadPriceAlertsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar alerta: {ex.Message}");
            }
        }

        public async Task DeletePriceAlertAsync(int alertId)
        {
            try
            {
                await _investmentService.DeletePriceAlertAsync(alertId);
                await RefreshCurrentTabAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir alerta: {ex.Message}");
            }
        }

        public async Task AddBrokerAsync(Broker broker)
        {
            try
            {
                await _investmentService.AddBrokerAsync(broker);

                if (SelectedTab == "Brokers")
                {
                    await LoadBrokersAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar corretora: {ex.Message}");
            }
        }

        public async Task DeleteBrokerAsync(int brokerId)
        {
            try
            {
                await _investmentService.DeleteBrokerAsync(brokerId);
                await RefreshCurrentTabAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir corretora: {ex.Message}");
            }
        }

        // Métodos utilitários

        public async Task RefreshCurrentTabAsync()
        {
            await LoadTabDataAsync();
        }

        public async Task RefreshAllDataAsync()
        {
            await LoadPortfolioSummaryAsync();
            await LoadTabDataAsync();
        }

        public async Task UpdatePricesAsync()
        {
            try
            {
                IsLoading = true;
                await _investmentService.UpdateAssetPricesAsync();
                await RefreshAllDataAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao atualizar preços: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task CheckAlertsAsync()
        {
            try
            {
                await _investmentService.CheckPriceAlertsAsync();

                if (SelectedTab == "Alerts")
                {
                    await LoadPriceAlertsAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao verificar alertas: {ex.Message}");
            }
        }

        // Análises rápidas

        public async Task<List<PortfolioPosition>> GetTopPerformersAsync(int count = 5)
        {
            return await _investmentService.GetTopPerformersAsync(count);
        }

        public async Task<List<PortfolioPosition>> GetWorstPerformersAsync(int count = 5)
        {
            return await _investmentService.GetWorstPerformersAsync(count);
        }

        public async Task<Dictionary<AssetType, decimal>> GetAllocationByTypeAsync()
        {
            return await _investmentService.GetAllocationByTypeAsync();
        }

        public async Task<Dictionary<string, decimal>> GetAllocationBySectorAsync()
        {
            return await _investmentService.GetAllocationBySectorAsync();
        }

        // Exportação e relatórios

        public async Task ExportPortfolioAsync(string format = "PDF")
        {
            try
            {
                IsLoading = true;

                // TODO: Implementar exportação
                await Task.Delay(1000); // Simular processamento
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao exportar portfolio: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task GenerateTaxReportAsync(int year)
        {
            try
            {
                IsLoading = true;
                var taxReport = await _investmentService.GenerateTaxReportAsync(year);

                // TODO: Exibir relatório ou exportar
                await Task.Delay(1000);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao gerar relatório de impostos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
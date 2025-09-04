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
        private decimal _totalPortfolioValue;
        private decimal _totalInvested;
        private decimal _totalProfitLoss;

        public ObservableCollection<Portfolio> Portfolio { get; set; }
        public ObservableCollection<Trade> RecentTrades { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public decimal TotalPortfolioValue
        {
            get => _totalPortfolioValue;
            set { _totalPortfolioValue = value; OnPropertyChanged(); }
        }

        public decimal TotalInvested
        {
            get => _totalInvested;
            set { _totalInvested = value; OnPropertyChanged(); }
        }

        public decimal TotalProfitLoss
        {
            get => _totalProfitLoss;
            set { _totalProfitLoss = value; OnPropertyChanged(); }
        }

        public decimal TotalProfitLossPercentage => TotalInvested > 0 ? (TotalProfitLoss / TotalInvested) * 100 : 0;

        public InvestmentViewModel(IInvestmentService investmentService)
        {
            _investmentService = investmentService;
            Portfolio = new ObservableCollection<Portfolio>();
            RecentTrades = new ObservableCollection<Trade>();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var portfolio = await _investmentService.GetPortfolioAsync();
                var trades = await _investmentService.GetAllTradesAsync();

                TotalPortfolioValue = await _investmentService.GetTotalPortfolioValueAsync();
                TotalInvested = await _investmentService.GetTotalInvestedAsync();
                TotalProfitLoss = await _investmentService.GetTotalProfitLossAsync();

                Portfolio.Clear();
                foreach (var item in portfolio)
                {
                    Portfolio.Add(item);
                }

                RecentTrades.Clear();
                foreach (var trade in trades.Take(10))
                {
                    RecentTrades.Add(trade);
                }
            }
            catch (Exception ex)
            {
                // Log error
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
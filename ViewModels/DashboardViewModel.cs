using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.ViewModels
{
    public class AccountSummary
    {
        public string AccountName { get; set; } = string.Empty;
        public string AccountIcon { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public Color AccountColor { get; set; } = Colors.Gray;
    }

    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionService _transactionService;
        private decimal _totalIncome;
        private decimal _totalExpenses;
        private decimal _balance;

        public decimal TotalIncome
        {
            get => _totalIncome;
            set { _totalIncome = value; OnPropertyChanged(); }
        }

        public decimal TotalExpenses
        {
            get => _totalExpenses;
            set { _totalExpenses = value; OnPropertyChanged(); }
        }

        public decimal Balance
        {
            get => _balance;
            set { _balance = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Transaction> RecentTransactions { get; set; }
        public ObservableCollection<AccountSummary> AccountSummaries { get; set; }

        public DashboardViewModel(ITransactionService transactionService)
        {
            _transactionService = transactionService;
            RecentTransactions = new ObservableCollection<Transaction>();
            AccountSummaries = new ObservableCollection<AccountSummary>();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var allTransactions = await _transactionService.GetAllTransactionsAsync();

                // Calcular totais
                TotalIncome = allTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                TotalExpenses = allTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                Balance = TotalIncome - TotalExpenses;

                // Últimas 5 transações
                var recent = allTransactions.Take(5).ToList();
                RecentTransactions.Clear();
                foreach (var transaction in recent)
                {
                    RecentTransactions.Add(transaction);
                }

                // Calcular resumo por conta
                if (allTransactions.Any() && allTransactions.First().Account != null)
                {
                    var accountSummaries = allTransactions
                        .GroupBy(t => t.Account)
                        .Select(g => new AccountSummary
                        {
                            AccountName = g.Key.Name,
                            AccountIcon = AccountHelper.GetAccountTypeIcon(g.Key.Type),
                            Balance = g.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount) -
                                      g.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount),
                            AccountColor = AccountHelper.GetAccountTypeColor(g.Key.Type)
                        })
                        .OrderByDescending(a => a.Balance)
                        .ToList();

                    AccountSummaries.Clear();
                    foreach (var summary in accountSummaries)
                    {
                        AccountSummaries.Add(summary);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar dados do dashboard: {ex.Message}");
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.ViewModels
{
    public class ComparisonViewModel : INotifyPropertyChanged
    {
        private readonly IReportService _reportService;
        private readonly IAccountService _accountService;
        private bool _isLoading;
        private DateTime _selectedDate = DateTime.Now;
        private string _selectedPeriodText = string.Empty;
        private Account? _selectedAccount = null;
        private string _selectedAccountText = "Todas as Contas";
        private ComparisonReport? _comparisonReport;
        private string _comparisonType = "Mensal";

        public ObservableCollection<PeriodComparison> MonthlyTrend { get; set; }
        public List<Account> Accounts { get; set; } = new();

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); UpdateSelectedPeriodText(); }
        }

        public string SelectedPeriodText
        {
            get => _selectedPeriodText;
            set { _selectedPeriodText = value; OnPropertyChanged(); }
        }

        public Account? SelectedAccount
        {
            get => _selectedAccount;
            set { _selectedAccount = value; OnPropertyChanged(); UpdateSelectedAccountText(); }
        }

        public string SelectedAccountText
        {
            get => _selectedAccountText;
            set { _selectedAccountText = value; OnPropertyChanged(); }
        }

        public ComparisonReport? ComparisonReport
        {
            get => _comparisonReport;
            set { _comparisonReport = value; OnPropertyChanged(); }
        }

        public string ComparisonType
        {
            get => _comparisonType;
            set { _comparisonType = value; OnPropertyChanged(); }
        }

        public ComparisonViewModel(IReportService reportService, IAccountService accountService)
        {
            _reportService = reportService;
            _accountService = accountService;
            MonthlyTrend = new ObservableCollection<PeriodComparison>();

            UpdateSelectedPeriodText();
            UpdateSelectedAccountText();
            _ = LoadAccountsAsync();
            _ = LoadDataAsync();
        }

        private void UpdateSelectedPeriodText()
        {
            SelectedPeriodText = $"{SelectedDate:MMMM yyyy}";
        }

        private void UpdateSelectedAccountText()
        {
            if (SelectedAccount == null)
            {
                SelectedAccountText = "Todas as Contas";
            }
            else
            {
                var icon = AccountHelper.GetAccountTypeIcon(SelectedAccount.Type);
                SelectedAccountText = $"{icon} {SelectedAccount.Name}";
            }
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                Accounts = await _accountService.GetAllAccountsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar contas: {ex.Message}");
            }
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;
                int? accountId = SelectedAccount?.Id;

                if (ComparisonType == "Mensal")
                {
                    await LoadMonthlyComparisonAsync(accountId);
                    await LoadMonthlyTrendAsync(accountId);
                }
                else if (ComparisonType == "Trimestral")
                {
                    await LoadQuarterlyTrendAsync(accountId);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar comparativos: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadMonthlyComparisonAsync(int? accountId)
        {
            var currentStart = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
            var currentEnd = currentStart.AddMonths(1).AddDays(-1);
            var previousStart = currentStart.AddMonths(-1);
            var previousEnd = previousStart.AddMonths(1).AddDays(-1);

            ComparisonReport = await _reportService.GetPeriodComparisonAsync(
                currentStart, currentEnd, previousStart, previousEnd, accountId);
        }

        private async Task LoadMonthlyTrendAsync(int? accountId)
        {
            var monthlyTrend = await _reportService.GetMonthlyTrendAsync(SelectedDate.Year, accountId);

            MonthlyTrend.Clear();
            foreach (var period in monthlyTrend.Where(p => p.TotalIncome > 0 || p.TotalExpense > 0))
            {
                MonthlyTrend.Add(period);
            }
        }

        private async Task LoadQuarterlyTrendAsync(int? accountId)
        {
            var quarterlyTrend = await _reportService.GetQuarterlyTrendAsync(SelectedDate.Year, accountId);

            MonthlyTrend.Clear();
            foreach (var period in quarterlyTrend)
            {
                MonthlyTrend.Add(period);
            }
        }

        public async Task ChangePeriodAsync(DateTime newDate)
        {
            SelectedDate = newDate;
            await LoadDataAsync();
        }

        public async Task ChangeAccountFilterAsync(Account? account)
        {
            SelectedAccount = account;
            await LoadDataAsync();
        }

        public async Task ChangeComparisonTypeAsync(string type)
        {
            ComparisonType = type;
            await LoadDataAsync();
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
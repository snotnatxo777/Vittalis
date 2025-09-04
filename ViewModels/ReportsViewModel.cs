using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.ViewModels
{
    public class CategoryReportItem
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double Percentage { get; set; }
        public Color CategoryColor { get; set; } = Colors.Gray;
    }

    public class ReportsViewModel : INotifyPropertyChanged
    {
        private readonly IReportService _reportService;
        private readonly IAccountService _accountService;
        private bool _isLoading;
        private decimal _totalIncome;
        private decimal _totalExpense;
        private decimal _averageMonthlyIncome;
        private decimal _averageMonthlyExpense;
        private string _topExpenseCategory = string.Empty;
        private string _topIncomeCategory = string.Empty;
        private DateTime _selectedDate = DateTime.Now;
        private string _selectedPeriodText = string.Empty;
        private List<Account> _accounts = new();
        private Account? _selectedAccount = null;
        private string _selectedAccountText = "Todas as Contas";

        public ObservableCollection<CategoryReportItem> ExpensesByCategory { get; set; }
        public ObservableCollection<CategoryReportItem> IncomesByCategory { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public decimal TotalIncome
        {
            get => _totalIncome;
            set { _totalIncome = value; OnPropertyChanged(); }
        }

        public decimal TotalExpense
        {
            get => _totalExpense;
            set { _totalExpense = value; OnPropertyChanged(); }
        }

        public decimal Balance => TotalIncome - TotalExpense;

        public decimal AverageMonthlyIncome
        {
            get => _averageMonthlyIncome;
            set { _averageMonthlyIncome = value; OnPropertyChanged(); }
        }

        public decimal AverageMonthlyExpense
        {
            get => _averageMonthlyExpense;
            set { _averageMonthlyExpense = value; OnPropertyChanged(); }
        }

        public string TopExpenseCategory
        {
            get => _topExpenseCategory;
            set { _topExpenseCategory = value; OnPropertyChanged(); }
        }

        public string TopIncomeCategory
        {
            get => _topIncomeCategory;
            set { _topIncomeCategory = value; OnPropertyChanged(); }
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

        public List<Account> Accounts
        {
            get => _accounts;
            set { _accounts = value; OnPropertyChanged(); }
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

        public ReportsViewModel(IReportService reportService, IAccountService accountService)
        {
            _reportService = reportService;
            _accountService = accountService;
            ExpensesByCategory = new ObservableCollection<CategoryReportItem>();
            IncomesByCategory = new ObservableCollection<CategoryReportItem>();

            UpdateSelectedPeriodText();
            UpdateSelectedAccountText();
            _ = LoadAccountsAsync();
            _ = LoadReportsAsync();
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

        private async Task LoadReportsAsync()
        {
            try
            {
                IsLoading = true;

                // Calcular início e fim do mês selecionado
                var startDate = new DateTime(SelectedDate.Year, SelectedDate.Month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                // Usar filtro de conta se selecionado
                int? accountId = SelectedAccount?.Id;

                var expensesByCategory = await _reportService.GetExpensesByCategoryAsync(startDate, endDate, accountId);
                var incomesByCategory = await _reportService.GetIncomesByCategoryAsync(startDate, endDate, accountId);

                TotalExpense = expensesByCategory.Values.Sum();
                TotalIncome = incomesByCategory.Values.Sum();

                // Estatísticas gerais (com filtro de conta)
                AverageMonthlyIncome = await _reportService.GetAverageMonthlyIncomeAsync(accountId);
                AverageMonthlyExpense = await _reportService.GetAverageMonthlyExpenseAsync(accountId);

                var topExpense = await _reportService.GetTopExpenseCategoryAsync(accountId);
                var topIncome = await _reportService.GetTopIncomeCategoryAsync(accountId);

                TopExpenseCategory = CategoryHelper.GetCategoryName(topExpense);
                TopIncomeCategory = CategoryHelper.GetCategoryName(topIncome);

                // Processar despesas por categoria
                ExpensesByCategory.Clear();
                foreach (var expense in expensesByCategory.OrderByDescending(x => x.Value))
                {
                    ExpensesByCategory.Add(new CategoryReportItem
                    {
                        CategoryName = CategoryHelper.GetCategoryName(expense.Key),
                        CategoryIcon = CategoryHelper.GetCategoryIcon(expense.Key),
                        Amount = expense.Value,
                        Percentage = TotalExpense > 0 ? (double)(expense.Value / TotalExpense * 100) : 0,
                        CategoryColor = GetCategoryColor(expense.Key)
                    });
                }

                // Processar receitas por categoria
                IncomesByCategory.Clear();
                foreach (var income in incomesByCategory.OrderByDescending(x => x.Value))
                {
                    IncomesByCategory.Add(new CategoryReportItem
                    {
                        CategoryName = CategoryHelper.GetCategoryName(income.Key),
                        CategoryIcon = CategoryHelper.GetCategoryIcon(income.Key),
                        Amount = income.Value,
                        Percentage = TotalIncome > 0 ? (double)(income.Value / TotalIncome * 100) : 0,
                        CategoryColor = GetCategoryColor(income.Key)
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar relatórios: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private Color GetCategoryColor(TransactionCategory category)
        {
            return category switch
            {
                TransactionCategory.Food => Colors.Orange,
                TransactionCategory.Transportation => Colors.Blue,
                TransactionCategory.Housing => Colors.Purple,
                TransactionCategory.Health => Colors.Red,
                TransactionCategory.Education => Colors.Green,
                TransactionCategory.Entertainment => Colors.Pink,
                TransactionCategory.Shopping => Colors.Yellow,
                TransactionCategory.Bills => Colors.Brown,
                TransactionCategory.Travel => Colors.Cyan,
                TransactionCategory.Salary => Colors.DarkGreen,
                TransactionCategory.Freelance => Colors.Teal,
                TransactionCategory.Investment => Colors.Gold,
                _ => Colors.Gray
            };
        }

        public async Task ChangePeriodAsync(DateTime newDate)
        {
            SelectedDate = newDate;
            await LoadReportsAsync();
        }

        public async Task ChangeAccountFilterAsync(Account? account)
        {
            SelectedAccount = account;
            await LoadReportsAsync();
        }

        public async Task RefreshAsync()
        {
            await LoadReportsAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using Vittalis.Models;
using Vittalis.Helpers;

namespace Vittalis.Services
{
    public class ReportService : IReportService
    {
        private readonly ITransactionService _transactionService;

        public ReportService(ITransactionService transactionService)
        {
            _transactionService = transactionService;
        }

        public async Task<Dictionary<TransactionCategory, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null, int? accountId = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();

            var filtered = transactions.Where(t => t.Type == TransactionType.Expense);

            if (startDate.HasValue)
                filtered = filtered.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                filtered = filtered.Where(t => t.Date <= endDate.Value);

            if (accountId.HasValue)
                filtered = filtered.Where(t => t.AccountId == accountId.Value);

            return filtered.GroupBy(t => t.Category)
                          .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }

        public async Task<Dictionary<TransactionCategory, decimal>> GetIncomesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null, int? accountId = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();

            var filtered = transactions.Where(t => t.Type == TransactionType.Income);

            if (startDate.HasValue)
                filtered = filtered.Where(t => t.Date >= startDate.Value);

            if (endDate.HasValue)
                filtered = filtered.Where(t => t.Date <= endDate.Value);

            if (accountId.HasValue)
                filtered = filtered.Where(t => t.AccountId == accountId.Value);

            return filtered.GroupBy(t => t.Category)
                          .ToDictionary(g => g.Key, g => g.Sum(t => t.Amount));
        }

        public async Task<Dictionary<DateTime, decimal>> GetDailyBalanceAsync(DateTime startDate, DateTime endDate, int? accountId = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var result = new Dictionary<DateTime, decimal>();

            if (accountId.HasValue)
                transactions = transactions.Where(t => t.AccountId == accountId.Value).ToList();

            for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
            {
                var dayTransactions = transactions.Where(t => t.Date.Date == date);
                var dayIncome = dayTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var dayExpense = dayTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                result[date] = dayIncome - dayExpense;
            }

            return result;
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyReportAsync(int year, int? accountId = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var yearTransactions = transactions.Where(t => t.Date.Year == year);

            if (accountId.HasValue)
                yearTransactions = yearTransactions.Where(t => t.AccountId == accountId.Value);

            var result = new Dictionary<string, decimal>();

            for (int month = 1; month <= 12; month++)
            {
                var monthTransactions = yearTransactions.Where(t => t.Date.Month == month);
                var income = monthTransactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                var expense = monthTransactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                var balance = income - expense;

                var monthName = new DateTime(year, month, 1).ToString("MMM", System.Globalization.CultureInfo.GetCultureInfo("pt-BR"));
                result[monthName] = balance;
            }

            return result;
        }

        public async Task<decimal> GetAverageMonthlyIncomeAsync(int? accountId = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var incomes = transactions.Where(t => t.Type == TransactionType.Income);

            if (accountId.HasValue)
                incomes = incomes.Where(t => t.AccountId == accountId.Value);

            var incomeList = incomes.ToList();
            if (!incomeList.Any()) return 0;

            var monthsWithData = incomeList.GroupBy(t => new { t.Date.Year, t.Date.Month }).Count();
            var totalIncome = incomeList.Sum(t => t.Amount);

            return monthsWithData > 0 ? totalIncome / monthsWithData : 0;
        }

        public async Task<decimal> GetAverageMonthlyExpenseAsync(int? accountId = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var expenses = transactions.Where(t => t.Type == TransactionType.Expense);

            if (accountId.HasValue)
                expenses = expenses.Where(t => t.AccountId == accountId.Value);

            var expenseList = expenses.ToList();
            if (!expenseList.Any()) return 0;

            var monthsWithData = expenseList.GroupBy(t => new { t.Date.Year, t.Date.Month }).Count();
            var totalExpense = expenseList.Sum(t => t.Amount);

            return monthsWithData > 0 ? totalExpense / monthsWithData : 0;
        }

        public async Task<TransactionCategory> GetTopExpenseCategoryAsync(int? accountId = null)
        {
            var expensesByCategory = await GetExpensesByCategoryAsync(accountId: accountId);
            return expensesByCategory.Any() ? expensesByCategory.OrderByDescending(x => x.Value).First().Key : TransactionCategory.OtherExpense;
        }

        public async Task<TransactionCategory> GetTopIncomeCategoryAsync(int? accountId = null)
        {
            var incomesByCategory = await GetIncomesByCategoryAsync(accountId: accountId);
            return incomesByCategory.Any() ? incomesByCategory.OrderByDescending(x => x.Value).First().Key : TransactionCategory.OtherIncome;
        }

        public async Task<ComparisonReport> GetPeriodComparisonAsync(DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd, int? accountId = null)
        {
            // Dados do período atual
            var currentIncomes = await GetIncomesByCategoryAsync(currentStart, currentEnd, accountId);
            var currentExpenses = await GetExpensesByCategoryAsync(currentStart, currentEnd, accountId);

            // Dados do período anterior
            var previousIncomes = await GetIncomesByCategoryAsync(previousStart, previousEnd, accountId);
            var previousExpenses = await GetExpensesByCategoryAsync(previousStart, previousEnd, accountId);

            var report = new ComparisonReport
            {
                CurrentPeriod = new PeriodComparison
                {
                    PeriodName = $"{currentStart:MMM yyyy}",
                    StartDate = currentStart,
                    EndDate = currentEnd,
                    TotalIncome = currentIncomes.Values.Sum(),
                    TotalExpense = currentExpenses.Values.Sum()
                },
                PreviousPeriod = new PeriodComparison
                {
                    PeriodName = $"{previousStart:MMM yyyy}",
                    StartDate = previousStart,
                    EndDate = previousEnd,
                    TotalIncome = previousIncomes.Values.Sum(),
                    TotalExpense = previousExpenses.Values.Sum()
                }
            };

            report.CurrentPeriod.Balance = report.CurrentPeriod.TotalIncome - report.CurrentPeriod.TotalExpense;
            report.PreviousPeriod.Balance = report.PreviousPeriod.TotalIncome - report.PreviousPeriod.TotalExpense;

            // Comparar despesas por categoria
            var allExpenseCategories = currentExpenses.Keys.Union(previousExpenses.Keys).Distinct();
            foreach (var category in allExpenseCategories)
            {
                var currentAmount = currentExpenses.GetValueOrDefault(category, 0);
                var previousAmount = previousExpenses.GetValueOrDefault(category, 0);

                report.ExpenseComparisons.Add(new CategoryComparison
                {
                    CategoryName = CategoryHelper.GetCategoryName(category),
                    CategoryIcon = CategoryHelper.GetCategoryIcon(category),
                    CurrentAmount = currentAmount,
                    PreviousAmount = previousAmount
                });
            }

            // Comparar receitas por categoria
            var allIncomeCategories = currentIncomes.Keys.Union(previousIncomes.Keys).Distinct();
            foreach (var category in allIncomeCategories)
            {
                var currentAmount = currentIncomes.GetValueOrDefault(category, 0);
                var previousAmount = previousIncomes.GetValueOrDefault(category, 0);

                report.IncomeComparisons.Add(new CategoryComparison
                {
                    CategoryName = CategoryHelper.GetCategoryName(category),
                    CategoryIcon = CategoryHelper.GetCategoryIcon(category),
                    CurrentAmount = currentAmount,
                    PreviousAmount = previousAmount
                });
            }

            // Ordenar por maior diferença (absoluta)
            report.ExpenseComparisons = report.ExpenseComparisons
                .OrderByDescending(c => Math.Abs(c.Difference))
                .ToList();

            report.IncomeComparisons = report.IncomeComparisons
                .OrderByDescending(c => Math.Abs(c.Difference))
                .ToList();

            return report;
        }

        public async Task<List<PeriodComparison>> GetMonthlyTrendAsync(int year, int? accountId = null)
        {
            var trends = new List<PeriodComparison>();

            for (int month = 1; month <= 12; month++)
            {
                var startDate = new DateTime(year, month, 1);
                var endDate = startDate.AddMonths(1).AddDays(-1);

                var incomes = await GetIncomesByCategoryAsync(startDate, endDate, accountId);
                var expenses = await GetExpensesByCategoryAsync(startDate, endDate, accountId);

                var totalIncome = incomes.Values.Sum();
                var totalExpense = expenses.Values.Sum();

                trends.Add(new PeriodComparison
                {
                    PeriodName = startDate.ToString("MMM"),
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    Balance = totalIncome - totalExpense
                });
            }

            return trends;
        }

        public async Task<List<PeriodComparison>> GetQuarterlyTrendAsync(int year, int? accountId = null)
        {
            var trends = new List<PeriodComparison>();

            for (int quarter = 1; quarter <= 4; quarter++)
            {
                var startMonth = (quarter - 1) * 3 + 1;
                var startDate = new DateTime(year, startMonth, 1);
                var endDate = startDate.AddMonths(3).AddDays(-1);

                var incomes = await GetIncomesByCategoryAsync(startDate, endDate, accountId);
                var expenses = await GetExpensesByCategoryAsync(startDate, endDate, accountId);

                var totalIncome = incomes.Values.Sum();
                var totalExpense = expenses.Values.Sum();

                trends.Add(new PeriodComparison
                {
                    PeriodName = $"Q{quarter}",
                    StartDate = startDate,
                    EndDate = endDate,
                    TotalIncome = totalIncome,
                    TotalExpense = totalExpense,
                    Balance = totalIncome - totalExpense
                });
            }

            return trends;
        }
    }
}
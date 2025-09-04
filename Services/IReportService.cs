using Vittalis.Models;
using Vittalis.Helpers;

namespace Vittalis.Services
{
    public interface IReportService
    {
        Task<Dictionary<TransactionCategory, decimal>> GetExpensesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null, int? accountId = null);
        Task<Dictionary<TransactionCategory, decimal>> GetIncomesByCategoryAsync(DateTime? startDate = null, DateTime? endDate = null, int? accountId = null);
        Task<Dictionary<DateTime, decimal>> GetDailyBalanceAsync(DateTime startDate, DateTime endDate, int? accountId = null);
        Task<Dictionary<string, decimal>> GetMonthlyReportAsync(int year, int? accountId = null);
        Task<decimal> GetAverageMonthlyIncomeAsync(int? accountId = null);
        Task<decimal> GetAverageMonthlyExpenseAsync(int? accountId = null);
        Task<TransactionCategory> GetTopExpenseCategoryAsync(int? accountId = null);
        Task<TransactionCategory> GetTopIncomeCategoryAsync(int? accountId = null);
        Task<ComparisonReport> GetPeriodComparisonAsync(DateTime currentStart, DateTime currentEnd, DateTime previousStart, DateTime previousEnd, int? accountId = null);
        Task<List<PeriodComparison>> GetMonthlyTrendAsync(int year, int? accountId = null);
        Task<List<PeriodComparison>> GetQuarterlyTrendAsync(int year, int? accountId = null);
    }
}
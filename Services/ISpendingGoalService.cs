using Vittalis.Models;

namespace Vittalis.Services
{
    public interface ISpendingGoalService
    {
        Task<List<SpendingGoal>> GetSpendingGoalsAsync(int year, int month);
        Task<SpendingGoal> AddSpendingGoalAsync(SpendingGoal spendingGoal);
        Task<bool> UpdateSpendingGoalAsync(SpendingGoal spendingGoal);
        Task<bool> DeleteSpendingGoalAsync(int id);
        Task<List<SpendingGoal>> GetSpendingGoalsWithProgressAsync(int year, int month);
        Task<SpendingGoal?> GetSpendingGoalAsync(TransactionCategory category, int year, int month, int? accountId = null);
    }
}
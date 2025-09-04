using Microsoft.EntityFrameworkCore;
using Vittalis.Data;
using Vittalis.Models;

namespace Vittalis.Services
{
    public class SpendingGoalService : ISpendingGoalService
    {
        private readonly VittalisDbContext _context;

        public SpendingGoalService(VittalisDbContext context)
        {
            _context = context;
        }

        public async Task<List<SpendingGoal>> GetSpendingGoalsAsync(int year, int month)
        {
            return await _context.SpendingGoals
                .Include(sg => sg.Account)
                .Where(sg => sg.Year == year && sg.Month == month && sg.IsActive)
                .OrderBy(sg => sg.Category)
                .ToListAsync();
        }

        public async Task<SpendingGoal> AddSpendingGoalAsync(SpendingGoal spendingGoal)
        {
            spendingGoal.CreatedAt = DateTime.UtcNow;
            _context.SpendingGoals.Add(spendingGoal);
            await _context.SaveChangesAsync();
            return spendingGoal;
        }

        public async Task<bool> UpdateSpendingGoalAsync(SpendingGoal spendingGoal)
        {
            _context.Entry(spendingGoal).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteSpendingGoalAsync(int id)
        {
            var spendingGoal = await _context.SpendingGoals.FindAsync(id);
            if (spendingGoal == null) return false;

            spendingGoal.IsActive = false; // Soft delete
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<SpendingGoal?> GetSpendingGoalAsync(TransactionCategory category, int year, int month, int? accountId = null)
        {
            return await _context.SpendingGoals
                .Include(sg => sg.Account)
                .FirstOrDefaultAsync(sg =>
                    sg.Category == category &&
                    sg.Year == year &&
                    sg.Month == month &&
                    sg.AccountId == accountId &&
                    sg.IsActive);
        }

        public async Task<List<SpendingGoal>> GetSpendingGoalsWithProgressAsync(int year, int month)
        {
            var spendingGoals = await GetSpendingGoalsAsync(year, month);

            // Calcular gastos atuais para cada meta
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Buscar todas as transações do período de uma vez
            var allTransactions = await _context.Transactions
                .Where(t => t.Type == TransactionType.Expense &&
                           t.Date >= startDate &&
                           t.Date <= endDate)
                .ToListAsync(); // Trazer para memória para usar LINQ to Objects

            foreach (var goal in spendingGoals)
            {
                // Filtrar transações para esta meta usando LINQ to Objects
                var goalTransactions = allTransactions
                    .Where(t => t.Category == goal.Category);

                // Filtrar por conta se especificada
                if (goal.AccountId.HasValue)
                {
                    goalTransactions = goalTransactions.Where(t => t.AccountId == goal.AccountId.Value);
                }

                // Calcular soma usando LINQ to Objects (não SQL)
                goal.CurrentSpent = goalTransactions.Sum(t => t.Amount);
            }

            return spendingGoals;
        }
    }
}
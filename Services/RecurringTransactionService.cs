using Microsoft.EntityFrameworkCore;
using Vittalis.Data;
using Vittalis.Models;

namespace Vittalis.Services
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private readonly VittalisDbContext _context;
        private readonly ITransactionService _transactionService;

        public RecurringTransactionService(VittalisDbContext context, ITransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<List<RecurringTransaction>> GetAllRecurringTransactionsAsync()
        {
            return await _context.RecurringTransactions
                .Include(rt => rt.Account)
                .Where(rt => rt.IsActive)
                .OrderBy(rt => rt.NextOccurrence)
                .ToListAsync();
        }

        public async Task<RecurringTransaction> AddRecurringTransactionAsync(RecurringTransaction recurringTransaction)
        {
            recurringTransaction.CreatedAt = DateTime.UtcNow;
            recurringTransaction.NextOccurrence = CalculateNextOccurrence(recurringTransaction.StartDate, recurringTransaction.Frequency);

            _context.RecurringTransactions.Add(recurringTransaction);
            await _context.SaveChangesAsync();
            return recurringTransaction;
        }

        public async Task<bool> UpdateRecurringTransactionAsync(RecurringTransaction recurringTransaction)
        {
            _context.Entry(recurringTransaction).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteRecurringTransactionAsync(int id)
        {
            var recurringTransaction = await _context.RecurringTransactions.FindAsync(id);
            if (recurringTransaction == null) return false;

            recurringTransaction.IsActive = false; // Soft delete
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<RecurringTransaction>> GetPendingTransactionsAsync()
        {
            var today = DateTime.Today;
            return await _context.RecurringTransactions
                .Include(rt => rt.Account)
                .Where(rt => rt.IsActive && rt.NextOccurrence <= today)
                .ToListAsync();
        }

        public async Task ProcessPendingTransactionsAsync()
        {
            var pendingTransactions = await GetPendingTransactionsAsync();

            foreach (var recurringTransaction in pendingTransactions)
            {
                // Criar nova transação baseada na recorrente
                var newTransaction = new Transaction
                {
                    Description = recurringTransaction.Description,
                    Amount = recurringTransaction.Amount,
                    Date = recurringTransaction.NextOccurrence,
                    Type = recurringTransaction.Type,
                    Category = recurringTransaction.Category,
                    AccountId = recurringTransaction.AccountId,
                    RecurringTransactionId = recurringTransaction.Id
                };

                await _transactionService.AddTransactionAsync(newTransaction);

                // Atualizar próxima ocorrência
                recurringTransaction.NextOccurrence = CalculateNextOccurrence(
                    recurringTransaction.NextOccurrence,
                    recurringTransaction.Frequency);

                // Verificar se chegou na data de fim
                if (recurringTransaction.EndDate.HasValue &&
                    recurringTransaction.NextOccurrence > recurringTransaction.EndDate.Value)
                {
                    recurringTransaction.IsActive = false;
                }

                await UpdateRecurringTransactionAsync(recurringTransaction);
            }
        }

        private DateTime CalculateNextOccurrence(DateTime currentDate, RecurrenceFrequency frequency)
        {
            return frequency switch
            {
                RecurrenceFrequency.Weekly => currentDate.AddDays(7),
                RecurrenceFrequency.Monthly => currentDate.AddMonths(1),
                RecurrenceFrequency.Quarterly => currentDate.AddMonths(3),
                RecurrenceFrequency.Yearly => currentDate.AddYears(1),
                _ => currentDate.AddMonths(1)
            };
        }
    }
}
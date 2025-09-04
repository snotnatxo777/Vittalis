using Vittalis.Models;

namespace Vittalis.Services
{
    public interface IRecurringTransactionService
    {
        Task<List<RecurringTransaction>> GetAllRecurringTransactionsAsync();
        Task<RecurringTransaction> AddRecurringTransactionAsync(RecurringTransaction recurringTransaction);
        Task<bool> UpdateRecurringTransactionAsync(RecurringTransaction recurringTransaction);
        Task<bool> DeleteRecurringTransactionAsync(int id);
        Task<List<RecurringTransaction>> GetPendingTransactionsAsync();
        Task ProcessPendingTransactionsAsync();
    }
}
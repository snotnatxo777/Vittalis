using Microsoft.EntityFrameworkCore;
using Vittalis.Data;
using Vittalis.Models;

namespace Vittalis.Services
{
    public interface ITransactionService
    {
        Task<List<Transaction>> GetAllTransactionsAsync();
        Task<Transaction> AddTransactionAsync(Transaction transaction);
        Task<Transaction?> GetTransactionByIdAsync(int id);
        Task<bool> UpdateTransactionAsync(Transaction transaction);
        Task<bool> DeleteTransactionAsync(int id);
    }

    public class TransactionService : ITransactionService
    {
        private readonly VittalisDbContext _context;

        public TransactionService(VittalisDbContext context)
        {
            _context = context;
        }

        public async Task<List<Transaction>> GetAllTransactionsAsync()
        {
            return await _context.Transactions
                .Include(t => t.Account)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<Transaction> AddTransactionAsync(Transaction transaction)
        {
            transaction.CreatedAt = DateTime.UtcNow;
            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _context.Transactions.FindAsync(id);
        }

        public async Task<bool> UpdateTransactionAsync(Transaction transaction)
        {
            _context.Entry(transaction).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null) return false;

            _context.Transactions.Remove(transaction);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }
    }
}
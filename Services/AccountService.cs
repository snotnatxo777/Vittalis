using Microsoft.EntityFrameworkCore;
using Vittalis.Data;
using Vittalis.Models;

namespace Vittalis.Services
{
    public class AccountService : IAccountService
    {
        private readonly VittalisDbContext _context;

        public AccountService(VittalisDbContext context)
        {
            _context = context;
        }

        public async Task<List<Account>> GetAllAccountsAsync()
        {
            return await _context.Accounts
                .Include(a => a.Transactions)
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<Account> AddAccountAsync(Account account)
        {
            account.CreatedAt = DateTime.UtcNow;
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync();
            return account;
        }

        public async Task<Account?> GetAccountByIdAsync(int id)
        {
            return await _context.Accounts
                .Include(a => a.Transactions)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<bool> UpdateAccountAsync(Account account)
        {
            _context.Entry(account).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAccountAsync(int id)
        {
            var account = await _context.Accounts.FindAsync(id);
            if (account == null) return false;

            account.IsActive = false; // Soft delete
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<decimal> GetAccountBalanceAsync(int accountId)
        {
            var account = await GetAccountByIdAsync(accountId);
            return account?.CurrentBalance ?? 0;
        }
    }
}
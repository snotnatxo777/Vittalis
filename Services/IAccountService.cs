using Vittalis.Models;

namespace Vittalis.Services
{
    public interface IAccountService
    {
        Task<List<Account>> GetAllAccountsAsync();
        Task<Account> AddAccountAsync(Account account);
        Task<Account?> GetAccountByIdAsync(int id);
        Task<bool> UpdateAccountAsync(Account account);
        Task<bool> DeleteAccountAsync(int id);
        Task<decimal> GetAccountBalanceAsync(int accountId);
    }
}
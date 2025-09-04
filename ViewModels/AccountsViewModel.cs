using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;

namespace Vittalis.ViewModels
{
    public class AccountsViewModel : INotifyPropertyChanged
    {
        private readonly IAccountService _accountService;
        private bool _isLoading;
        private decimal _totalBalance;

        public ObservableCollection<Account> Accounts { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public decimal TotalBalance
        {
            get => _totalBalance;
            set { _totalBalance = value; OnPropertyChanged(); }
        }

        public AccountsViewModel(IAccountService accountService)
        {
            _accountService = accountService;
            Accounts = new ObservableCollection<Account>();
            _ = LoadAccountsAsync();
        }

        private async Task LoadAccountsAsync()
        {
            try
            {
                IsLoading = true;
                var accounts = await _accountService.GetAllAccountsAsync();

                Accounts.Clear();
                foreach (var account in accounts)
                {
                    Accounts.Add(account);
                }

                // Calcular saldo total
                TotalBalance = Accounts.Sum(a => a.CurrentBalance);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar contas: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task AddAccountAsync(Account account)
        {
            try
            {
                await _accountService.AddAccountAsync(account);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar conta: {ex.Message}");
            }
        }

        public async Task DeleteAccountAsync(int accountId)
        {
            try
            {
                await _accountService.DeleteAccountAsync(accountId);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir conta: {ex.Message}");
            }
        }

        public async Task RefreshAsync()
        {
            await LoadAccountsAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
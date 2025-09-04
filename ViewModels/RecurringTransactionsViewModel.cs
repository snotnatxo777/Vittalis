using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.ViewModels
{
    public class RecurringTransactionsViewModel : INotifyPropertyChanged
    {
        private readonly IRecurringTransactionService _recurringTransactionService;
        private bool _isLoading;
        private int _pendingCount;

        public ObservableCollection<RecurringTransaction> RecurringTransactions { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public int PendingCount
        {
            get => _pendingCount;
            set { _pendingCount = value; OnPropertyChanged(); }
        }

        public RecurringTransactionsViewModel(IRecurringTransactionService recurringTransactionService)
        {
            _recurringTransactionService = recurringTransactionService;
            RecurringTransactions = new ObservableCollection<RecurringTransaction>();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var recurringTransactions = await _recurringTransactionService.GetAllRecurringTransactionsAsync();
                var pendingTransactions = await _recurringTransactionService.GetPendingTransactionsAsync();

                RecurringTransactions.Clear();
                foreach (var transaction in recurringTransactions)
                {
                    RecurringTransactions.Add(transaction);
                }

                PendingCount = pendingTransactions.Count;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar transações recorrentes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ProcessPendingTransactionsAsync()
        {
            try
            {
                IsLoading = true;
                await _recurringTransactionService.ProcessPendingTransactionsAsync();
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao processar transações pendentes: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task DeleteRecurringTransactionAsync(int id)
        {
            try
            {
                await _recurringTransactionService.DeleteRecurringTransactionAsync(id);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir transação recorrente: {ex.Message}");
            }
        }

        public async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
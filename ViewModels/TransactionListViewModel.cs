using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;

namespace Vittalis.ViewModels
{
    public class TransactionListViewModel : INotifyPropertyChanged
    {
        private readonly ITransactionService _transactionService;
        private bool _isLoading;
        private List<Transaction> _allTransactions = new();
        private string _searchText = string.Empty;
        private string _filterType = "Todos";

        public async Task SearchTransactionsAsync(string searchText)
        {
            _searchText = searchText;
            await ApplyFiltersAsync();
        }

        public async Task FilterByTypeAsync(string filterType)
        {
            _filterType = filterType;
            await ApplyFiltersAsync();
        }

        private async Task ApplyFiltersAsync()
        {
            var filteredTransactions = _allTransactions.AsEnumerable();

            // Filtro por tipo
            if (_filterType == "Receitas")
                filteredTransactions = filteredTransactions.Where(t => t.Type == TransactionType.Income);
            else if (_filterType == "Despesas")
                filteredTransactions = filteredTransactions.Where(t => t.Type == TransactionType.Expense);

            // Filtro por busca
            if (!string.IsNullOrWhiteSpace(_searchText))
                filteredTransactions = filteredTransactions.Where(t =>
                    t.Description.Contains(_searchText, StringComparison.OrdinalIgnoreCase));

            // Atualizar lista
            Transactions.Clear();
            foreach (var transaction in filteredTransactions.OrderByDescending(t => t.Date))
            {
                Transactions.Add(transaction);
            }
        }

        public async Task DeleteTransactionAsync(int transactionId)
        {
            try
            {
                await _transactionService.DeleteTransactionAsync(transactionId);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                // Log error
            }
        }

        public ObservableCollection<Transaction> Transactions { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public TransactionListViewModel(ITransactionService transactionService)
        {
            _transactionService = transactionService;
            Transactions = new ObservableCollection<Transaction>();
            _ = LoadTransactionsAsync();
        }

        private async Task LoadTransactionsAsync()
        {
            try
            {
                IsLoading = true;
                _allTransactions = await _transactionService.GetAllTransactionsAsync();
                await ApplyFiltersAsync();
            }
            catch (Exception ex)
            {
                // Log error
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task RefreshAsync()
        {
            await LoadTransactionsAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.ViewModels
{
    public class CreditCardViewModel : INotifyPropertyChanged
    {
        private readonly ICreditCardService _creditCardService;
        private bool _isLoading;
        private decimal _totalCreditLimit;
        private decimal _totalUsedLimit;
        private decimal _totalAvailableLimit;

        public ObservableCollection<CreditCard> CreditCards { get; set; }
        public ObservableCollection<CreditCardTransaction> RecentTransactions { get; set; }
        public ObservableCollection<Installment> ActiveInstallments { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public decimal TotalCreditLimit
        {
            get => _totalCreditLimit;
            set { _totalCreditLimit = value; OnPropertyChanged(); }
        }

        public decimal TotalUsedLimit
        {
            get => _totalUsedLimit;
            set { _totalUsedLimit = value; OnPropertyChanged(); }
        }

        public decimal TotalAvailableLimit
        {
            get => _totalAvailableLimit;
            set { _totalAvailableLimit = value; OnPropertyChanged(); }
        }

        public double OverallUsagePercentage => TotalCreditLimit > 0 ? (double)(TotalUsedLimit / TotalCreditLimit * 100) : 0;

        public CreditCardViewModel(ICreditCardService creditCardService)
        {
            _creditCardService = creditCardService;
            CreditCards = new ObservableCollection<CreditCard>();
            RecentTransactions = new ObservableCollection<CreditCardTransaction>();
            ActiveInstallments = new ObservableCollection<Installment>();
            _ = LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var creditCards = await _creditCardService.GetAllCreditCardsAsync();

                CreditCards.Clear();
                foreach (var card in creditCards)
                {
                    CreditCards.Add(card);
                }

                // Calcular totais
                TotalCreditLimit = CreditCards.Sum(c => c.CreditLimit);
                TotalUsedLimit = CreditCards.Sum(c => c.CurrentBalance);
                TotalAvailableLimit = TotalCreditLimit - TotalUsedLimit;

                // Notificar propriedade calculada
                OnPropertyChanged(nameof(OverallUsagePercentage));

                // Carregar transações recentes (últimas 10 de todos os cartões)
                await LoadRecentTransactionsAsync();

                // Carregar parcelamentos ativos
                await LoadActiveInstallmentsAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar cartões: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task LoadRecentTransactionsAsync()
        {
            var allTransactions = new List<CreditCardTransaction>();

            foreach (var card in CreditCards)
            {
                var transactions = await _creditCardService.GetTransactionsByCardAsync(card.Id);
                allTransactions.AddRange(transactions.Take(5)); // 5 mais recentes de cada cartão
            }

            var recentTransactions = allTransactions
                .OrderByDescending(t => t.TransactionDate)
                .Take(10)
                .ToList();

            RecentTransactions.Clear();
            foreach (var transaction in recentTransactions)
            {
                RecentTransactions.Add(transaction);
            }
        }

        private async Task LoadActiveInstallmentsAsync()
        {
            var allInstallments = new List<Installment>();

            foreach (var card in CreditCards)
            {
                var installments = await _creditCardService.GetActiveInstallmentsAsync(card.Id);
                allInstallments.AddRange(installments);
            }

            ActiveInstallments.Clear();
            foreach (var installment in allInstallments.OrderBy(i => i.FirstInstallmentDate))
            {
                ActiveInstallments.Add(installment);
            }
        }

        public async Task AddCreditCardAsync(CreditCard creditCard)
        {
            try
            {
                await _creditCardService.AddCreditCardAsync(creditCard);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar cartão: {ex.Message}");
            }
        }

        public async Task AddTransactionAsync(CreditCardTransaction transaction)
        {
            try
            {
                await _creditCardService.AddTransactionAsync(transaction);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar transação: {ex.Message}");
            }
        }

        public async Task AddInstallmentAsync(Installment installment)
        {
            try
            {
                await _creditCardService.AddInstallmentAsync(installment);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar parcelamento: {ex.Message}");
            }
        }

        public async Task DeleteCreditCardAsync(int cardId)
        {
            try
            {
                await _creditCardService.DeleteCreditCardAsync(cardId);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir cartão: {ex.Message}");
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
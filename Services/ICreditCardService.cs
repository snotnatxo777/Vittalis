using Vittalis.Models;

namespace Vittalis.Services
{
    public interface ICreditCardService
    {
        Task<List<CreditCard>> GetAllCreditCardsAsync();
        Task<CreditCard> AddCreditCardAsync(CreditCard creditCard);
        Task<bool> UpdateCreditCardAsync(CreditCard creditCard);
        Task<bool> DeleteCreditCardAsync(int id);

        Task<CreditCardTransaction> AddTransactionAsync(CreditCardTransaction transaction);
        Task<List<CreditCardTransaction>> GetTransactionsByCardAsync(int creditCardId);
        Task<List<CreditCardTransaction>> GetCurrentBillTransactionsAsync(int creditCardId);

        Task<Installment> AddInstallmentAsync(Installment installment);
        Task<List<Installment>> GetActiveInstallmentsAsync(int creditCardId);

        Task<CreditCardBill> GenerateBillAsync(int creditCardId, DateTime closingDate);
        Task<List<CreditCardBill>> GetBillsAsync(int creditCardId);

        Task UpdateCardBalancesAsync();
    }
}
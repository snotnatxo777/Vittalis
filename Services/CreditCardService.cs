using Microsoft.EntityFrameworkCore;
using Vittalis.Data;
using Vittalis.Models;

namespace Vittalis.Services
{
    public class CreditCardService : ICreditCardService
    {
        private readonly VittalisDbContext _context;

        public CreditCardService(VittalisDbContext context)
        {
            _context = context;
        }

        public async Task<List<CreditCard>> GetAllCreditCardsAsync()
        {
            var cards = await _context.CreditCards
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();

            // Calcular saldo atual para cada cartão
            foreach (var card in cards)
            {
                card.CurrentBalance = await CalculateCurrentBalanceAsync(card.Id);
            }

            return cards;
        }

        public async Task<CreditCard> AddCreditCardAsync(CreditCard creditCard)
        {
            creditCard.CreatedAt = DateTime.UtcNow;
            _context.CreditCards.Add(creditCard);
            await _context.SaveChangesAsync();
            return creditCard;
        }

        public async Task<bool> UpdateCreditCardAsync(CreditCard creditCard)
        {
            _context.Entry(creditCard).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteCreditCardAsync(int id)
        {
            var creditCard = await _context.CreditCards.FindAsync(id);
            if (creditCard == null) return false;

            creditCard.IsActive = false; // Soft delete
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<CreditCardTransaction> AddTransactionAsync(CreditCardTransaction transaction)
        {
            transaction.CreatedAt = DateTime.UtcNow;

            // Apenas adicionar ao controle do cartão (não criar transação duplicada)
            _context.CreditCardTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return transaction;
        }

        public async Task<List<CreditCardTransaction>> GetTransactionsByCardAsync(int creditCardId)
        {
            return await _context.CreditCardTransactions
                .Include(t => t.Installment)
                .Where(t => t.CreditCardId == creditCardId)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<List<CreditCardTransaction>> GetCurrentBillTransactionsAsync(int creditCardId)
        {
            var card = await _context.CreditCards.FindAsync(creditCardId);
            if (card == null) return new List<CreditCardTransaction>();

            var currentDate = DateTime.Now;
            var closingDate = CalculateClosingDate(card.ClosingDay, currentDate);

            // Se já passou do fechamento deste mês, buscar próximo ciclo
            if (currentDate > closingDate)
            {
                closingDate = closingDate.AddMonths(1);
            }

            var previousClosingDate = closingDate.AddMonths(-1);

            return await _context.CreditCardTransactions
                .Include(t => t.Installment)
                .Where(t => t.CreditCardId == creditCardId &&
                           t.TransactionDate > previousClosingDate &&
                           t.TransactionDate <= closingDate)
                .OrderByDescending(t => t.TransactionDate)
                .ToListAsync();
        }

        public async Task<Installment> AddInstallmentAsync(Installment installment)
        {
            installment.CreatedAt = DateTime.UtcNow;
            _context.Installments.Add(installment);
            await _context.SaveChangesAsync();

            // Criar as transações das parcelas
            await CreateInstallmentTransactionsAsync(installment);

            return installment;
        }

        public async Task<List<Installment>> GetActiveInstallmentsAsync(int creditCardId)
        {
            return await _context.Installments
                .Where(i => i.CreditCardId == creditCardId && !i.IsCompleted)
                .OrderBy(i => i.FirstInstallmentDate)
                .ToListAsync();
        }

        public async Task<CreditCardBill> GenerateBillAsync(int creditCardId, DateTime closingDate)
        {
            var transactions = await GetCurrentBillTransactionsAsync(creditCardId);
            var totalAmount = transactions.Sum(t => t.Amount);

            var card = await _context.CreditCards.FindAsync(creditCardId);
            if (card == null) throw new ArgumentException("Cartão não encontrado");

            var dueDate = CalculateDueDate(card.DueDay, closingDate);

            var bill = new CreditCardBill
            {
                CreditCardId = creditCardId,
                CreditCard = card,
                ClosingDate = closingDate,
                DueDate = dueDate,
                TotalAmount = totalAmount,
                BillTransactions = transactions
            };

            _context.CreditCardBills.Add(bill);
            await _context.SaveChangesAsync();

            return bill;
        }

        public async Task<List<CreditCardBill>> GetBillsAsync(int creditCardId)
        {
            return await _context.CreditCardBills
                .Include(b => b.CreditCard)
                .Where(b => b.CreditCardId == creditCardId)
                .OrderByDescending(b => b.ClosingDate)
                .ToListAsync();
        }

        public async Task UpdateCardBalancesAsync()
        {
            var cards = await _context.CreditCards.Where(c => c.IsActive).ToListAsync();

            foreach (var card in cards)
            {
                card.CurrentBalance = await CalculateCurrentBalanceAsync(card.Id);
            }

            await _context.SaveChangesAsync();
        }

        private async Task<decimal> CalculateCurrentBalanceAsync(int creditCardId)
        {
            var transactions = await GetCurrentBillTransactionsAsync(creditCardId);
            return transactions.Sum(t => t.Amount);
        }

        private async Task CreateInstallmentTransactionsAsync(Installment installment)
        {
            var currentDate = installment.FirstInstallmentDate;

            for (int i = 0; i < installment.TotalInstallments; i++)
            {
                var transaction = new CreditCardTransaction
                {
                    CreditCardId = installment.CreditCardId,
                    Description = $"{installment.Description} ({i + 1}/{installment.TotalInstallments})",
                    Amount = installment.InstallmentAmount,
                    TransactionDate = currentDate,
                    Category = installment.Category,
                    InstallmentId = installment.Id
                };

                _context.CreditCardTransactions.Add(transaction);
                currentDate = currentDate.AddMonths(1);
            }

            await _context.SaveChangesAsync();
        }

        private DateTime CalculateClosingDate(int closingDay, DateTime referenceDate)
        {
            var closingDate = new DateTime(referenceDate.Year, referenceDate.Month, closingDay);

            // Se o dia de fechamento ainda não chegou neste mês, usar este mês
            // Se já passou, usar próximo mês
            if (referenceDate.Day > closingDay)
            {
                closingDate = closingDate.AddMonths(1);
            }

            return closingDate;
        }

        private DateTime CalculateDueDate(int dueDay, DateTime closingDate)
        {
            var nextMonth = closingDate.AddMonths(1);
            return new DateTime(nextMonth.Year, nextMonth.Month, dueDay);
        }
    }
}
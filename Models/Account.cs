using System.ComponentModel.DataAnnotations;

namespace Vittalis.Models
{
    public enum AccountType
    {
        CheckingAccount,    // Conta Corrente
        SavingsAccount,     // Poupança
        CreditCard,         // Cartão de Crédito
        Cash,               // Dinheiro
        Investment,         // Investimentos
        Other               // Outras
    }

    public class Account
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public AccountType Type { get; set; }

        public decimal InitialBalance { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegação
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

        // Propriedade calculada - saldo atual
        public decimal CurrentBalance => InitialBalance +
            Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount) -
            Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
    }
}
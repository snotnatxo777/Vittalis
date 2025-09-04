using System.ComponentModel.DataAnnotations;

namespace Vittalis.Models
{
    public enum TransactionType
    {
        Income,
        Expense
    }

    public enum TransactionCategory
    {
        // Receitas
        Salary,
        Freelance,
        Investment,
        Gift,
        OtherIncome,
        // Despesas
        Food,
        Transportation,
        Housing,
        Health,
        Education,
        Entertainment,
        Shopping,
        Bills,
        Travel,
        OtherExpense
    }

    public class Transaction
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public TransactionCategory Category { get; set; }

        [Required]
        public int AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;

        public int? RecurringTransactionId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
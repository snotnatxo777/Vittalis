using System.ComponentModel.DataAnnotations;

namespace Vittalis.Models
{
    public enum RecurrenceFrequency
    {
        Weekly,     // Semanal
        Monthly,    // Mensal
        Quarterly,  // Trimestral
        Yearly      // Anual
    }

    public class RecurringTransaction
    {
        public int Id { get; set; }

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public TransactionCategory Category { get; set; }

        [Required]
        public int AccountId { get; set; }

        public virtual Account Account { get; set; } = null!;

        [Required]
        public RecurrenceFrequency Frequency { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public DateTime NextOccurrence { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegação para transações geradas
        public virtual ICollection<Transaction> GeneratedTransactions { get; set; } = new List<Transaction>();
    }
}
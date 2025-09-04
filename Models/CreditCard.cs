using System.ComponentModel.DataAnnotations;

namespace Vittalis.Models
{
    public class CreditCard
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty; // Ex: "Cartão Nubank"

        [Required]
        public string LastFourDigits { get; set; } = string.Empty; // Últimos 4 dígitos

        [Required]
        public decimal CreditLimit { get; set; }

        [Required]
        public int ClosingDay { get; set; } // Dia do fechamento da fatura

        [Required]
        public int DueDay { get; set; } // Dia do vencimento

        public int? LinkedAccountId { get; set; } // Conta bancária vinculada (opcional)

        public virtual Account? LinkedAccount { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegação
        public virtual ICollection<CreditCardTransaction> Transactions { get; set; } = new List<CreditCardTransaction>();
        public virtual ICollection<Installment> Installments { get; set; } = new List<Installment>();

        // Propriedades calculadas
        public decimal CurrentBalance { get; set; } // Será calculada pelo serviço
        public decimal AvailableLimit => CreditLimit - CurrentBalance;
        public double UsagePercentage => CreditLimit > 0 ? (double)(CurrentBalance / CreditLimit * 100) : 0;
    }

    public class CreditCardTransaction
    {
        public int Id { get; set; }

        [Required]
        public int CreditCardId { get; set; }

        public virtual CreditCard CreditCard { get; set; } = null!;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal Amount { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; }

        [Required]
        public TransactionCategory Category { get; set; }

        public int? InstallmentId { get; set; } // Null se for à vista

        public virtual Installment? Installment { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Installment
    {
        public int Id { get; set; }

        [Required]
        public int CreditCardId { get; set; }

        public virtual CreditCard CreditCard { get; set; } = null!;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal TotalAmount { get; set; }

        [Required]
        public decimal InstallmentAmount { get; set; }

        [Required]
        public int TotalInstallments { get; set; }

        [Required]
        public int PaidInstallments { get; set; } = 0;

        [Required]
        public DateTime FirstInstallmentDate { get; set; }

        [Required]
        public TransactionCategory Category { get; set; }

        public bool IsCompleted => PaidInstallments >= TotalInstallments;

        public int RemainingInstallments => Math.Max(0, TotalInstallments - PaidInstallments);

        public decimal RemainingAmount => RemainingInstallments * InstallmentAmount;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegação
        public virtual ICollection<CreditCardTransaction> Transactions { get; set; } = new List<CreditCardTransaction>();
    }

    public class CreditCardBill
    {
        public int Id { get; set; }

        [Required]
        public int CreditCardId { get; set; }

        public virtual CreditCard CreditCard { get; set; } = null!;

        [Required]
        public DateTime ClosingDate { get; set; }

        [Required]
        public DateTime DueDate { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public decimal PaidAmount { get; set; } = 0;

        public bool IsPaid => PaidAmount >= TotalAmount;

        public decimal RemainingAmount => TotalAmount - PaidAmount;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Lista de transações desta fatura
        public List<CreditCardTransaction> BillTransactions { get; set; } = new();
    }
}
using System.ComponentModel.DataAnnotations;

namespace Vittalis.Models
{
    public class SpendingGoal
    {
        public int Id { get; set; }

        [Required]
        public TransactionCategory Category { get; set; }

        [Required]
        public decimal MonthlyLimit { get; set; }

        [Required]
        public int Year { get; set; }

        [Required]
        public int Month { get; set; }

        public int? AccountId { get; set; } // Opcional - meta específica para uma conta

        public virtual Account? Account { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriedades calculadas (serão preenchidas pelo serviço)
        public decimal CurrentSpent { get; set; } = 0;

        public decimal RemainingAmount => MonthlyLimit - CurrentSpent;

        public double ProgressPercentage => MonthlyLimit > 0 ? (double)(CurrentSpent / MonthlyLimit * 100) : 0;

        public bool IsOverBudget => CurrentSpent > MonthlyLimit;

        public string ProgressStatus
        {
            get
            {
                var percentage = ProgressPercentage;
                return percentage switch
                {
                    <= 50 => "Dentro do Orçamento",
                    <= 80 => "Atenção",
                    <= 100 => "Próximo do Limite",
                    _ => "Acima do Orçamento"
                };
            }
        }

        public Color ProgressColor
        {
            get
            {
                var percentage = ProgressPercentage;
                return percentage switch
                {
                    <= 50 => Colors.Green,
                    <= 80 => Colors.Orange,
                    <= 100 => Colors.Red,
                    _ => Colors.DarkRed
                };
            }
        }
    }
}
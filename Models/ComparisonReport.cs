namespace Vittalis.Models
{
    public class PeriodComparison
    {
        public string PeriodName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
        public decimal Balance { get; set; }
        public decimal SavingsRate => TotalIncome > 0 ? (Balance / TotalIncome) * 100 : 0;
    }

    public class CategoryComparison
    {
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public decimal CurrentAmount { get; set; }
        public decimal PreviousAmount { get; set; }
        public decimal Difference => CurrentAmount - PreviousAmount;
        public double ChangePercentage => PreviousAmount > 0 ? (double)((Difference / PreviousAmount) * 100) : 0;
        public string TrendIndicator => Difference > 0 ? "📈" : Difference < 0 ? "📉" : "➡️";
        public Color TrendColor => Difference > 0 ? Colors.Red : Difference < 0 ? Colors.Green : Colors.Gray;
    }

    public class ComparisonReport
    {
        public PeriodComparison CurrentPeriod { get; set; } = new();
        public PeriodComparison PreviousPeriod { get; set; } = new();
        public List<CategoryComparison> ExpenseComparisons { get; set; } = new();
        public List<CategoryComparison> IncomeComparisons { get; set; } = new();

        // Comparações gerais
        public decimal IncomeDifference => CurrentPeriod.TotalIncome - PreviousPeriod.TotalIncome;
        public decimal ExpenseDifference => CurrentPeriod.TotalExpense - PreviousPeriod.TotalExpense;
        public decimal BalanceDifference => CurrentPeriod.Balance - PreviousPeriod.Balance;

        public double IncomeChangePercentage => PreviousPeriod.TotalIncome > 0 ?
            (double)((IncomeDifference / PreviousPeriod.TotalIncome) * 100) : 0;

        public double ExpenseChangePercentage => PreviousPeriod.TotalExpense > 0 ?
            (double)((ExpenseDifference / PreviousPeriod.TotalExpense) * 100) : 0;

        public string OverallTrend
        {
            get
            {
                if (BalanceDifference > 0) return "Melhoria";
                if (BalanceDifference < 0) return "Piora";
                return "Estável";
            }
        }

        public Color OverallTrendColor
        {
            get
            {
                if (BalanceDifference > 0) return Colors.Green;
                if (BalanceDifference < 0) return Colors.Red;
                return Colors.Gray;
            }
        }
    }
}
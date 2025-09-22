using System.ComponentModel.DataAnnotations;

namespace Vittalis.Models
{
    // ENUMS NECESSÁRIOS
    public enum TrendDirection
    {
        Up,
        Down,
        Stable
    }

    public enum FinancialHealthScore
    {
        Excellent = 5,
        Good = 4,
        Fair = 3,
        Poor = 2,
        Critical = 1
    }

    public enum AlertSeverity
    {
        Info,
        Warning,
        Critical
    }

    // Projeção financeira
    public class FinancialProjection
    {
        public DateTime Date { get; set; }
        public decimal ProjectedIncome { get; set; }
        public decimal ProjectedExpense { get; set; }
        public decimal ProjectedBalance { get; set; }
        public decimal ProjectedSavings { get; set; }
        public string ProjectionType { get; set; } = string.Empty; // "Conservative", "Optimistic", "Realistic"
    }

    // Análise de tendências
    public class TrendAnalysis
    {
        public string Period { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public decimal PreviousValue { get; set; }
        public decimal TrendValue => Value - PreviousValue;
        public double TrendPercentage => PreviousValue != 0 ? (double)((TrendValue / PreviousValue) * 100) : 0;
        public TrendDirection Direction => TrendValue > 0 ? TrendDirection.Up : TrendValue < 0 ? TrendDirection.Down : TrendDirection.Stable;
        public Color TrendColor => Direction == TrendDirection.Up ? Colors.Green : Direction == TrendDirection.Down ? Colors.Red : Colors.Gray;
    }

    // Indicadores de saúde financeira
    public class FinancialHealthIndicators
    {
        public decimal SavingsRate { get; set; } // % de poupança
        public decimal DebtToIncomeRatio { get; set; } // Relação dívida/renda
        public decimal EmergencyFundMonths { get; set; } // Meses de reserva de emergência
        public decimal ExpenseVariability { get; set; } // Variabilidade dos gastos
        public FinancialHealthScore OverallScore { get; set; }
        public List<HealthAlert> Alerts { get; set; } = new();
        public string Summary { get; set; } = string.Empty;
    }

    public class HealthAlert
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public AlertSeverity Severity { get; set; }
        public string Icon { get; set; } = string.Empty;
        public Color Color { get; set; }
    }

    // Análise de categorias
    public class CategoryAnalysis
    {
        public TransactionCategory Category { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryIcon { get; set; } = string.Empty;
        public decimal CurrentAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal BestMonth { get; set; }
        public decimal WorstMonth { get; set; }
        public List<MonthlyValue> MonthlyHistory { get; set; } = new();
        public double GrowthRate { get; set; }
        public string Recommendation { get; set; } = string.Empty;
    }

    public class MonthlyValue
    {
        public string Month { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }

    // Cashflow detalhado
    public class CashFlowAnalysis
    {
        public List<CashFlowPeriod> Periods { get; set; } = new();
        public decimal AverageMonthlyIncome { get; set; }
        public decimal AverageMonthlyExpense { get; set; }
        public decimal PredictedCashFlow { get; set; }
        public int PositiveMonths { get; set; }
        public int NegativeMonths { get; set; }
        public decimal LargestInflow { get; set; }
        public decimal LargestOutflow { get; set; }
    }

    public class CashFlowPeriod
    {
        public string PeriodName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal NetFlow { get; set; }
        public decimal AccumulatedFlow { get; set; }
    }

    // Relatório de investimentos avançado
    public class AdvancedInvestmentReport
    {
        public decimal TotalInvested { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalReturn { get; set; }
        public double TotalReturnPercentage { get; set; }
        public decimal DividendsReceived { get; set; }
        public double AssetAllocation { get; set; }
        public List<AssetPerformance> AssetPerformances { get; set; } = new();
        public List<MonthlyPerformance> MonthlyPerformances { get; set; } = new();
        public string RiskProfile { get; set; } = string.Empty;
        public List<string> Recommendations { get; set; } = new();
    }

    public class AssetPerformance
    {
        public string AssetSymbol { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public decimal Invested { get; set; }
        public decimal CurrentValue { get; set; }
        public double ReturnPercentage { get; set; }
        public double Weight { get; set; } // % do portfolio
        public AssetType Type { get; set; }
        public string Performance { get; set; } = string.Empty; // "Excellent", "Good", "Poor"
    }

    public class MonthlyPerformance
    {
        public string Month { get; set; } = string.Empty;
        public decimal PortfolioValue { get; set; }
        public double MonthlyReturn { get; set; }
        public double AccumulatedReturn { get; set; }
    }

    // Relatório personalizado
    public class CustomReport
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime LastGenerated { get; set; }
        public CustomReportSettings Settings { get; set; } = new();
        public bool IsActive { get; set; } = true;
    }

    public class CustomReportSettings
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<int> SelectedAccountIds { get; set; } = new();
        public List<TransactionCategory> SelectedCategories { get; set; } = new();
        public List<string> SelectedMetrics { get; set; } = new(); // "Income", "Expense", "Balance", etc.
        public string GroupBy { get; set; } = string.Empty; // "Month", "Category", "Account"
        public string ChartType { get; set; } = string.Empty; // "Line", "Bar", "Pie"
        public bool IncludeProjections { get; set; }
        public bool IncludeTrends { get; set; }
    }
}
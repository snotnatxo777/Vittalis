using Vittalis.Models;
using Vittalis.Data;
using Vittalis.Helpers;

namespace Vittalis.Services
{
    public interface IAdvancedReportService
    {
        // Projeções financeiras
        Task<List<FinancialProjection>> GetFinancialProjectionsAsync(int months = 12, int? accountId = null);
        Task<List<FinancialProjection>> GetScenarioProjectionsAsync(decimal incomeVariation, decimal expenseVariation, int months = 12);

        // Análise de tendências
        Task<List<TrendAnalysis>> GetIncomeTrendsAsync(int periods = 12, string periodType = "monthly");
        Task<List<TrendAnalysis>> GetExpenseTrendsAsync(int periods = 12, string periodType = "monthly");
        Task<List<TrendAnalysis>> GetCategoryTrendsAsync(TransactionCategory category, int periods = 12);

        // Indicadores de saúde financeira
        Task<FinancialHealthIndicators> GetFinancialHealthAsync(int? accountId = null);
        Task<List<HealthAlert>> GetFinancialAlertsAsync();

        // Análise de categorias
        Task<List<CategoryAnalysis>> GetCategoryAnalysisAsync(int months = 12, TransactionType? type = null);
        Task<CategoryAnalysis> GetSpecificCategoryAnalysisAsync(TransactionCategory category, int months = 12);

        // Cash flow avançado
        Task<CashFlowAnalysis> GetCashFlowAnalysisAsync(DateTime startDate, DateTime endDate, string groupBy = "monthly");
        Task<decimal> PredictCashFlowAsync(DateTime targetDate);

        // Investimentos avançados
        Task<AdvancedInvestmentReport> GetAdvancedInvestmentReportAsync();
        Task<List<AssetPerformance>> GetTopPerformingAssetsAsync(int count = 5);
        Task<List<AssetPerformance>> GetWorstPerformingAssetsAsync(int count = 5);

        // Relatórios personalizados
        Task<CustomReport> CreateCustomReportAsync(CustomReport report);
        Task<List<CustomReport>> GetCustomReportsAsync();
        Task<object> GenerateCustomReportDataAsync(int reportId);
        Task<bool> DeleteCustomReportAsync(int reportId);
    }

    public class AdvancedReportService : IAdvancedReportService
    {
        private readonly ITransactionService _transactionService;
        private readonly IAccountService _accountService;
        private readonly IInvestmentService _investmentService;
        private readonly VittalisDbContext _context;

        public AdvancedReportService(
            ITransactionService transactionService,
            IAccountService accountService,
            IInvestmentService investmentService,
            VittalisDbContext context)
        {
            _transactionService = transactionService;
            _accountService = accountService;
            _investmentService = investmentService;
            _context = context;
        }

        public async Task<List<FinancialProjection>> GetFinancialProjectionsAsync(int months = 12, int? accountId = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();

            if (accountId.HasValue)
                transactions = transactions.Where(t => t.AccountId == accountId.Value).ToList();

            // Calcular médias dos últimos 6 meses
            var recentTransactions = transactions
                .Where(t => t.Date >= DateTime.Now.AddMonths(-6))
                .ToList();

            var avgMonthlyIncome = recentTransactions
                .Where(t => t.Type == TransactionType.Income)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .DefaultIfEmpty()
                .Average(g => g?.Sum(t => t.Amount) ?? 0);

            var avgMonthlyExpense = recentTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .DefaultIfEmpty()
                .Average(g => g?.Sum(t => t.Amount) ?? 0);

            var projections = new List<FinancialProjection>();
            var currentDate = DateTime.Now.Date;
            var runningBalance = await GetCurrentBalanceAsync(accountId);

            for (int i = 1; i <= months; i++)
            {
                var projectionDate = currentDate.AddMonths(i);

                // Cenários: Conservador, Realista, Otimista
                var scenarios = new[]
                {
                    new { Type = "Conservative", IncomeMultiplier = 0.9m, ExpenseMultiplier = 1.1m },
                    new { Type = "Realistic", IncomeMultiplier = 1.0m, ExpenseMultiplier = 1.0m },
                    new { Type = "Optimistic", IncomeMultiplier = 1.1m, ExpenseMultiplier = 0.9m }
                };

                foreach (var scenario in scenarios)
                {
                    var projectedIncome = avgMonthlyIncome * scenario.IncomeMultiplier;
                    var projectedExpense = avgMonthlyExpense * scenario.ExpenseMultiplier;
                    var projectedBalance = runningBalance + (projectedIncome - projectedExpense) * i;

                    projections.Add(new FinancialProjection
                    {
                        Date = projectionDate,
                        ProjectedIncome = projectedIncome,
                        ProjectedExpense = projectedExpense,
                        ProjectedBalance = projectedBalance,
                        ProjectedSavings = projectedIncome - projectedExpense,
                        ProjectionType = scenario.Type
                    });
                }
            }

            return projections;
        }

        public async Task<List<TrendAnalysis>> GetIncomeTrendsAsync(int periods = 12, string periodType = "monthly")
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var incomeTransactions = transactions.Where(t => t.Type == TransactionType.Income).ToList();

            var trends = new List<TrendAnalysis>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < periods; i++)
            {
                DateTime startDate, endDate;
                string periodName;

                if (periodType == "monthly")
                {
                    startDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-i);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    periodName = startDate.ToString("MMM yyyy");
                }
                else // weekly
                {
                    startDate = currentDate.AddDays(-7 * i).AddDays(-(int)currentDate.DayOfWeek);
                    endDate = startDate.AddDays(6);
                    periodName = $"Semana {startDate:dd/MM}";
                }

                var periodIncome = incomeTransactions
                    .Where(t => t.Date >= startDate && t.Date <= endDate)
                    .Sum(t => t.Amount);

                var previousPeriodIncome = i < periods - 1 ?
                    incomeTransactions
                        .Where(t => t.Date >= startDate.AddMonths(-1) && t.Date <= endDate.AddMonths(-1))
                        .Sum(t => t.Amount) : 0;

                trends.Add(new TrendAnalysis
                {
                    Period = periodName,
                    Value = periodIncome,
                    PreviousValue = previousPeriodIncome
                });
            }

            return trends.OrderBy(t => t.Period).ToList();
        }

        public async Task<List<TrendAnalysis>> GetExpenseTrendsAsync(int periods = 12, string periodType = "monthly")
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var expenseTransactions = transactions.Where(t => t.Type == TransactionType.Expense).ToList();

            var trends = new List<TrendAnalysis>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < periods; i++)
            {
                DateTime startDate, endDate;
                string periodName;

                if (periodType == "monthly")
                {
                    startDate = new DateTime(currentDate.Year, currentDate.Month, 1).AddMonths(-i);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    periodName = startDate.ToString("MMM yyyy");
                }
                else // weekly
                {
                    startDate = currentDate.AddDays(-7 * i).AddDays(-(int)currentDate.DayOfWeek);
                    endDate = startDate.AddDays(6);
                    periodName = $"Semana {startDate:dd/MM}";
                }

                var periodExpense = expenseTransactions
                    .Where(t => t.Date >= startDate && t.Date <= endDate)
                    .Sum(t => t.Amount);

                var previousPeriodExpense = i < periods - 1 ?
                    expenseTransactions
                        .Where(t => t.Date >= startDate.AddMonths(-1) && t.Date <= endDate.AddMonths(-1))
                        .Sum(t => t.Amount) : 0;

                trends.Add(new TrendAnalysis
                {
                    Period = periodName,
                    Value = periodExpense,
                    PreviousValue = previousPeriodExpense
                });
            }

            return trends.OrderBy(t => t.Period).ToList();
        }

        public async Task<FinancialHealthIndicators> GetFinancialHealthAsync(int? accountId = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();

            if (accountId.HasValue)
                transactions = transactions.Where(t => t.AccountId == accountId.Value).ToList();

            var recentTransactions = transactions
                .Where(t => t.Date >= DateTime.Now.AddMonths(-12))
                .ToList();

            var totalIncome = recentTransactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            var totalExpense = recentTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var monthlyIncome = totalIncome / 12;
            var monthlyExpense = totalExpense / 12;
            var savingsRate = totalIncome > 0 ? ((totalIncome - totalExpense) / totalIncome) * 100 : 0;

            // Calcular variabilidade dos gastos
            var monthlyExpenses = recentTransactions
                .Where(t => t.Type == TransactionType.Expense)
                .GroupBy(t => new { t.Date.Year, t.Date.Month })
                .Select(g => g.Sum(t => t.Amount))
                .ToList();

            var expenseVariability = monthlyExpenses.Any() ?
                (decimal)Math.Sqrt(monthlyExpenses.Select(e => Math.Pow((double)(e - monthlyExpense), 2)).Average()) : 0;

            // Reserva de emergência (saldo atual / gasto mensal)
            var currentBalance = await GetCurrentBalanceAsync(accountId);
            var emergencyFundMonths = monthlyExpense > 0 ? currentBalance / monthlyExpense : 0;

            // Calcular score geral
            var score = CalculateHealthScore(savingsRate, emergencyFundMonths, expenseVariability);

            var alerts = new List<HealthAlert>();

            // Gerar alertas
            if (savingsRate < 10)
                alerts.Add(new HealthAlert
                {
                    Title = "Taxa de Poupança Baixa",
                    Message = "Sua taxa de poupança está abaixo de 10%. Considere reduzir gastos ou aumentar renda.",
                    Severity = AlertSeverity.Warning,
                    Icon = "⚠️",
                    Color = Colors.Orange
                });

            if (emergencyFundMonths < 3)
                alerts.Add(new HealthAlert
                {
                    Title = "Reserva de Emergência Insuficiente",
                    Message = "Recomenda-se ter pelo menos 3 meses de gastos como reserva.",
                    Severity = AlertSeverity.Critical,
                    Icon = "🚨",
                    Color = Colors.Red
                });

            return new FinancialHealthIndicators
            {
                SavingsRate = savingsRate,
                DebtToIncomeRatio = 0, // Implementar se houver sistema de dívidas
                EmergencyFundMonths = emergencyFundMonths,
                ExpenseVariability = expenseVariability,
                OverallScore = score,
                Alerts = alerts,
                Summary = GenerateHealthSummary(score, savingsRate, emergencyFundMonths)
            };
        }

        public async Task<List<CategoryAnalysis>> GetCategoryAnalysisAsync(int months = 12, TransactionType? type = null)
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var recentTransactions = transactions
                .Where(t => t.Date >= DateTime.Now.AddMonths(-months))
                .ToList();

            if (type.HasValue)
                recentTransactions = recentTransactions.Where(t => t.Type == type.Value).ToList();

            var categoryAnalyses = new List<CategoryAnalysis>();
            var categories = recentTransactions.Select(t => t.Category).Distinct();

            foreach (var category in categories)
            {
                var categoryTransactions = recentTransactions
                    .Where(t => t.Category == category)
                    .ToList();

                var monthlyAmounts = categoryTransactions
                    .GroupBy(t => new { t.Date.Year, t.Date.Month })
                    .Select(g => g.Sum(t => t.Amount))
                    .ToList();

                var analysis = new CategoryAnalysis
                {
                    Category = category,
                    CategoryName = CategoryHelper.GetCategoryName(category),
                    CategoryIcon = CategoryHelper.GetCategoryIcon(category),
                    CurrentAmount = categoryTransactions.Sum(t => t.Amount),
                    AverageAmount = monthlyAmounts.Any() ? monthlyAmounts.Average() : 0,
                    BestMonth = monthlyAmounts.Any() ? monthlyAmounts.Min() : 0,
                    WorstMonth = monthlyAmounts.Any() ? monthlyAmounts.Max() : 0,
                    MonthlyHistory = GenerateMonthlyHistory(categoryTransactions, months),
                    GrowthRate = CalculateGrowthRate(monthlyAmounts),
                    Recommendation = GenerateRecommendation(category, monthlyAmounts)
                };

                categoryAnalyses.Add(analysis);
            }

            return categoryAnalyses.OrderByDescending(c => c.CurrentAmount).ToList();
        }

        public async Task<CashFlowAnalysis> GetCashFlowAnalysisAsync(DateTime startDate, DateTime endDate, string groupBy = "monthly")
        {
            var transactions = await _transactionService.GetAllTransactionsAsync();
            var filteredTransactions = transactions
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .ToList();

            var periods = new List<CashFlowPeriod>();
            var current = startDate;
            var accumulatedFlow = 0m;

            while (current <= endDate)
            {
                DateTime periodEnd;
                string periodName;

                if (groupBy == "monthly")
                {
                    periodEnd = new DateTime(current.Year, current.Month, DateTime.DaysInMonth(current.Year, current.Month));
                    periodName = current.ToString("MMM yyyy");
                }
                else // weekly
                {
                    periodEnd = current.AddDays(6);
                    periodName = $"Semana {current:dd/MM}";
                }

                if (periodEnd > endDate) periodEnd = endDate;

                var periodTransactions = filteredTransactions
                    .Where(t => t.Date >= current && t.Date <= periodEnd)
                    .ToList();

                var income = periodTransactions
                    .Where(t => t.Type == TransactionType.Income)
                    .Sum(t => t.Amount);

                var expense = periodTransactions
                    .Where(t => t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                var netFlow = income - expense;
                accumulatedFlow += netFlow;

                periods.Add(new CashFlowPeriod
                {
                    PeriodName = periodName,
                    StartDate = current,
                    EndDate = periodEnd,
                    Income = income,
                    Expense = expense,
                    NetFlow = netFlow,
                    AccumulatedFlow = accumulatedFlow
                });

                current = groupBy == "monthly" ? current.AddMonths(1) : current.AddDays(7);
            }

            return new CashFlowAnalysis
            {
                Periods = periods,
                AverageMonthlyIncome = periods.Any() ? periods.Average(p => p.Income) : 0,
                AverageMonthlyExpense = periods.Any() ? periods.Average(p => p.Expense) : 0,
                PredictedCashFlow = periods.LastOrDefault()?.AccumulatedFlow ?? 0,
                PositiveMonths = periods.Count(p => p.NetFlow > 0),
                NegativeMonths = periods.Count(p => p.NetFlow < 0),
                LargestInflow = periods.Any() ? periods.Max(p => p.Income) : 0,
                LargestOutflow = periods.Any() ? periods.Max(p => p.Expense) : 0
            };
        }

        // Métodos auxiliares privados
        private async Task<decimal> GetCurrentBalanceAsync(int? accountId)
        {
            if (accountId.HasValue)
            {
                return await _accountService.GetAccountBalanceAsync(accountId.Value);
            }

            var accounts = await _accountService.GetAllAccountsAsync();
            return accounts.Sum(a => a.CurrentBalance);
        }

        private FinancialHealthScore CalculateHealthScore(decimal savingsRate, decimal emergencyFund, decimal variability)
        {
            var score = 0;

            // Taxa de poupança (40% do score)
            if (savingsRate >= 20) score += 2;
            else if (savingsRate >= 10) score += 1;

            // Reserva de emergência (40% do score)
            if (emergencyFund >= 6) score += 2;
            else if (emergencyFund >= 3) score += 1;

            // Estabilidade dos gastos (20% do score)
            if (variability < 500) score += 1;

            return score switch
            {
                5 => FinancialHealthScore.Excellent,
                4 => FinancialHealthScore.Good,
                3 => FinancialHealthScore.Fair,
                2 => FinancialHealthScore.Poor,
                _ => FinancialHealthScore.Critical
            };
        }

        private string GenerateHealthSummary(FinancialHealthScore score, decimal savingsRate, decimal emergencyFund)
        {
            return score switch
            {
                FinancialHealthScore.Excellent => "Excelente! Sua saúde financeira está ótima. Continue assim!",
                FinancialHealthScore.Good => "Boa situação financeira. Pequenos ajustes podem melhorar ainda mais.",
                FinancialHealthScore.Fair => "Situação razoável. Foque em aumentar a poupança e reserva de emergência.",
                FinancialHealthScore.Poor => "Atenção necessária. Revise seus gastos e estabeleça metas de economia.",
                _ => "Situação crítica. É urgente reorganizar suas finanças."
            };
        }

        private List<MonthlyValue> GenerateMonthlyHistory(List<Transaction> transactions, int months)
        {
            var history = new List<MonthlyValue>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < months; i++)
            {
                var monthDate = currentDate.AddMonths(-i);
                var monthTransactions = transactions
                    .Where(t => t.Date.Year == monthDate.Year && t.Date.Month == monthDate.Month)
                    .Sum(t => t.Amount);

                history.Add(new MonthlyValue
                {
                    Month = monthDate.ToString("MMM yyyy"),
                    Value = monthTransactions
                });
            }

            return history.OrderBy(h => h.Month).ToList();
        }

        private double CalculateGrowthRate(List<decimal> values)
        {
            if (values.Count < 2) return 0;

            var firstValue = values.First();
            var lastValue = values.Last();

            return firstValue != 0 ? (double)((lastValue - firstValue) / firstValue * 100) : 0;
        }

        private string GenerateRecommendation(TransactionCategory category, List<decimal> monthlyAmounts)
        {
            if (!monthlyAmounts.Any()) return "Sem dados suficientes para recomendação.";

            var average = monthlyAmounts.Average();
            var trend = monthlyAmounts.Last() - monthlyAmounts.First();

            return category switch
            {
                TransactionCategory.Food when trend > average * 0.1m => "Considere planejar melhor as refeições para controlar gastos.",
                TransactionCategory.Transportation when trend > 0 => "Analise alternativas de transporte mais econômicas.",
                TransactionCategory.Entertainment when monthlyAmounts.Last() > average * 1.5m => "Gastos com lazer estão acima da média. Considere moderação.",
                _ => trend > 0 ? "Monitore este categoria - gastos estão aumentando." : "Categoria sob controle."
            };
        }

        // Implementações básicas dos outros métodos da interface
        public Task<List<FinancialProjection>> GetScenarioProjectionsAsync(decimal incomeVariation, decimal expenseVariation, int months = 12)
        {
            return GetFinancialProjectionsAsync(months);
        }

        public Task<List<TrendAnalysis>> GetCategoryTrendsAsync(TransactionCategory category, int periods = 12)
        {
            return GetIncomeTrendsAsync(periods);
        }

        public Task<List<HealthAlert>> GetFinancialAlertsAsync()
        {
            return Task.FromResult(new List<HealthAlert>());
        }

        public Task<CategoryAnalysis> GetSpecificCategoryAnalysisAsync(TransactionCategory category, int months = 12)
        {
            return Task.FromResult(new CategoryAnalysis());
        }

        public Task<decimal> PredictCashFlowAsync(DateTime targetDate)
        {
            return Task.FromResult(0m);
        }

        public Task<AdvancedInvestmentReport> GetAdvancedInvestmentReportAsync()
        {
            return Task.FromResult(new AdvancedInvestmentReport());
        }

        public Task<List<AssetPerformance>> GetTopPerformingAssetsAsync(int count = 5)
        {
            return Task.FromResult(new List<AssetPerformance>());
        }

        public Task<List<AssetPerformance>> GetWorstPerformingAssetsAsync(int count = 5)
        {
            return Task.FromResult(new List<AssetPerformance>());
        }

        public Task<CustomReport> CreateCustomReportAsync(CustomReport report)
        {
            return Task.FromResult(report);
        }

        public Task<List<CustomReport>> GetCustomReportsAsync()
        {
            return Task.FromResult(new List<CustomReport>());
        }

        public Task<object> GenerateCustomReportDataAsync(int reportId)
        {
            return Task.FromResult(new object());
        }

        public Task<bool> DeleteCustomReportAsync(int reportId)
        {
            return Task.FromResult(true);
        }
    }
}
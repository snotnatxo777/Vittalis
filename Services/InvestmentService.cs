using Microsoft.EntityFrameworkCore;
using Vittalis.Data;
using Vittalis.Models;

namespace Vittalis.Services
{
    public interface IInvestmentService
    {
        // Gestão de Ativos
        Task<List<Asset>> GetAllAssetsAsync();
        Task<Asset> AddAssetAsync(Asset asset);
        Task<Asset?> GetAssetByIdAsync(int id);
        Task<Asset?> GetAssetBySymbolAsync(string symbol);
        Task<bool> UpdateAssetAsync(Asset asset);
        Task<bool> DeleteAssetAsync(int id);
        Task<List<Asset>> SearchAssetsAsync(string query);

        // Gestão de Trades
        Task<List<Trade>> GetAllTradesAsync();
        Task<Trade> AddTradeAsync(Trade trade);
        Task<bool> DeleteTradeAsync(int tradeId);
        Task<List<Trade>> GetTradesByAssetAsync(int assetId);
        Task<List<Trade>> GetTradesByDateRangeAsync(DateTime startDate, DateTime endDate);

        // Portfolio
        Task<List<PortfolioPosition>> GetPortfolioAsync();
        Task<PortfolioSummary> GetPortfolioSummaryAsync();
        Task<decimal> GetTotalPortfolioValueAsync();
        Task<decimal> GetTotalInvestedAsync();
        Task<decimal> GetTotalProfitLossAsync();
        Task<decimal> GetTotalDividendsAsync();

        // Corretoras
        Task<List<Broker>> GetAllBrokersAsync();
        Task<Broker> AddBrokerAsync(Broker broker);
        Task<bool> UpdateBrokerAsync(Broker broker);
        Task<bool> DeleteBrokerAsync(int brokerId);

        // Dividendos
        Task<List<Dividend>> GetDividendsAsync();
        Task<Dividend> AddDividendAsync(Dividend dividend);
        Task<decimal> GetDividendsByAssetAsync(int assetId);
        Task<decimal> GetDividendsByYearAsync(int year);

        // Histórico de Preços
        Task<List<AssetPrice>> GetPriceHistoryAsync(int assetId, DateTime? startDate = null, DateTime? endDate = null);
        Task<AssetPrice> AddPriceAsync(AssetPrice price);
        Task UpdateAssetPricesAsync(); // Atualização automática de preços

        // Metas de Investimento
        Task<List<InvestmentGoal>> GetInvestmentGoalsAsync();
        Task<InvestmentGoal> AddInvestmentGoalAsync(InvestmentGoal goal);
        Task<bool> UpdateInvestmentGoalAsync(InvestmentGoal goal);
        Task<bool> DeleteInvestmentGoalAsync(int goalId);

        // Watch Lists
        Task<List<WatchList>> GetWatchListsAsync();
        Task<WatchList> AddWatchListAsync(WatchList watchList);
        Task<bool> UpdateWatchListAsync(WatchList watchList);
        Task<bool> DeleteWatchListAsync(int watchListId);

        // Alertas de Preço
        Task<List<PriceAlert>> GetPriceAlertsAsync();
        Task<PriceAlert> AddPriceAlertAsync(PriceAlert alert);
        Task<bool> DeletePriceAlertAsync(int alertId);
        Task CheckPriceAlertsAsync();

        // Análises e Relatórios
        Task<Dictionary<AssetType, decimal>> GetAllocationByTypeAsync();
        Task<Dictionary<string, decimal>> GetAllocationBySectorAsync();
        Task<List<PortfolioPosition>> GetTopPerformersAsync(int count = 5);
        Task<List<PortfolioPosition>> GetWorstPerformersAsync(int count = 5);
        Task<TaxReport> GenerateTaxReportAsync(int year);

        // Performance
        Task<decimal> GetPortfolioPerformanceAsync(int months = 12);
        Task<Dictionary<string, decimal>> GetMonthlyPerformanceAsync(int months = 12);
    }

    public class InvestmentService : IInvestmentService
    {
        private readonly VittalisDbContext _context;

        public InvestmentService(VittalisDbContext context)
        {
            _context = context;
        }

        #region Gestão de Ativos

        public async Task<List<Asset>> GetAllAssetsAsync()
        {
            return await _context.Assets
                .Include(a => a.Trades)
                .Include(a => a.Dividends)
                .Where(a => a.Status == AssetStatus.Active)
                .OrderBy(a => a.Symbol)
                .ToListAsync();
        }

        public async Task<Asset> AddAssetAsync(Asset asset)
        {
            asset.CreatedAt = DateTime.UtcNow;
            asset.LastPriceUpdate = DateTime.UtcNow;

            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
            return asset;
        }

        public async Task<Asset?> GetAssetByIdAsync(int id)
        {
            return await _context.Assets
                .Include(a => a.Trades)
                .Include(a => a.Dividends)
                .Include(a => a.PriceHistory)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Asset?> GetAssetBySymbolAsync(string symbol)
        {
            return await _context.Assets
                .Include(a => a.Trades)
                .Include(a => a.Dividends)
                .FirstOrDefaultAsync(a => a.Symbol.ToUpper() == symbol.ToUpper());
        }

        public async Task<bool> UpdateAssetAsync(Asset asset)
        {
            _context.Entry(asset).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteAssetAsync(int id)
        {
            var asset = await _context.Assets.FindAsync(id);
            if (asset == null) return false;

            asset.Status = AssetStatus.Inactive;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<Asset>> SearchAssetsAsync(string query)
        {
            return await _context.Assets
                .Where(a => a.Symbol.Contains(query) || a.Name.Contains(query))
                .Where(a => a.Status == AssetStatus.Active)
                .OrderBy(a => a.Symbol)
                .ToListAsync();
        }

        #endregion

        #region Gestão de Trades

        public async Task<List<Trade>> GetAllTradesAsync()
        {
            return await _context.Trades
                .Include(t => t.Asset)
                .Include(t => t.Broker)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<Trade> AddTradeAsync(Trade trade)
        {
            trade.CreatedAt = DateTime.UtcNow;

            _context.Trades.Add(trade);
            await _context.SaveChangesAsync();

            // Atualizar preço do ativo se necessário
            var asset = await _context.Assets.FindAsync(trade.AssetId);
            if (asset != null && trade.Date >= asset.LastPriceUpdate.Date)
            {
                asset.PreviousPrice = asset.CurrentPrice;
                asset.CurrentPrice = trade.Price;
                asset.LastPriceUpdate = trade.Date;
                await _context.SaveChangesAsync();
            }

            return trade;
        }

        public async Task<bool> DeleteTradeAsync(int tradeId)
        {
            var trade = await _context.Trades.FindAsync(tradeId);
            if (trade == null) return false;

            _context.Trades.Remove(trade);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<List<Trade>> GetTradesByAssetAsync(int assetId)
        {
            return await _context.Trades
                .Include(t => t.Asset)
                .Include(t => t.Broker)
                .Where(t => t.AssetId == assetId)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<List<Trade>> GetTradesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Trades
                .Include(t => t.Asset)
                .Include(t => t.Broker)
                .Where(t => t.Date >= startDate && t.Date <= endDate)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        #endregion

        #region Portfolio

        public async Task<List<PortfolioPosition>> GetPortfolioAsync()
        {
            var trades = await _context.Trades
                .Include(t => t.Asset)
                .ToListAsync();

            var portfolio = new List<PortfolioPosition>();
            var groupedByAsset = trades.GroupBy(t => t.Asset);

            foreach (var assetGroup in groupedByAsset)
            {
                var asset = assetGroup.Key;
                var assetTrades = assetGroup.ToList();

                var totalBought = assetTrades.Where(t => t.Type == TradeType.Buy).Sum(t => t.Quantity);
                var totalSold = assetTrades.Where(t => t.Type == TradeType.Sell).Sum(t => t.Quantity);
                var currentQuantity = totalBought - totalSold;

                if (currentQuantity <= 0) continue;

                var buyTrades = assetTrades.Where(t => t.Type == TradeType.Buy);
                var totalInvested = buyTrades.Sum(t => t.GrossValue + t.TotalCosts);
                var averagePrice = totalBought > 0 ? totalInvested / totalBought : 0;

                var currentValue = currentQuantity * asset.CurrentPrice;
                var profitLoss = currentValue - (currentQuantity * averagePrice);
                var profitLossPercentage = averagePrice > 0 ? (profitLoss / (currentQuantity * averagePrice)) * 100 : 0;

                // Calcular dividendos recebidos
                var dividendsReceived = await GetDividendsByAssetAsync(asset.Id);

                portfolio.Add(new PortfolioPosition
                {
                    AssetSymbol = asset.Symbol,
                    AssetName = asset.Name,
                    AssetType = asset.Type,
                    TotalQuantity = currentQuantity,
                    AveragePrice = averagePrice,
                    CurrentPrice = asset.CurrentPrice,
                    TotalInvested = currentQuantity * averagePrice,
                    CurrentValue = currentValue,
                    ProfitLoss = profitLoss,
                    ProfitLossPercentage = profitLossPercentage,
                    DividendsReceived = dividendsReceived,
                    // TODO: Calcular métricas de risco (Beta, Volatilidade, etc.)
                    Beta = 1.0,
                    Volatility = 0.2,
                    SharpeRatio = 0.5
                });
            }

            // Calcular peso no portfolio
            var totalPortfolioValue = portfolio.Sum(p => p.CurrentValue);
            foreach (var position in portfolio)
            {
                position.WeightInPortfolio = totalPortfolioValue > 0 ?
                    (double)(position.CurrentValue / totalPortfolioValue * 100) : 0;
            }

            return portfolio.OrderByDescending(p => p.CurrentValue).ToList();
        }

        public async Task<PortfolioSummary> GetPortfolioSummaryAsync()
        {
            var portfolio = await GetPortfolioAsync();
            var trades = await GetAllTradesAsync();

            var summary = new PortfolioSummary
            {
                TotalInvested = portfolio.Sum(p => p.TotalInvested),
                CurrentValue = portfolio.Sum(p => p.CurrentValue),
                TotalProfitLoss = portfolio.Sum(p => p.ProfitLoss),
                TotalDividends = portfolio.Sum(p => p.DividendsReceived),
                TotalAssets = portfolio.Count,
                TotalTrades = trades.Count
            };

            summary.TotalReturn = summary.TotalProfitLoss + summary.TotalDividends;
            summary.TotalProfitLossPercentage = summary.TotalInvested > 0 ?
                (summary.TotalProfitLoss / summary.TotalInvested) * 100 : 0;
            summary.TotalReturnPercentage = summary.TotalInvested > 0 ?
                (summary.TotalReturn / summary.TotalInvested) * 100 : 0;

            // Alocação por tipo de ativo
            summary.AllocationByType = portfolio
                .GroupBy(p => p.AssetType)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.CurrentValue));

            // Outras métricas
            summary.AverageTradeSize = trades.Any() ? trades.Average(t => t.GrossValue) : 0;
            summary.LargestPosition = portfolio.Any() ? portfolio.Max(p => p.CurrentValue) : 0;
            summary.SmallestPosition = portfolio.Any() ? portfolio.Min(p => p.CurrentValue) : 0;

            return summary;
        }

        public async Task<decimal> GetTotalPortfolioValueAsync()
        {
            var portfolio = await GetPortfolioAsync();
            return portfolio.Sum(p => p.CurrentValue);
        }

        public async Task<decimal> GetTotalInvestedAsync()
        {
            var portfolio = await GetPortfolioAsync();
            return portfolio.Sum(p => p.TotalInvested);
        }

        public async Task<decimal> GetTotalProfitLossAsync()
        {
            var portfolio = await GetPortfolioAsync();
            return portfolio.Sum(p => p.ProfitLoss);
        }

        public async Task<decimal> GetTotalDividendsAsync()
        {
            return await _context.Dividends.SumAsync(d => d.AmountPerShare);
        }

        #endregion

        #region Corretoras

        public async Task<List<Broker>> GetAllBrokersAsync()
        {
            return await _context.Brokers
                .Where(b => b.IsActive)
                .OrderBy(b => b.Name)
                .ToListAsync();
        }

        public async Task<Broker> AddBrokerAsync(Broker broker)
        {
            _context.Brokers.Add(broker);
            await _context.SaveChangesAsync();
            return broker;
        }

        public async Task<bool> UpdateBrokerAsync(Broker broker)
        {
            _context.Entry(broker).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteBrokerAsync(int brokerId)
        {
            var broker = await _context.Brokers.FindAsync(brokerId);
            if (broker == null) return false;

            broker.IsActive = false;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region Dividendos

        public async Task<List<Dividend>> GetDividendsAsync()
        {
            return await _context.Dividends
                .Include(d => d.Asset)
                .OrderByDescending(d => d.PaymentDate)
                .ToListAsync();
        }

        public async Task<Dividend> AddDividendAsync(Dividend dividend)
        {
            dividend.CreatedAt = DateTime.UtcNow;
            _context.Dividends.Add(dividend);
            await _context.SaveChangesAsync();
            return dividend;
        }

        public async Task<decimal> GetDividendsByAssetAsync(int assetId)
        {
            return await _context.Dividends
                .Where(d => d.AssetId == assetId)
                .SumAsync(d => d.AmountPerShare);
        }

        public async Task<decimal> GetDividendsByYearAsync(int year)
        {
            return await _context.Dividends
                .Where(d => d.PaymentDate.Year == year)
                .SumAsync(d => d.AmountPerShare);
        }

        #endregion

        #region Histórico de Preços

        public async Task<List<AssetPrice>> GetPriceHistoryAsync(int assetId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.AssetPrices.Where(p => p.AssetId == assetId);

            if (startDate.HasValue)
                query = query.Where(p => p.Date >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.Date <= endDate.Value);

            return await query.OrderByDescending(p => p.Date).ToListAsync();
        }

        public async Task<AssetPrice> AddPriceAsync(AssetPrice price)
        {
            price.CreatedAt = DateTime.UtcNow;
            _context.AssetPrices.Add(price);
            await _context.SaveChangesAsync();
            return price;
        }

        public async Task UpdateAssetPricesAsync()
        {
            // TODO: Implementar integração com API de cotações
            // Por enquanto, simulação de atualização
            var assets = await _context.Assets.Where(a => a.Status == AssetStatus.Active).ToListAsync();

            foreach (var asset in assets)
            {
                // Simular variação de preço aleatória
                var random = new Random();
                var variation = (decimal)(random.NextDouble() * 0.1 - 0.05); // -5% a +5%

                asset.PreviousPrice = asset.CurrentPrice;
                asset.CurrentPrice = Math.Max(0.01m, asset.CurrentPrice * (1 + variation));
                asset.LastPriceUpdate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Metas de Investimento

        public async Task<List<InvestmentGoal>> GetInvestmentGoalsAsync()
        {
            return await _context.InvestmentGoals
                .Where(g => g.IsActive)
                .OrderBy(g => g.TargetDate)
                .ToListAsync();
        }

        public async Task<InvestmentGoal> AddInvestmentGoalAsync(InvestmentGoal goal)
        {
            goal.CreatedAt = DateTime.UtcNow;
            _context.InvestmentGoals.Add(goal);
            await _context.SaveChangesAsync();
            return goal;
        }

        public async Task<bool> UpdateInvestmentGoalAsync(InvestmentGoal goal)
        {
            _context.Entry(goal).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteInvestmentGoalAsync(int goalId)
        {
            var goal = await _context.InvestmentGoals.FindAsync(goalId);
            if (goal == null) return false;

            goal.IsActive = false;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region Watch Lists

        public async Task<List<WatchList>> GetWatchListsAsync()
        {
            return await _context.WatchLists
                .OrderBy(w => w.Name)
                .ToListAsync();
        }

        public async Task<WatchList> AddWatchListAsync(WatchList watchList)
        {
            watchList.CreatedAt = DateTime.UtcNow;
            _context.WatchLists.Add(watchList);
            await _context.SaveChangesAsync();
            return watchList;
        }

        public async Task<bool> UpdateWatchListAsync(WatchList watchList)
        {
            _context.Entry(watchList).State = EntityState.Modified;
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> DeleteWatchListAsync(int watchListId)
        {
            var watchList = await _context.WatchLists.FindAsync(watchListId);
            if (watchList == null) return false;

            _context.WatchLists.Remove(watchList);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        #endregion

        #region Alertas de Preço

        public async Task<List<PriceAlert>> GetPriceAlertsAsync()
        {
            return await _context.PriceAlerts
                .Include(a => a.Asset)
                .Where(a => a.IsActive && !a.IsTriggered)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<PriceAlert> AddPriceAlertAsync(PriceAlert alert)
        {
            alert.CreatedAt = DateTime.UtcNow;
            _context.PriceAlerts.Add(alert);
            await _context.SaveChangesAsync();
            return alert;
        }

        public async Task<bool> DeletePriceAlertAsync(int alertId)
        {
            var alert = await _context.PriceAlerts.FindAsync(alertId);
            if (alert == null) return false;

            _context.PriceAlerts.Remove(alert);
            var result = await _context.SaveChangesAsync();
            return result > 0;
        }

        public async Task CheckPriceAlertsAsync()
        {
            var activeAlerts = await _context.PriceAlerts
                .Include(a => a.Asset)
                .Where(a => a.IsActive && !a.IsTriggered)
                .ToListAsync();

            foreach (var alert in activeAlerts)
            {
                bool shouldTrigger = alert.Condition switch
                {
                    AlertCondition.Above => alert.Asset.CurrentPrice >= alert.TargetPrice,
                    AlertCondition.Below => alert.Asset.CurrentPrice <= alert.TargetPrice,
                    AlertCondition.Equals => Math.Abs(alert.Asset.CurrentPrice - alert.TargetPrice) < 0.01m,
                    _ => false
                };

                if (shouldTrigger)
                {
                    alert.IsTriggered = true;
                    alert.TriggeredAt = DateTime.UtcNow;
                }
            }

            await _context.SaveChangesAsync();
        }

        #endregion

        #region Análises e Relatórios

        public async Task<Dictionary<AssetType, decimal>> GetAllocationByTypeAsync()
        {
            var portfolio = await GetPortfolioAsync();
            return portfolio
                .GroupBy(p => p.AssetType)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.CurrentValue));
        }

        public async Task<Dictionary<string, decimal>> GetAllocationBySectorAsync()
        {
            var portfolio = await GetPortfolioAsync();
            var assets = await _context.Assets.ToListAsync();

            var allocation = new Dictionary<string, decimal>();

            foreach (var position in portfolio)
            {
                var asset = assets.FirstOrDefault(a => a.Symbol == position.AssetSymbol);
                var sector = asset?.Sector ?? "Outros";

                if (allocation.ContainsKey(sector))
                    allocation[sector] += position.CurrentValue;
                else
                    allocation[sector] = position.CurrentValue;
            }

            return allocation;
        }

        public async Task<List<PortfolioPosition>> GetTopPerformersAsync(int count = 5)
        {
            var portfolio = await GetPortfolioAsync();
            return portfolio
                .OrderByDescending(p => p.ProfitLossPercentage)
                .Take(count)
                .ToList();
        }

        public async Task<List<PortfolioPosition>> GetWorstPerformersAsync(int count = 5)
        {
            var portfolio = await GetPortfolioAsync();
            return portfolio
                .OrderBy(p => p.ProfitLossPercentage)
                .Take(count)
                .ToList();
        }

        public async Task<TaxReport> GenerateTaxReportAsync(int year)
        {
            var trades = await _context.Trades
                .Include(t => t.Asset)
                .Where(t => t.Date.Year == year)
                .OrderBy(t => t.Date)
                .ToListAsync();

            var dividends = await _context.Dividends
                .Where(d => d.PaymentDate.Year == year)
                .SumAsync(d => d.AmountPerShare);

            var report = new TaxReport
            {
                Year = year,
                TotalDividends = dividends,
                Transactions = new List<TaxTransaction>()
            };

            // TODO: Implementar cálculo detalhado de impostos
            // Por enquanto, estrutura básica

            foreach (var trade in trades)
            {
                report.Transactions.Add(new TaxTransaction
                {
                    AssetSymbol = trade.Asset.Symbol,
                    Date = trade.Date,
                    Type = trade.Type,
                    Quantity = trade.Quantity,
                    Price = trade.Price,
                    ProfitLoss = 0, // Calcular baseado no preço médio
                    IsDayTrade = false // Determinar se é day trade
                });
            }

            return report;
        }

        #endregion

        #region Performance

        public async Task<decimal> GetPortfolioPerformanceAsync(int months = 12)
        {
            var currentValue = await GetTotalPortfolioValueAsync();
            var invested = await GetTotalInvestedAsync();

            return invested > 0 ? ((currentValue - invested) / invested) * 100 : 0;
        }

        public async Task<Dictionary<string, decimal>> GetMonthlyPerformanceAsync(int months = 12)
        {
            var performance = new Dictionary<string, decimal>();
            var currentDate = DateTime.Now;

            for (int i = 0; i < months; i++)
            {
                var date = currentDate.AddMonths(-i);
                var monthName = date.ToString("MMM yyyy");

                // TODO: Calcular performance real baseada no histórico de preços
                // Por enquanto, valor simulado
                performance[monthName] = (decimal)(new Random().NextDouble() * 20 - 10); // -10% a +10%
            }

            return performance.OrderBy(kvp => kvp.Key).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }

        #endregion
    }
}
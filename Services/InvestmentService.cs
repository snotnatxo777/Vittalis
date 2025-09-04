using Microsoft.EntityFrameworkCore;
using Vittalis.Data;
using Vittalis.Models;

namespace Vittalis.Services
{
    public interface IInvestmentService
    {
        Task<List<Asset>> GetAllAssetsAsync();
        Task<Asset> AddAssetAsync(Asset asset);
        Task<List<Trade>> GetAllTradesAsync();
        Task<Trade> AddTradeAsync(Trade trade);
        Task<bool> DeleteTradeAsync(int tradeId);
        Task<List<Portfolio>> GetPortfolioAsync();
        Task<decimal> GetTotalPortfolioValueAsync();
        Task<decimal> GetTotalInvestedAsync();
        Task<decimal> GetTotalProfitLossAsync();
    }

    public class InvestmentService : IInvestmentService
    {
        private readonly VittalisDbContext _context;

        public InvestmentService(VittalisDbContext context)
        {
            _context = context;
        }

        public async Task<List<Asset>> GetAllAssetsAsync()
        {
            return await _context.Assets.OrderBy(a => a.Symbol).ToListAsync();
        }

        public async Task<Asset> AddAssetAsync(Asset asset)
        {
            asset.CreatedAt = DateTime.UtcNow;
            _context.Assets.Add(asset);
            await _context.SaveChangesAsync();
            return asset;
        }

        public async Task<List<Trade>> GetAllTradesAsync()
        {
            return await _context.Trades
                .Include(t => t.Asset)
                .OrderByDescending(t => t.Date)
                .ToListAsync();
        }

        public async Task<Trade> AddTradeAsync(Trade trade)
        {
            trade.CreatedAt = DateTime.UtcNow;
            _context.Trades.Add(trade);
            await _context.SaveChangesAsync();
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

        public async Task<List<Portfolio>> GetPortfolioAsync()
        {
            var trades = await _context.Trades
                .Include(t => t.Asset)
                .ToListAsync();

            var portfolio = new List<Portfolio>();

            var groupedByAsset = trades.GroupBy(t => t.Asset);

            foreach (var assetGroup in groupedByAsset)
            {
                var asset = assetGroup.Key;
                var assetTrades = assetGroup.ToList();

                var totalBought = assetTrades.Where(t => t.Type == TradeType.Buy).Sum(t => t.Quantity);
                var totalSold = assetTrades.Where(t => t.Type == TradeType.Sell).Sum(t => t.Quantity);
                var currentQuantity = totalBought - totalSold;

                if (currentQuantity <= 0) continue; // Não mostrar ativos zerados

                var buyTrades = assetTrades.Where(t => t.Type == TradeType.Buy);
                var totalInvested = buyTrades.Sum(t => t.TotalCost);
                var averagePrice = totalBought > 0 ? totalInvested / totalBought : 0;

                var currentValue = currentQuantity * asset.CurrentPrice;
                var profitLoss = currentValue - (currentQuantity * averagePrice);
                var profitLossPercentage = averagePrice > 0 ? (profitLoss / (currentQuantity * averagePrice)) * 100 : 0;

                portfolio.Add(new Portfolio
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
                    ProfitLossPercentage = profitLossPercentage
                });
            }

            return portfolio.OrderByDescending(p => p.CurrentValue).ToList();
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
    }
}
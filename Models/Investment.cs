using System.ComponentModel.DataAnnotations;

namespace Vittalis.Models
{
    public enum AssetType
    {
        Stock,      // Ações
        Fund,       // Fundos
        RealEstate, // FII
        Crypto,     // Criptomoedas
        ETF,        // ETF
        Bond        // Títulos
    }

    public enum TradeType
    {
        Buy,
        Sell
    }

    public class Asset
    {
        public int Id { get; set; }

        [Required]
        public string Symbol { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public AssetType Type { get; set; }

        public decimal CurrentPrice { get; set; }
        public DateTime LastPriceUpdate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegação
        public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
    }

    public class Trade
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;

        [Required]
        public TradeType Type { get; set; }

        [Required]
        public int Quantity { get; set; }

        [Required]
        public decimal Price { get; set; }

        public decimal Fees { get; set; } = 0;

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriedades calculadas
        public decimal TotalCost => (Quantity * Price) + Fees;
        public decimal NetValue => Type == TradeType.Buy ? -TotalCost : (Quantity * Price) - Fees;
    }

    public class Portfolio
    {
        public string AssetSymbol { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public AssetType AssetType { get; set; }
        public int TotalQuantity { get; set; }
        public decimal AveragePrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal TotalInvested { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal ProfitLoss { get; set; }
        public decimal ProfitLossPercentage { get; set; }
    }
}
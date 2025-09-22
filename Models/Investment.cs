using System.ComponentModel.DataAnnotations;

namespace Vittalis.Models
{
    public enum AssetType
    {
        Stock,          // Ações
        Fund,           // Fundos de Investimento
        RealEstate,     // FII - Fundos Imobiliários
        Crypto,         // Criptomoedas
        ETF,            // ETF
        Bond,           // Títulos/Renda Fixa
        Commodity,      // Commodities
        Option,         // Opções
        Future,         // Futuros
        Treasury,       // Tesouro Direto
        CDB,            // CDB
        LCI_LCA,        // LCI/LCA
        Debenture       // Debêntures
    }

    public enum TradeType
    {
        Buy,            // Compra
        Sell,           // Venda
        Dividend,       // Dividendo
        Bonus,          // Bonificação
        Split,          // Desdobramento
        GroupingStock,  // Grupamento
        Subscription    // Subscrição
    }

    public enum AssetStatus
    {
        Active,         // Ativo
        Inactive,       // Inativo
        Delisted,       // Deslistado
        Suspended       // Suspenso
    }

    public enum BrokerType
    {
        Clear,
        XP,
        BTG,
        Inter,
        Rico,
        Modal,
        Easynvest,
        Avenue,
        C6Bank,
        Itau,
        BB,
        Bradesco,
        Santander,
        Nubank,
        Other
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

        public string CNPJ { get; set; } = string.Empty;
        public string ISIN { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Sector { get; set; } = string.Empty;
        public string Segment { get; set; } = string.Empty;
        public string Exchange { get; set; } = "B3"; // B3, NYSE, NASDAQ, etc.

        public decimal CurrentPrice { get; set; }
        public decimal PreviousPrice { get; set; }
        public decimal DailyChange => CurrentPrice - PreviousPrice;
        public double DailyChangePercentage => PreviousPrice != 0 ? (double)((DailyChange / PreviousPrice) * 100) : 0;

        // Dados fundamentalistas (para ações)
        public decimal? MarketCap { get; set; }
        public double? PE { get; set; }          // P/L
        public double? PB { get; set; }          // P/VP
        public double? ROE { get; set; }         // ROE
        public double? ROA { get; set; }         // ROA
        public double? DividendYield { get; set; }
        public decimal? Revenue { get; set; }    // Receita
        public decimal? NetIncome { get; set; }  // Lucro Líquido
        public decimal? BookValue { get; set; }  // Patrimônio Líquido

        // Dados de renda fixa
        public DateTime? MaturityDate { get; set; }
        public double? InterestRate { get; set; }
        public string? Indexer { get; set; }     // CDI, IPCA, SELIC, etc.

        public AssetStatus Status { get; set; } = AssetStatus.Active;
        public DateTime LastPriceUpdate { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navegação
        public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
        public virtual ICollection<Dividend> Dividends { get; set; } = new List<Dividend>();
        public virtual ICollection<AssetPrice> PriceHistory { get; set; } = new List<AssetPrice>();
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

        public decimal Fees { get; set; } = 0;          // Taxas de corretagem
        public decimal Taxes { get; set; } = 0;         // Impostos
        public decimal EmolumentFees { get; set; } = 0; // Emolumentos
        public decimal OtherCosts { get; set; } = 0;    // Outros custos

        [Required]
        public DateTime Date { get; set; } = DateTime.Now;

        public int? BrokerId { get; set; }
        public virtual Broker? Broker { get; set; }

        public string Notes { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;  // ID da ordem na corretora

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriedades calculadas
        public decimal TotalCosts => Fees + Taxes + EmolumentFees + OtherCosts;
        public decimal GrossValue => Quantity * Price;
        public decimal NetValue => Type == TradeType.Buy ?
            -(GrossValue + TotalCosts) :
            (GrossValue - TotalCosts);
    }

    public class Broker
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public BrokerType Type { get; set; }
        public string CNPJ { get; set; } = string.Empty;
        public decimal DefaultFeePercentage { get; set; } = 0;
        public decimal FixedFee { get; set; } = 0;
        public bool IsActive { get; set; } = true;

        // Navegação
        public virtual ICollection<Trade> Trades { get; set; } = new List<Trade>();
    }

    public class Dividend
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;

        [Required]
        public decimal AmountPerShare { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; }

        public DateTime ExDividendDate { get; set; }
        public DateTime DeclarationDate { get; set; }

        public DividendType Type { get; set; } = DividendType.Dividend;
        public string Description { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum DividendType
    {
        Dividend,           // Dividendo
        JCP,               // Juros sobre Capital Próprio
        Bonus,             // Bonificação
        Subscription       // Direito de Subscrição
    }

    public class AssetPrice
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;

        [Required]
        public DateTime Date { get; set; }

        [Required]
        public decimal OpenPrice { get; set; }

        [Required]
        public decimal ClosePrice { get; set; }

        [Required]
        public decimal HighPrice { get; set; }

        [Required]
        public decimal LowPrice { get; set; }

        public long Volume { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    // Classes para análises avançadas
    public class PortfolioPosition
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
        public double WeightInPortfolio { get; set; }
        public decimal DividendsReceived { get; set; }
        public decimal TotalReturn => ProfitLoss + DividendsReceived;
        public decimal TotalReturnPercentage => TotalInvested != 0 ? (TotalReturn / TotalInvested) * 100 : 0;

        // Métricas de risco
        public double Beta { get; set; }
        public double Volatility { get; set; }
        public double SharpeRatio { get; set; }

        // Performance
        public decimal Performance1M { get; set; }
        public decimal Performance3M { get; set; }
        public decimal Performance6M { get; set; }
        public decimal Performance1Y { get; set; }
        public decimal PerformanceYTD { get; set; }
    }

    public class PortfolioSummary
    {
        public decimal TotalInvested { get; set; }
        public decimal CurrentValue { get; set; }
        public decimal TotalProfitLoss { get; set; }
        public decimal TotalProfitLossPercentage { get; set; }
        public decimal TotalDividends { get; set; }
        public decimal TotalReturn { get; set; }
        public decimal TotalReturnPercentage { get; set; }

        // Diversificação
        public Dictionary<AssetType, decimal> AllocationByType { get; set; } = new();
        public Dictionary<string, decimal> AllocationBySector { get; set; } = new();
        public Dictionary<BrokerType, decimal> AllocationByBroker { get; set; } = new();

        // Métricas de performance
        public decimal Performance1M { get; set; }
        public decimal Performance3M { get; set; }
        public decimal Performance6M { get; set; }
        public decimal Performance1Y { get; set; }
        public decimal PerformanceYTD { get; set; }

        // Métricas de risco
        public double PortfolioBeta { get; set; }
        public double PortfolioVolatility { get; set; }
        public double SharpeRatio { get; set; }
        public double MaxDrawdown { get; set; }

        // Estatísticas
        public int TotalAssets { get; set; }
        public int TotalTrades { get; set; }
        public decimal AverageTradeSize { get; set; }
        public decimal LargestPosition { get; set; }
        public decimal SmallestPosition { get; set; }
    }

    public class InvestmentGoal
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal TargetAmount { get; set; }

        public DateTime TargetDate { get; set; }

        public decimal CurrentAmount { get; set; }

        public decimal MonthlyContribution { get; set; }

        public GoalType Type { get; set; }

        public List<int> AssetIds { get; set; } = new(); // Ativos relacionados a esta meta

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propriedades calculadas
        public decimal ProgressPercentage => TargetAmount != 0 ? (CurrentAmount / TargetAmount) * 100 : 0;
        public int MonthsRemaining => (int)Math.Ceiling((TargetDate - DateTime.Now).TotalDays / 30);
        public decimal MonthlyNeeded => MonthsRemaining > 0 ? (TargetAmount - CurrentAmount) / MonthsRemaining : 0;
    }

    public enum GoalType
    {
        Retirement,         // Aposentadoria
        Emergency,          // Reserva de Emergência
        House,              // Casa Própria
        Car,                // Carro
        Travel,             // Viagem
        Education,          // Educação
        Wedding,            // Casamento
        Other               // Outros
    }

    public class WatchList
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public List<int> AssetIds { get; set; } = new();

        public bool IsDefault { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class PriceAlert
    {
        public int Id { get; set; }

        [Required]
        public int AssetId { get; set; }
        public virtual Asset Asset { get; set; } = null!;

        [Required]
        public decimal TargetPrice { get; set; }

        [Required]
        public AlertCondition Condition { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsTriggered { get; set; } = false;
        public DateTime? TriggeredAt { get; set; }

        public string Notes { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public enum AlertCondition
    {
        Above,      // Acima
        Below,      // Abaixo
        Equals      // Igual
    }

    public class TaxReport
    {
        public int Year { get; set; }
        public decimal ProfitFromDayTrade { get; set; }
        public decimal LossFromDayTrade { get; set; }
        public decimal ProfitFromSwingTrade { get; set; }
        public decimal LossFromSwingTrade { get; set; }
        public decimal TaxOwed { get; set; }
        public decimal TotalDividends { get; set; }
        public List<TaxTransaction> Transactions { get; set; } = new();
    }

    public class TaxTransaction
    {
        public string AssetSymbol { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public TradeType Type { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal ProfitLoss { get; set; }
        public bool IsDayTrade { get; set; }
    }
}
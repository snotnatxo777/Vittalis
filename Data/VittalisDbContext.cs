using Microsoft.EntityFrameworkCore;
using Vittalis.Models;

namespace Vittalis.Data
{
    public class VittalisDbContext : DbContext
    {
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Asset> Assets { get; set; }
        public DbSet<Trade> Trades { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<RecurringTransaction> RecurringTransactions { get; set; }
        public DbSet<SpendingGoal> SpendingGoals { get; set; }
        public DbSet<CreditCard> CreditCards { get; set; }
        public DbSet<CreditCardTransaction> CreditCardTransactions { get; set; }
        public DbSet<Installment> Installments { get; set; }
        public DbSet<CreditCardBill> CreditCardBills { get; set; }
        public DbSet<Broker> Brokers { get; set; }
        public DbSet<Dividend> Dividends { get; set; }
        public DbSet<AssetPrice> AssetPrices { get; set; }
        public DbSet<InvestmentGoal> InvestmentGoals { get; set; }
        public DbSet<WatchList> WatchLists { get; set; }
        public DbSet<PriceAlert> PriceAlerts { get; set; }

        public VittalisDbContext(DbContextOptions<VittalisDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configurações da tabela Account
            modelBuilder.Entity<Account>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.InitialBalance).HasColumnType("decimal(18,2)");
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Configurações da tabela Transaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relacionamento com Account
                entity.HasOne(e => e.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(e => e.AccountId);
            });

            // Configurações para Asset
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Symbol).IsRequired().HasMaxLength(10);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18,4)");
            });

            // Configurações para Trade
            modelBuilder.Entity<Trade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,4)");
                entity.Property(e => e.Fees).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.HasOne(e => e.Asset)
                      .WithMany(a => a.Trades)
                      .HasForeignKey(e => e.AssetId);
            });

            // Configurações da tabela RecurringTransaction
            modelBuilder.Entity<RecurringTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.Frequency).IsRequired();
                entity.Property(e => e.StartDate).IsRequired();
                entity.Property(e => e.NextOccurrence).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relacionamento com Account
                entity.HasOne(e => e.Account)
                      .WithMany()
                      .HasForeignKey(e => e.AccountId);
            });

            // Atualizar configuração de Transaction para incluir relacionamento com RecurringTransaction
            modelBuilder.Entity<Transaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Date).IsRequired();
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relacionamento com Account
                entity.HasOne(e => e.Account)
                      .WithMany(a => a.Transactions)
                      .HasForeignKey(e => e.AccountId);

                // Opcional: relacionamento com RecurringTransaction
                entity.Property(e => e.RecurringTransactionId).IsRequired(false);
            });

            // Configurações da tabela SpendingGoal
            modelBuilder.Entity<SpendingGoal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Category).IsRequired();
                entity.Property(e => e.MonthlyLimit).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.Year).IsRequired();
                entity.Property(e => e.Month).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();

                // Relacionamento opcional com Account
                entity.HasOne(e => e.Account)
                      .WithMany()
                      .HasForeignKey(e => e.AccountId)
                      .IsRequired(false);

                // Índice único para categoria + mês + ano + conta
                entity.HasIndex(e => new { e.Category, e.Year, e.Month, e.AccountId })
                      .IsUnique();
            });

            // Configurações para CreditCard
            modelBuilder.Entity<CreditCard>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.LastFourDigits).IsRequired().HasMaxLength(4);
                entity.Property(e => e.CreditLimit).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.ClosingDay).IsRequired();
                entity.Property(e => e.DueDay).IsRequired();

                // Relacionamento com Account
                entity.HasOne(e => e.LinkedAccount)
                      .WithMany()
                      .HasForeignKey(e => e.LinkedAccountId)
                      .IsRequired(false);
            });

            // Configurações para CreditCardTransaction
            modelBuilder.Entity<CreditCardTransaction>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Amount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.TransactionDate).IsRequired();
                entity.Property(e => e.Category).IsRequired();

                entity.HasOne(e => e.CreditCard)
                      .WithMany(c => c.Transactions)
                      .HasForeignKey(e => e.CreditCardId);

                entity.HasOne(e => e.Installment)
                      .WithMany(i => i.Transactions)
                      .HasForeignKey(e => e.InstallmentId)
                      .IsRequired(false);
            });

            // Configurações para Installment
            modelBuilder.Entity<Installment>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.InstallmentAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.TotalInstallments).IsRequired();
                entity.Property(e => e.PaidInstallments).IsRequired();
                entity.Property(e => e.FirstInstallmentDate).IsRequired();
                entity.Property(e => e.Category).IsRequired();

                entity.HasOne(e => e.CreditCard)
                      .WithMany(c => c.Installments)
                      .HasForeignKey(e => e.CreditCardId);
            });

            // Configurações para CreditCardBill
            modelBuilder.Entity<CreditCardBill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.ClosingDate).IsRequired();
                entity.Property(e => e.DueDate).IsRequired();
                entity.Property(e => e.TotalAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.PaidAmount).HasColumnType("decimal(18,2)");

                entity.HasOne(e => e.CreditCard)
                      .WithMany()
                      .HasForeignKey(e => e.CreditCardId);
            });

            // Configurações para Broker
            modelBuilder.Entity<Broker>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.CNPJ).HasMaxLength(20);
                entity.Property(e => e.DefaultFeePercentage).HasColumnType("decimal(5,4)");
                entity.Property(e => e.FixedFee).HasColumnType("decimal(10,2)");
            });

            // Configurações para Dividend
            modelBuilder.Entity<Dividend>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AmountPerShare).IsRequired().HasColumnType("decimal(18,4)");
                entity.HasOne(e => e.Asset)
                      .WithMany(a => a.Dividends)
                      .HasForeignKey(e => e.AssetId);
            });

            // Configurações para AssetPrice
            modelBuilder.Entity<AssetPrice>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.OpenPrice).IsRequired().HasColumnType("decimal(18,4)");
                entity.Property(e => e.ClosePrice).IsRequired().HasColumnType("decimal(18,4)");
                entity.Property(e => e.HighPrice).IsRequired().HasColumnType("decimal(18,4)");
                entity.Property(e => e.LowPrice).IsRequired().HasColumnType("decimal(18,4)");
                entity.HasOne(e => e.Asset)
                      .WithMany(a => a.PriceHistory)
                      .HasForeignKey(e => e.AssetId);
            });

            // Configurações para InvestmentGoal
            modelBuilder.Entity<InvestmentGoal>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.TargetAmount).IsRequired().HasColumnType("decimal(18,2)");
                entity.Property(e => e.CurrentAmount).HasColumnType("decimal(18,2)");
                entity.Property(e => e.MonthlyContribution).HasColumnType("decimal(18,2)");
                entity.Property(e => e.AssetIds)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList());
            });

            // Configurações para WatchList
            modelBuilder.Entity<WatchList>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.AssetIds)
                      .HasConversion(
                          v => string.Join(',', v),
                          v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(int.Parse).ToList());
            });

            // Configurações para PriceAlert
            modelBuilder.Entity<PriceAlert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.TargetPrice).IsRequired().HasColumnType("decimal(18,4)");
                entity.HasOne(e => e.Asset)
                      .WithMany()
                      .HasForeignKey(e => e.AssetId);
            });

            // Atualizar configuração de Trade para incluir Broker
            modelBuilder.Entity<Trade>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.Quantity).IsRequired();
                entity.Property(e => e.Price).IsRequired().HasColumnType("decimal(18,4)");
                entity.Property(e => e.Fees).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Taxes).HasColumnType("decimal(18,2)");
                entity.Property(e => e.EmolumentFees).HasColumnType("decimal(18,2)");
                entity.Property(e => e.OtherCosts).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Notes).HasMaxLength(500);
                entity.Property(e => e.OrderId).HasMaxLength(50);

                entity.HasOne(e => e.Asset)
                      .WithMany(a => a.Trades)
                      .HasForeignKey(e => e.AssetId);

                entity.HasOne(e => e.Broker)
                      .WithMany(b => b.Trades)
                      .HasForeignKey(e => e.BrokerId)
                      .IsRequired(false);
            });

            // Atualizar configuração de Asset
            modelBuilder.Entity<Asset>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Symbol).IsRequired().HasMaxLength(20);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).IsRequired();
                entity.Property(e => e.CNPJ).HasMaxLength(20);
                entity.Property(e => e.ISIN).HasMaxLength(20);
                entity.Property(e => e.Sector).HasMaxLength(100);
                entity.Property(e => e.Segment).HasMaxLength(100);
                entity.Property(e => e.Exchange).HasMaxLength(50);
                entity.Property(e => e.CurrentPrice).HasColumnType("decimal(18,4)");
                entity.Property(e => e.PreviousPrice).HasColumnType("decimal(18,4)");
                entity.Property(e => e.MarketCap).HasColumnType("decimal(20,2)");
                entity.Property(e => e.Revenue).HasColumnType("decimal(20,2)");
                entity.Property(e => e.NetIncome).HasColumnType("decimal(20,2)");
                entity.Property(e => e.BookValue).HasColumnType("decimal(20,2)");
            });
        }
    }
}
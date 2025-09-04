using Vittalis.Models;
using Microsoft.EntityFrameworkCore;

namespace Vittalis.Data
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(VittalisDbContext context)
        {
            // Deletar e recriar o banco com nova estrutura
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // Criar contas padrão
            var accounts = new[]
            {
                new Account { Name = "Conta Corrente", Type = AccountType.CheckingAccount, InitialBalance = 0 },
                new Account { Name = "Poupança", Type = AccountType.SavingsAccount, InitialBalance = 0 },
                new Account { Name = "Cartão de Crédito", Type = AccountType.CreditCard, InitialBalance = 0 },
                new Account { Name = "Dinheiro", Type = AccountType.Cash, InitialBalance = 0 }
            };

            context.Accounts.AddRange(accounts);
            await context.SaveChangesAsync();

            // Buscar a primeira conta para usar nas transações de exemplo
            var defaultAccount = accounts[0];

            // Adicionar dados de exemplo com contas
            var sampleTransactions = new[]
            {
                new Transaction
                {
                    Description = "Salário",
                    Amount = 3500.00m,
                    Date = DateTime.Now.AddDays(-5),
                    Type = TransactionType.Income,
                    Category = TransactionCategory.Salary,
                    AccountId = defaultAccount.Id
                },
                new Transaction
                {
                    Description = "Supermercado",
                    Amount = 250.50m,
                    Date = DateTime.Now.AddDays(-3),
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.Food,
                    AccountId = defaultAccount.Id
                },
                new Transaction
                {
                    Description = "Freelance",
                    Amount = 800.00m,
                    Date = DateTime.Now.AddDays(-1),
                    Type = TransactionType.Income,
                    Category = TransactionCategory.Freelance,
                    AccountId = defaultAccount.Id
                },
                new Transaction
                {
                    Description = "Gasolina",
                    Amount = 120.00m,
                    Date = DateTime.Now,
                    Type = TransactionType.Expense,
                    Category = TransactionCategory.Transportation,
                    AccountId = defaultAccount.Id
                }
            };

            context.Transactions.AddRange(sampleTransactions);
            await context.SaveChangesAsync();

            // Adicionar ativos de exemplo
            var sampleAssets = new[]
            {
                new Asset { Symbol = "PETR4", Name = "Petrobras", Type = AssetType.Stock, CurrentPrice = 32.50m },
                new Asset { Symbol = "VALE3", Name = "Vale", Type = AssetType.Stock, CurrentPrice = 65.80m },
                new Asset { Symbol = "ITUB4", Name = "Itaú Unibanco", Type = AssetType.Stock, CurrentPrice = 28.90m }
            };
            context.Assets.AddRange(sampleAssets);
            await context.SaveChangesAsync();

            // Adicionar trades de exemplo
            var petr4 = sampleAssets[0];
            var vale3 = sampleAssets[1];
            var sampleTrades = new[]
            {
                new Trade
                {
                    AssetId = petr4.Id,
                    Type = TradeType.Buy,
                    Quantity = 100,
                    Price = 30.00m,
                    Date = DateTime.Now.AddDays(-10)
                },
                new Trade
                {
                    AssetId = vale3.Id,
                    Type = TradeType.Buy,
                    Quantity = 50,
                    Price = 60.00m,
                    Date = DateTime.Now.AddDays(-5)
                }
            };
            context.Trades.AddRange(sampleTrades);
            await context.SaveChangesAsync();
        }
    }
}
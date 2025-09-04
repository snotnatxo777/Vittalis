using Vittalis.Models;

namespace Vittalis.Helpers
{
    public static class AccountHelper
    {
        public static Dictionary<AccountType, string> GetAccountTypeNames() => new()
        {
            { AccountType.CheckingAccount, "Conta Corrente" },
            { AccountType.SavingsAccount, "Poupança" },
            { AccountType.CreditCard, "Cartão de Crédito" },
            { AccountType.Cash, "Dinheiro" },
            { AccountType.Investment, "Investimentos" },
            { AccountType.Other, "Outras" }
        };

        public static Dictionary<AccountType, string> GetAccountTypeIcons() => new()
        {
            { AccountType.CheckingAccount, "🏦" },
            { AccountType.SavingsAccount, "🐷" },
            { AccountType.CreditCard, "💳" },
            { AccountType.Cash, "💰" },
            { AccountType.Investment, "📈" },
            { AccountType.Other, "📄" }
        };

        public static Dictionary<AccountType, Color> GetAccountTypeColors() => new()
        {
            { AccountType.CheckingAccount, Colors.Blue },
            { AccountType.SavingsAccount, Colors.Green },
            { AccountType.CreditCard, Colors.Orange },
            { AccountType.Cash, Colors.Gold },
            { AccountType.Investment, Colors.Purple },
            { AccountType.Other, Colors.Gray }
        };

        public static string GetAccountTypeName(AccountType type)
        {
            return GetAccountTypeNames().GetValueOrDefault(type, type.ToString());
        }

        public static string GetAccountTypeIcon(AccountType type)
        {
            return GetAccountTypeIcons().GetValueOrDefault(type, "📄");
        }

        public static Color GetAccountTypeColor(AccountType type)
        {
            return GetAccountTypeColors().GetValueOrDefault(type, Colors.Gray);
        }
    }
}
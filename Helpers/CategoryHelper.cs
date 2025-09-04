using Vittalis.Models;

namespace Vittalis.Helpers
{
    public static class CategoryHelper
    {
        public static Dictionary<TransactionCategory, string> GetCategoryNames() => new()
        {
            // Receitas
            { TransactionCategory.Salary, "Salário" },
            { TransactionCategory.Freelance, "Freelance" },
            { TransactionCategory.Investment, "Investimentos" },
            { TransactionCategory.Gift, "Presente/Doação" },
            { TransactionCategory.OtherIncome, "Outras Receitas" },
            
            // Despesas
            { TransactionCategory.Food, "Alimentação" },
            { TransactionCategory.Transportation, "Transporte" },
            { TransactionCategory.Housing, "Moradia" },
            { TransactionCategory.Health, "Saúde" },
            { TransactionCategory.Education, "Educação" },
            { TransactionCategory.Entertainment, "Lazer" },
            { TransactionCategory.Shopping, "Compras" },
            { TransactionCategory.Bills, "Contas" },
            { TransactionCategory.Travel, "Viagem" },
            { TransactionCategory.OtherExpense, "Outras Despesas" }
        };

        public static Dictionary<TransactionCategory, string> GetCategoryIcons() => new()
        {
            // Receitas
            { TransactionCategory.Salary, "💰" },
            { TransactionCategory.Freelance, "💻" },
            { TransactionCategory.Investment, "📈" },
            { TransactionCategory.Gift, "🎁" },
            { TransactionCategory.OtherIncome, "💵" },
            
            // Despesas
            { TransactionCategory.Food, "🍽️" },
            { TransactionCategory.Transportation, "🚗" },
            { TransactionCategory.Housing, "🏠" },
            { TransactionCategory.Health, "⚕️" },
            { TransactionCategory.Education, "📚" },
            { TransactionCategory.Entertainment, "🎭" },
            { TransactionCategory.Shopping, "🛒" },
            { TransactionCategory.Bills, "📄" },
            { TransactionCategory.Travel, "✈️" },
            { TransactionCategory.OtherExpense, "📝" }
        };

        public static string GetCategoryName(TransactionCategory category)
        {
            return GetCategoryNames().GetValueOrDefault(category, category.ToString());
        }

        public static string GetCategoryIcon(TransactionCategory category)
        {
            return GetCategoryIcons().GetValueOrDefault(category, "•");
        }

        public static List<TransactionCategory> GetCategoriesByType(TransactionType type)
        {
            return type switch
            {
                TransactionType.Income => new List<TransactionCategory>
                {
                    TransactionCategory.Salary,
                    TransactionCategory.Freelance,
                    TransactionCategory.Investment,
                    TransactionCategory.Gift,
                    TransactionCategory.OtherIncome
                },
                TransactionType.Expense => new List<TransactionCategory>
                {
                    TransactionCategory.Food,
                    TransactionCategory.Transportation,
                    TransactionCategory.Housing,
                    TransactionCategory.Health,
                    TransactionCategory.Education,
                    TransactionCategory.Entertainment,
                    TransactionCategory.Shopping,
                    TransactionCategory.Bills,
                    TransactionCategory.Travel,
                    TransactionCategory.OtherExpense
                },
                _ => new List<TransactionCategory>()
            };
        }
    }
}
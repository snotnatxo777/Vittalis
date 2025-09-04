using System.Globalization;
using Vittalis.Models;

namespace Vittalis.Helpers
{
    public class CategoryToNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TransactionCategory category)
            {
                return CategoryHelper.GetCategoryName(category);
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
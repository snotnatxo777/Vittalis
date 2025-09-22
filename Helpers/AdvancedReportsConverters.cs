using System.Globalization;
using Vittalis.Models;

namespace Vittalis.Helpers
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue && parameter is string targetValue)
            {
                return stringValue.Equals(targetValue, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IntToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue > 0;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CashFlowProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal cashFlow)
            {
                // Normalizar o valor para um progresso entre 0 e 1
                // Assumindo um range de -5000 a +5000 como exemplo
                var normalizedValue = (double)((cashFlow + 5000) / 10000);
                return Math.Max(0, Math.Min(1, normalizedValue));
            }
            return 0.5; // Valor neutro
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class HealthScoreToProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is FinancialHealthScore score)
            {
                return (int)score / 5.0; // Converte para progresso de 0 a 1
            }
            return 0.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class TrendDirectionToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TrendDirection direction)
            {
                return direction switch
                {
                    TrendDirection.Up => "📈",
                    TrendDirection.Down => "📉",
                    _ => "➡️"
                };
            }
            return "➡️";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AlertSeverityToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is AlertSeverity severity)
            {
                return severity switch
                {
                    AlertSeverity.Critical => Colors.Red,
                    AlertSeverity.Warning => Colors.Orange,
                    AlertSeverity.Info => Colors.Blue,
                    _ => Colors.Gray
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PercentageToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double percentage)
            {
                return percentage switch
                {
                    >= 80 => Colors.Red,      // Vermelho para valores altos (ruim)
                    >= 60 => Colors.Orange,   // Laranja para valores médios
                    >= 40 => Colors.Yellow,   // Amarelo para valores baixos
                    _ => Colors.Green         // Verde para valores muito baixos (bom)
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MoneyRangeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal amount)
            {
                return amount switch
                {
                    > 1000 => Colors.Green,
                    > 500 => Colors.Orange,
                    > 0 => Colors.Yellow,
                    _ => Colors.Red
                };
            }
            return Colors.Gray;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1.0 : 0.3;
            }
            return 1.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DateToRelativeStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                var timeSpan = DateTime.Now - date;

                return timeSpan.TotalDays switch
                {
                    < 1 => "Hoje",
                    < 2 => "Ontem",
                    < 7 => $"{(int)timeSpan.TotalDays} dias atrás",
                    < 30 => $"{(int)(timeSpan.TotalDays / 7)} semanas atrás",
                    < 365 => $"{(int)(timeSpan.TotalDays / 30)} meses atrás",
                    _ => $"{(int)(timeSpan.TotalDays / 365)} anos atrás"
                };
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
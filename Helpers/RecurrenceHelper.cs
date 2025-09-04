using Vittalis.Models;

namespace Vittalis.Helpers
{
    public static class RecurrenceHelper
    {
        public static Dictionary<RecurrenceFrequency, string> GetFrequencyNames() => new()
        {
            { RecurrenceFrequency.Weekly, "Semanal" },
            { RecurrenceFrequency.Monthly, "Mensal" },
            { RecurrenceFrequency.Quarterly, "Trimestral" },
            { RecurrenceFrequency.Yearly, "Anual" }
        };

        public static string GetFrequencyName(RecurrenceFrequency frequency)
        {
            return GetFrequencyNames().GetValueOrDefault(frequency, frequency.ToString());
        }

        public static string GetFrequencyDescription(RecurrenceFrequency frequency)
        {
            return frequency switch
            {
                RecurrenceFrequency.Weekly => "A cada 7 dias",
                RecurrenceFrequency.Monthly => "Todo mês",
                RecurrenceFrequency.Quarterly => "A cada 3 meses",
                RecurrenceFrequency.Yearly => "Todo ano",
                _ => "Personalizado"
            };
        }
    }
}
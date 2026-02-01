using WPFBudgetPlanerare.Models;

namespace WPFBudgetPlanerare.Services
{
    public class ForecastCalculationService
    {
        public decimal CalculateMonthlyForecast(
            IEnumerable<Transaction> transactions,
            decimal monthlyIncomeFromAnnual,
            int month = 0) // 0 = nuvarande månad
        {
                if (month == 0)
                month = DateTime.Now.Month;

            decimal total = monthlyIncomeFromAnnual;

            foreach (var t in transactions)
            {
                // månadsutgifter, läggs till varje månad
                if (t.Recurrence == RecurrenceType.Monthly)
                {
                    total += t.Amount * (t.Type == TransactionType.Expense ? -1 : 1);
                }
                // årsutgifter, läggs bara till den valda månaden
                else if (t.Recurrence == RecurrenceType.Yearly)
                {
                    if (t.MonthOfYear == month)
                    {
                        total += t.Amount * (t.Type == TransactionType.Expense ? -1 : 1);
                    }
                }

                else if (t.Recurrence == RecurrenceType.None)
                {
                    total += t.Amount * (t.Type == TransactionType.Expense ? -1 : 1);
                }
            }

            return total;
        }

        public decimal CalculateAverageMonthlyForecast(
            IEnumerable<Transaction> transactions,
            decimal monthlyIncomeFromAnnual)
        {
            decimal total = monthlyIncomeFromAnnual;

            foreach (var t in transactions)
            {
                if (t.Recurrence == RecurrenceType.Monthly)
                {
                    total += t.Amount * (t.Type == TransactionType.Expense ? -1 : 1);
                }

                // Årliga utgifter sprids ut på alla 12 månader
                else if (t.Recurrence == RecurrenceType.Yearly)
                {
                    total += (t.Amount / 12) * (t.Type == TransactionType.Expense ? -1 : 1);
                }

                else if (t.Recurrence == RecurrenceType.None)
                {
                    total += t.Amount * (t.Type == TransactionType.Expense ? -1 : 1);
                }
            }

            return total;
        }
    }
}

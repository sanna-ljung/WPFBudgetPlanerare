using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WPFBudgetPlanerare.Models;

namespace WPFBudgetPlanerare.Services
{
    public class ForecastCalculationService
    {
        public decimal CalculateMonthlyForecast(
            IEnumerable<Transaction> transactions)
        {
            decimal total = 0;
            foreach (var t in transactions)
            {
                if (t.Recurrence == RecurrenceType.Monthly)
                {
                    total += t.Amount * (t.Type == TransactionType.Expense ? -1 : 1);
                }
                if (t.Recurrence == RecurrenceType.Yearly)
                {
                    total += (t.Amount / 12) * (t.Type == TransactionType.Expense ? -1 : 1);
                }

                if (t.Recurrence == RecurrenceType.None)
                {
                    total += t.Amount * (t.Type == TransactionType.Expense ? -1 : 1);
                }
            }
            return total;
        }
    }
}

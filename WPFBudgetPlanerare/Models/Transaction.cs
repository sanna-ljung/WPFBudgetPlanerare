using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WPFBudgetPlanerare.Models
{
    public enum TransactionType
    {
        Income,
        Expense
    }
    public enum RecurrenceType
    {
        None,
        Monthly,
        Yearly

    }
    public class Transaction
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime Date { get; set; }
        public TransactionType Type { get; set; } //inkomst/utgift
        public RecurrenceType Recurrence { get; set; } //månatlig/årlig/ingen
        public int? Month { get; set; } //för årlig återkommande
        public Category Category { get; set; }
    }
}

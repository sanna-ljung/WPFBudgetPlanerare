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
        public TransactionType Type { get; set; } //inkomst/utgift
        public Category Category { get; set; }
        public RecurrenceType Recurrence { get; set; } //månatlig/årlig/ingen
        public int? MonthOfYear { get; set; } //årlig återkommande, 1-12 el null
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

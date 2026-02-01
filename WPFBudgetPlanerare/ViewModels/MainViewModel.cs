using System.Collections.ObjectModel;
using WPFBudgetPlanerare.Data;
using WPFBudgetPlanerare.Models;
using WPFBudgetPlanerare.Services;
using WPFBudgetPlanerare.Command;

namespace WPFBudgetPlanerare.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ForecastCalculationService _forecastService;

        //samlingar till UI
        //lista som DataGrid binder till
        public ObservableCollection<Transaction> Transactions { get; }

        //listor till ComboBoxar
        public IEnumerable<TransactionType> TransactionTypes =>
            Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();

        public IEnumerable<Category> Categories =>
            Enum.GetValues(typeof(Category)).Cast<Category>();

        public IEnumerable<RecurrenceType> RecurrenceTypes =>
            Enum.GetValues(typeof(RecurrenceType)).Cast<RecurrenceType>();

        public IEnumerable<int> Months => Enumerable.Range(1, 12);

        public Dictionary<int, string> MonthNames => new()
        {
            { 1, "Januari" },
            {2, "Februari" },
            {3, "Mars" },
            {4, "April" },
            {5, "Maj" },
            {6, "Juni" },
            {7, "Juli" },
            {8, "Augusti" },
            {9, "September" },
            {10, "Oktober" },
            {11, "November" },
            {12, "December" }

        };

        // inmatning
        
        private string _newTransactionName;
        public string NewTransactionName
        {
            get => _newTransactionName;
            set
            {
                _newTransactionName = value;
                OnPropertyChanged();
            }
        }

        private decimal _newTransactionAmount;
        public decimal NewTransactionAmount
        {
            get => _newTransactionAmount;
            set
            {
                _newTransactionAmount = value;
                OnPropertyChanged();
            }
        }

        private TransactionType _selectedTransactionType;
        public TransactionType SelectedTransactionType
        {
            get => _selectedTransactionType;
            set
            {
                _selectedTransactionType = value;
                OnPropertyChanged();
            }
        }

        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
            }
        }

        private int _selectedMonth = DateTime.Now.Month;
        public int SelectedMonth
        {
            get => _selectedMonth;
            set
            {
                _selectedMonth = value;
                OnPropertyChanged();
            }
        }

        private RecurrenceType _selectedRecurrenceType;
        public RecurrenceType SelectedRecurrenceType
        {
            get => _selectedRecurrenceType;
            set
            {
                _selectedRecurrenceType = value;
                OnPropertyChanged();
            }
        }

        // selected (DataGrid)
        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                _selectedTransaction = value;
                OnPropertyChanged();
                RemoveTransactionCommand.RaiseCanExecuteChanged();
            }
        }

        //årsinkomst/arbetstid
        private decimal _annualIncome;
        public decimal AnnualIncome
        {
            get => _annualIncome;
            set
            {
                _annualIncome = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HourlyRate));
                OnPropertyChanged(nameof(MonthlyIncomeFromAnnual));
                UpdateTotals();
            }
        }

        private int _annualWorkHours;
        public int AnnualWorkHours
        {
            get => _annualWorkHours;
            set
            {
                _annualWorkHours = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HourlyRate));
                OnPropertyChanged(nameof(MonthlyIncomeFromAnnual));
                UpdateTotals();
            }
        }

        public decimal HourlyRate =>
            AnnualWorkHours > 0 ? AnnualIncome / AnnualWorkHours : 0;

        public decimal MonthlyIncomeFromAnnual =>
            AnnualIncome / 12;

        //summering, prognos
        private decimal _totalIncome;
        public decimal TotalIncome
        {
            get => _totalIncome;
            set
            {
                _totalIncome = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MonthlyForecast));
            }
        }

        private decimal _totalExpense;
        public decimal TotalExpense
        {
            get => _totalExpense;
            set
            {
                _totalExpense = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MonthlyForecast));
            }
        }

        // Prognos för denna månad
        public decimal MonthlyForecast =>
            _forecastService.CalculateMonthlyForecast(Transactions, MonthlyIncomeFromAnnual, DateTime.Now.Month);

        // Prognos för nästa månad
        public decimal NextMonthForecast
        {
            get
            {
                int nextMonth = DateTime.Now.AddMonths(1).Month;
                return _forecastService.CalculateMonthlyForecast(Transactions, MonthlyIncomeFromAnnual, nextMonth);
            }
        }

        // Genomsnittlig månadsprognos
        public decimal AverageMonthlyForecast =>
            _forecastService.CalculateAverageMonthlyForecast(Transactions, MonthlyIncomeFromAnnual);


        // commands
        public RelayCommand AddTransactionCommand { get; }
        public RelayCommand RemoveTransactionCommand { get; }

        //konstruktor
        public MainViewModel()
        {
            _dbContext = new ApplicationDbContext();
            _forecastService = new ForecastCalculationService();

            Transactions = new ObservableCollection<Transaction>(_dbContext.Transactions.ToList());

            //initiera commands
            AddTransactionCommand = new RelayCommand(_ => AddTransaction());
            RemoveTransactionCommand = new RelayCommand(_ => RemoveTransaction(), _ => SelectedTransaction != null);

            //beräkna summeringar vid start
            UpdateTotals();
        }

        //metoder
        private void AddTransaction()
        {
            if (string.IsNullOrWhiteSpace(NewTransactionName))
                return;

            if (NewTransactionAmount <= 0)
                return;

            var transaction = new Transaction
            {
                Name = NewTransactionName,
                Amount = NewTransactionAmount,
                Type = SelectedTransactionType,
                Category = SelectedCategory,
                Recurrence = SelectedRecurrenceType,
                MonthOfYear = SelectedRecurrenceType == RecurrenceType.Yearly
                    ? SelectedMonth
                    : null
            };

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            Transactions.Add(transaction);
            UpdateTotals();

            NewTransactionName = string.Empty;
            NewTransactionAmount = 0;
            SelectedMonth = DateTime.Now.Month;
        }

        private void RemoveTransaction()
        {
            if (SelectedTransaction == null) return;

            _dbContext.Transactions.Remove(SelectedTransaction);
            _dbContext.SaveChanges();

            Transactions.Remove(SelectedTransaction);
            SelectedTransaction = null;

            UpdateTotals();
        }

        private void UpdateTotals()
        {
            decimal transactionIncome = Transactions
                .Where(t => t.Type == TransactionType.Income)
                .Sum(t => t.Amount);

            //lägg till månadsinkomst från årsinkomst
            TotalIncome = transactionIncome + MonthlyIncomeFromAnnual;

            //beräkna total utgift, årskostnad delat på 12 m månadshänsyn
            TotalExpense = Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t =>
                {
                    if (t.Recurrence == RecurrenceType.Monthly)
                        return t.Amount;

                    if (t.Recurrence == RecurrenceType.Yearly)
                        return t.Amount / 12;

                    return t.Amount;
                });

            //uppdaterar prognoser när data ändras
            OnPropertyChanged(nameof(MonthlyForecast));
            OnPropertyChanged(nameof(NextMonthForecast));
            OnPropertyChanged(nameof(AverageMonthlyForecast));
        }
    }
}

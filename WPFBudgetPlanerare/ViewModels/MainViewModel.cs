using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WPFBudgetPlanerare.Data;
using WPFBudgetPlanerare.Models;
using WPFBudgetPlanerare.Services;
using WPFBudgetPlanerare.Command;

namespace WPFBudgetPlanerare.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly ApplicationDbContext _dbContext;

        //samlingar till UI
        //lista som DataGrid binder till
        public ObservableCollection<Transaction> Transactions { get; }

        //listor till ComboBoxar(enum)
        public IEnumerable<TransactionType> TransactionTypes =>
            Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>();

        public IEnumerable<Category> Categories =>
            Enum.GetValues(typeof(Category)).Cast<Category>();

        public IEnumerable<RecurrenceType> RecurrenceTypes =>
            Enum.GetValues(typeof(RecurrenceType)).Cast<RecurrenceType>();

        // inmatning
        public string NewTransactionName { get; set; }
        public decimal NewTransactionAmount { get; set; }
        public TransactionType SelectedTransactionType { get; set; }
        public Category SelectedCategory { get; set; }
        public RecurrenceType SelectedRecurrenceType { get; set; }

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

        public decimal MonthlyForecast => TotalIncome - TotalExpense;

        // commands
        public RelayCommand AddTransactionCommand { get; }
        public RelayCommand RemoveTransactionCommand { get; }

        //konstruktor
        public MainViewModel()
        {
            _dbContext = new ApplicationDbContext();

            //ladda data från databasen
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
                Recurrence = SelectedRecurrenceType
            };

            _dbContext.Transactions.Add(transaction);
            _dbContext.SaveChanges();

            Transactions.Add(transaction);
            UpdateTotals();
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

            //beräkna total utgift, årskostnad delat på 12
            TotalExpense = Transactions
                .Where(t => t.Type == TransactionType.Expense)
                .Sum(t =>
                    t.Recurrence == RecurrenceType.Yearly
                        ? t.Amount / 12
                        : t.Amount
                );
        }
    }
}

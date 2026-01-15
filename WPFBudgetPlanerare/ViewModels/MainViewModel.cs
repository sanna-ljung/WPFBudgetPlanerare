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
    public class MainViewModel:BaseViewModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ForecastCalculationService _forecast;

        public ObservableCollection<Transaction> Transactions { get; set; }

        public decimal MonthlyForecast =>
            _forecast.CalculateMonthlyForecast(Transactions);

        public ICommand AddTransactionCommand { get; }
        public ICommand RemoveTransactionCommand { get; }

        public MainViewModel() 
        {
            _context = new ApplicationDbContext();
            _forecast = new ForecastCalculationService();

            Transactions = new ObservableCollection<Transaction>(
                _context.Transactions.ToList());
            
            AddTransactionCommand = new RelayCommand(AddTransaction);
            RemoveTransactionCommand = new RelayCommand(RemoveTransaction);
        }

        private void AddTransaction(object? parameter)
        {
            var t = new Transaction 
            { Amount = 0, 
                Name = "New Transaction", 
                Date = DateTime.Now, Type = 
                TransactionType.Expense, 
                Recurrence = RecurrenceType.None };
            Transactions.Add(t);
            _context.Transactions.Add(t);
            _context.SaveChanges();
            OnPropertyChanged(nameof(MonthlyForecast));
        }
        private void RemoveTransaction(object? parameter) 
        {
            if (parameter is Transaction t) 
            {
                Transactions.Remove(t);
                _context.Transactions.Remove(t);
                _context.SaveChanges();
                OnPropertyChanged(nameof(MonthlyForecast));
            }
        }

        private Transaction _selectedTransaction;
        public Transaction SelectedTransaction
        {
            get => _selectedTransaction;
            set
            {
                _selectedTransaction = value;
                OnPropertyChanged();
            }
        }
    }
}

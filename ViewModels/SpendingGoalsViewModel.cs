using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Vittalis.Models;
using Vittalis.Services;
using Vittalis.Helpers;

namespace Vittalis.ViewModels
{
    public class SpendingGoalsViewModel : INotifyPropertyChanged
    {
        private readonly ISpendingGoalService _spendingGoalService;
        private bool _isLoading;
        private DateTime _selectedDate = DateTime.Now;
        private string _selectedPeriodText = string.Empty;
        private decimal _totalBudget;
        private decimal _totalSpent;
        private int _goalsOverBudget;

        public ObservableCollection<SpendingGoal> SpendingGoals { get; set; }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); UpdateSelectedPeriodText(); }
        }

        public string SelectedPeriodText
        {
            get => _selectedPeriodText;
            set { _selectedPeriodText = value; OnPropertyChanged(); }
        }

        public decimal TotalBudget
        {
            get => _totalBudget;
            set { _totalBudget = value; OnPropertyChanged(); }
        }

        public decimal TotalSpent
        {
            get => _totalSpent;
            set { _totalSpent = value; OnPropertyChanged(); }
        }

        public decimal RemainingBudget => TotalBudget - TotalSpent;

        public double OverallProgress => TotalBudget > 0 ? (double)(TotalSpent / TotalBudget * 100) : 0;

        public int GoalsOverBudget
        {
            get => _goalsOverBudget;
            set { _goalsOverBudget = value; OnPropertyChanged(); }
        }

        public SpendingGoalsViewModel(ISpendingGoalService spendingGoalService)
        {
            _spendingGoalService = spendingGoalService;
            SpendingGoals = new ObservableCollection<SpendingGoal>();
            UpdateSelectedPeriodText();
            _ = LoadDataAsync();
        }

        private void UpdateSelectedPeriodText()
        {
            SelectedPeriodText = $"{SelectedDate:MMMM yyyy}";
        }

        private async Task LoadDataAsync()
        {
            try
            {
                IsLoading = true;

                var spendingGoals = await _spendingGoalService.GetSpendingGoalsWithProgressAsync(
                    SelectedDate.Year, SelectedDate.Month);

                SpendingGoals.Clear();
                foreach (var goal in spendingGoals)
                {
                    SpendingGoals.Add(goal);
                }

                // Calcular totais
                TotalBudget = SpendingGoals.Sum(g => g.MonthlyLimit);
                TotalSpent = SpendingGoals.Sum(g => g.CurrentSpent);
                GoalsOverBudget = SpendingGoals.Count(g => g.IsOverBudget);

                // Notificar propriedades calculadas
                OnPropertyChanged(nameof(RemainingBudget));
                OnPropertyChanged(nameof(OverallProgress));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao carregar metas: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public async Task ChangePeriodAsync(DateTime newDate)
        {
            SelectedDate = newDate;
            await LoadDataAsync();
        }

        public async Task AddSpendingGoalAsync(SpendingGoal spendingGoal)
        {
            try
            {
                spendingGoal.Year = SelectedDate.Year;
                spendingGoal.Month = SelectedDate.Month;
                await _spendingGoalService.AddSpendingGoalAsync(spendingGoal);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao adicionar meta: {ex.Message}");
            }
        }

        public async Task DeleteSpendingGoalAsync(int goalId)
        {
            try
            {
                await _spendingGoalService.DeleteSpendingGoalAsync(goalId);
                await RefreshAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao excluir meta: {ex.Message}");
            }
        }

        public async Task RefreshAsync()
        {
            await LoadDataAsync();
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
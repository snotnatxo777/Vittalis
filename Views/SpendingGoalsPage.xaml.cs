using Vittalis.Models;
using Vittalis.Services;
using Vittalis.ViewModels;
using Vittalis.Helpers;

namespace Vittalis.Views;

public partial class SpendingGoalsPage : ContentPage
{
    private readonly SpendingGoalsViewModel _viewModel;

    public SpendingGoalsPage(SpendingGoalsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    private async void OnPreviousMonthClicked(object sender, EventArgs e)
    {
        var previousMonth = _viewModel.SelectedDate.AddMonths(-1);
        await _viewModel.ChangePeriodAsync(previousMonth);
    }

    private async void OnNextMonthClicked(object sender, EventArgs e)
    {
        var nextMonth = _viewModel.SelectedDate.AddMonths(1);

        // Permitir navegar até 12 meses no futuro
        if (nextMonth <= DateTime.Now.AddMonths(12))
        {
            await _viewModel.ChangePeriodAsync(nextMonth);
        }
    }

    private async void OnAddGoalClicked(object sender, EventArgs e)
    {
        var serviceProvider = Handler?.MauiContext?.Services;
        var addPage = serviceProvider?.GetService<AddSpendingGoalPage>();

        if (addPage != null)
        {
            // Passar a data selecionada para a página
            addPage.SetSelectedDate(_viewModel.SelectedDate);
            await Navigation.PushAsync(addPage);
        }
        else
        {
            await DisplayAlert("Erro", "Erro ao carregar página de nova meta", "OK");
        }
    }

    private async void OnMenuClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is SpendingGoal spendingGoal)
        {
            var action = await DisplayActionSheet(
                $"Meta: {CategoryHelper.GetCategoryName(spendingGoal.Category)}",
                "Cancelar",
                null,
                "Editar Limite",
                "Excluir Meta");

            switch (action)
            {
                case "Editar Limite":
                    await EditSpendingGoal(spendingGoal);
                    break;
                case "Excluir Meta":
                    await DeleteSpendingGoal(spendingGoal);
                    break;
            }
        }
    }

    private async Task EditSpendingGoal(SpendingGoal spendingGoal)
    {
        var currentLimit = spendingGoal.MonthlyLimit.ToString("F2");
        var newLimitText = await DisplayPromptAsync(
            "Editar Meta",
            $"Novo limite para {CategoryHelper.GetCategoryName(spendingGoal.Category)}:",
            "OK",
            "Cancelar",
            placeholder: currentLimit,
            keyboard: Keyboard.Numeric);

        if (!string.IsNullOrWhiteSpace(newLimitText) &&
            decimal.TryParse(newLimitText, out decimal newLimit) &&
            newLimit > 0)
        {
            spendingGoal.MonthlyLimit = newLimit;

            var serviceProvider = Handler?.MauiContext?.Services;
            var spendingGoalService = serviceProvider?.GetService<ISpendingGoalService>();

            if (spendingGoalService != null)
            {
                await spendingGoalService.UpdateSpendingGoalAsync(spendingGoal);
                await _viewModel.RefreshAsync();
                await DisplayAlert("Sucesso", "Meta atualizada com sucesso!", "OK");
            }
        }
    }

    private async Task DeleteSpendingGoal(SpendingGoal spendingGoal)
    {
        var categoryName = CategoryHelper.GetCategoryName(spendingGoal.Category);
        var result = await DisplayAlert(
            "Confirmar Exclusão",
            $"Deseja realmente excluir a meta de {spendingGoal.MonthlyLimit:C} para '{categoryName}'?",
            "Sim",
            "Não");

        if (result)
        {
            await _viewModel.DeleteSpendingGoalAsync(spendingGoal.Id);
            await DisplayAlert("Sucesso", "Meta excluída com sucesso!", "OK");
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.RefreshAsync();
    }
}
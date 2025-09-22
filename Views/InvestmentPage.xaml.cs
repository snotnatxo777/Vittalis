using Microsoft.Extensions.DependencyInjection;
using Vittalis.Services;
using Vittalis.ViewModels;
using Vittalis.Models;
using System.Globalization;

namespace Vittalis.Views;

public partial class InvestmentPage : ContentPage
{
    private readonly InvestmentViewModel _viewModel;
    private string _currentTab = "Portfolio";

    public InvestmentPage(InvestmentViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            // Atualizar dados quando a página aparecer
            await _viewModel.RefreshAllDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao carregar dados: {ex.Message}", "OK");
        }
    }

    private async void OnAddTradeClicked(object sender, EventArgs e)
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            if (serviceProvider != null)
            {
                var investmentService = serviceProvider.GetService<IInvestmentService>();
                if (investmentService != null)
                {
                    // Criar e navegar para AddTradePage
                    var addTradePage = new AddTradePage(investmentService);
                    await Navigation.PushAsync(addTradePage);
                }
                else
                {
                    await DisplayAlert("Erro", "Serviço de investimentos não disponível", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao abrir página de trade: {ex.Message}", "OK");
        }
    }

    private async void OnUpdatePricesClicked(object sender, EventArgs e)
    {
        try
        {
            await _viewModel.UpdatePricesAsync();
            await DisplayAlert("Sucesso", "Preços atualizados com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao atualizar preços: {ex.Message}", "OK");
        }
    }

    private async void OnTabChanged(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is string tabName)
        {
            _currentTab = tabName;

            // Esconder todas as abas
            PortfolioContent.IsVisible = false;
            AssetsContent.IsVisible = false;
            TradesContent.IsVisible = false;
            DividendsContent.IsVisible = false;
            GoalsContent.IsVisible = false;

            // Mostrar a aba selecionada
            switch (tabName)
            {
                case "Portfolio":
                    PortfolioContent.IsVisible = true;
                    break;
                case "Assets":
                    AssetsContent.IsVisible = true;
                    break;
                case "Trades":
                    TradesContent.IsVisible = true;
                    break;
                case "Dividends":
                    DividendsContent.IsVisible = true;
                    break;
                case "Goals":
                    GoalsContent.IsVisible = true;
                    break;
            }

            // Atualizar o ViewModel se necessário
            if (_viewModel.SelectedTab != tabName)
            {
                _viewModel.SelectedTab = tabName;
            }
        }
    }

    private async void OnAddAssetClicked(object sender, EventArgs e)
    {
        try
        {
            var symbol = await DisplayPromptAsync("Novo Ativo", "Digite o símbolo do ativo (ex: PETR4):");
            if (string.IsNullOrWhiteSpace(symbol)) return;

            var name = await DisplayPromptAsync("Novo Ativo", "Nome do ativo:");
            if (string.IsNullOrWhiteSpace(name)) return;

            var typeOptions = new[] { "Ação", "FII", "ETF", "Cripto", "Renda Fixa" };
            var selectedType = await DisplayActionSheet("Tipo de Ativo", "Cancelar", null, typeOptions);
            if (selectedType == "Cancelar") return;

            var assetType = selectedType switch
            {
                "Ação" => AssetType.Stock,
                "FII" => AssetType.RealEstate,
                "ETF" => AssetType.ETF,
                "Cripto" => AssetType.Crypto,
                "Renda Fixa" => AssetType.Bond,
                _ => AssetType.Stock
            };

            var priceStr = await DisplayPromptAsync("Novo Ativo", "Preço atual:", keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(priceStr, out decimal price) || price <= 0)
            {
                await DisplayAlert("Erro", "Preço inválido", "OK");
                return;
            }

            var asset = new Asset
            {
                Symbol = symbol.ToUpper().Trim(),
                Name = name.Trim(),
                Type = assetType,
                CurrentPrice = price,
                PreviousPrice = price,
                Status = AssetStatus.Active,
                Exchange = "B3"
            };

            await _viewModel.AddAssetAsync(asset);
            await DisplayAlert("Sucesso", "Ativo adicionado com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao adicionar ativo: {ex.Message}", "OK");
        }
    }

    private async void OnAddGoalClicked(object sender, EventArgs e)
    {
        try
        {
            var goalName = await DisplayPromptAsync("Nova Meta", "Nome da meta:");
            if (string.IsNullOrWhiteSpace(goalName)) return;

            var targetAmountStr = await DisplayPromptAsync("Nova Meta", "Valor alvo:", keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(targetAmountStr, out decimal targetAmount) || targetAmount <= 0)
            {
                await DisplayAlert("Erro", "Valor inválido", "OK");
                return;
            }

            var monthsStr = await DisplayPromptAsync("Nova Meta", "Prazo em meses:", keyboard: Keyboard.Numeric);
            if (!int.TryParse(monthsStr, out int months) || months <= 0)
            {
                await DisplayAlert("Erro", "Prazo inválido", "OK");
                return;
            }

            var typeOptions = new[] { "Aposentadoria", "Reserva de Emergência", "Casa Própria", "Carro", "Viagem", "Educação", "Outros" };
            var selectedTypeStr = await DisplayActionSheet("Tipo de Meta", "Cancelar", null, typeOptions);
            if (selectedTypeStr == "Cancelar") return;

            var goalType = selectedTypeStr switch
            {
                "Aposentadoria" => GoalType.Retirement,
                "Reserva de Emergência" => GoalType.Emergency,
                "Casa Própria" => GoalType.House,
                "Carro" => GoalType.Car,
                "Viagem" => GoalType.Travel,
                "Educação" => GoalType.Education,
                _ => GoalType.Other
            };

            var goal = new InvestmentGoal
            {
                Name = goalName.Trim(),
                TargetAmount = targetAmount,
                TargetDate = DateTime.Now.AddMonths(months),
                CurrentAmount = 0,
                MonthlyContribution = targetAmount / months,
                Type = goalType,
                IsActive = true,
                Description = $"Meta de {goalName} com prazo de {months} meses"
            };

            await _viewModel.AddInvestmentGoalAsync(goal);
            await DisplayAlert("Sucesso", "Meta adicionada com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao adicionar meta: {ex.Message}", "OK");
        }
    }

    private async void OnAddDividendClicked(object sender, EventArgs e)
    {
        try
        {
            // Buscar ativos disponíveis
            var serviceProvider = Handler?.MauiContext?.Services;
            var investmentService = serviceProvider?.GetService<IInvestmentService>();
            if (investmentService == null)
            {
                await DisplayAlert("Erro", "Serviço não disponível", "OK");
                return;
            }

            var allAssets = await investmentService.GetAllAssetsAsync();

            if (!allAssets.Any())
            {
                await DisplayAlert("Aviso", "Nenhum ativo encontrado. Adicione ativos primeiro.", "OK");
                return;
            }

            var assetNames = allAssets.Select(a => $"{a.Symbol} - {a.Name}").ToArray();
            var selectedAsset = await DisplayActionSheet("Selecione o Ativo", "Cancelar", null, assetNames);

            if (selectedAsset == "Cancelar" || string.IsNullOrEmpty(selectedAsset)) return;

            var selectedIndex = Array.IndexOf(assetNames, selectedAsset);
            var asset = allAssets[selectedIndex];

            var amountStr = await DisplayPromptAsync("Dividendo", "Valor por ação:", keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(amountStr, out decimal amount) || amount <= 0)
            {
                await DisplayAlert("Erro", "Valor inválido", "OK");
                return;
            }

            var typeOptions = new[] { "Dividendo", "JCP", "Bonificação" };
            var selectedTypeStr = await DisplayActionSheet("Tipo", "Cancelar", null, typeOptions);
            if (selectedTypeStr == "Cancelar") return;

            var dividendType = selectedTypeStr switch
            {
                "Dividendo" => DividendType.Dividend,
                "JCP" => DividendType.JCP,
                "Bonificação" => DividendType.Bonus,
                _ => DividendType.Dividend
            };

            var dividend = new Dividend
            {
                AssetId = asset.Id,
                AmountPerShare = amount,
                PaymentDate = DateTime.Now,
                ExDividendDate = DateTime.Now.AddDays(-30),
                DeclarationDate = DateTime.Now.AddDays(-45),
                Type = dividendType,
                Description = $"{selectedTypeStr} {asset.Symbol}"
            };

            await _viewModel.AddDividendAsync(dividend);
            await DisplayAlert("Sucesso", "Dividendo registrado com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao registrar dividendo: {ex.Message}", "OK");
        }
    }

    private async void OnCheckAlertsClicked(object sender, EventArgs e)
    {
        try
        {
            await _viewModel.CheckAlertsAsync();

            var serviceProvider = Handler?.MauiContext?.Services;
            var investmentService = serviceProvider?.GetService<IInvestmentService>();
            if (investmentService == null) return;

            var alerts = await investmentService.GetPriceAlertsAsync();
            var triggeredAlerts = alerts.Where(a => a.IsTriggered).ToList();

            if (triggeredAlerts.Any())
            {
                var message = string.Join("\n", triggeredAlerts.Select(a =>
                    $"🔔 {a.Asset?.Symbol}: {a.Condition} R$ {a.TargetPrice:F2}"));
                await DisplayAlert("Alertas Disparados", message, "OK");
            }
            else
            {
                await DisplayAlert("Alertas", "Nenhum alerta foi disparado.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao verificar alertas: {ex.Message}", "OK");
        }
    }

    private async void OnTaxReportClicked(object sender, EventArgs e)
    {
        try
        {
            var year = DateTime.Now.Year;
            var serviceProvider = Handler?.MauiContext?.Services;
            var investmentService = serviceProvider?.GetService<IInvestmentService>();
            if (investmentService == null) return;

            var report = await investmentService.GenerateTaxReportAsync(year);

            var message = $"📊 Relatório de Impostos {year}\n\n" +
                         $"💰 Total de Dividendos: {report.TotalDividends:C}\n" +
                         $"📈 Lucro Day Trade: {report.ProfitFromDayTrade:C}\n" +
                         $"📉 Prejuízo Day Trade: {report.LossFromDayTrade:C}\n" +
                         $"📈 Lucro Swing Trade: {report.ProfitFromSwingTrade:C}\n" +
                         $"📉 Prejuízo Swing Trade: {report.LossFromSwingTrade:C}\n" +
                         $"💸 Imposto Devido: {report.TaxOwed:C}\n" +
                         $"🧾 Total de Operações: {report.Transactions.Count}";

            await DisplayAlert("Relatório de Impostos", message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao gerar relatório: {ex.Message}", "OK");
        }
    }

    private async void OnExportClicked(object sender, EventArgs e)
    {
        try
        {
            var format = await DisplayActionSheet("Formato de Exportação", "Cancelar", null, "PDF", "Excel", "CSV");
            if (format != "Cancelar" && !string.IsNullOrEmpty(format))
            {
                await _viewModel.ExportPortfolioAsync(format);
                await DisplayAlert("Sucesso", $"Portfolio exportado em {format}!", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao exportar: {ex.Message}", "OK");
        }
    }

    private async void OnItemMenuClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter != null)
        {
            var item = button.CommandParameter;
            var actions = new List<string> { "Detalhes" };

            // Adicionar ações específicas baseadas no tipo
            switch (item)
            {
                case PortfolioPosition:
                    actions.AddRange(new[] { "Adicionar Trade", "Ver Histórico" });
                    break;
                case Trade:
                    actions.Add("Excluir");
                    break;
                case Asset:
                    actions.AddRange(new[] { "Editar Preço", "Adicionar Alerta" });
                    break;
                case InvestmentGoal:
                case Dividend:
                    actions.Add("Excluir");
                    break;
            }

            var action = await DisplayActionSheet("Ações", "Cancelar", null, actions.ToArray());

            switch (action)
            {
                case "Detalhes":
                    await ShowItemDetails(item);
                    break;
                case "Excluir":
                    await DeleteItem(item);
                    break;
                case "Adicionar Trade":
                    await OnAddTradeClicked(sender, e);
                    break;
                case "Editar Preço":
                    await EditAssetPrice(item as Asset);
                    break;
                case "Adicionar Alerta":
                    await AddPriceAlert(item as Asset);
                    break;
                case "Ver Histórico":
                    await ShowAssetHistory(item as PortfolioPosition);
                    break;
            }
        }
    }

    private async Task ShowItemDetails(object item)
    {
        try
        {
            string details = item switch
            {
                PortfolioPosition position => $"📊 {position.AssetSymbol} - {position.AssetName}\n\n" +
                                            $"📦 Quantidade: {position.TotalQuantity} unidades\n" +
                                            $"💰 Preço Médio: {position.AveragePrice:C}\n" +
                                            $"📈 Preço Atual: {position.CurrentPrice:C}\n" +
                                            $"💵 Valor Investido: {position.TotalInvested:C}\n" +
                                            $"💎 Valor Atual: {position.CurrentValue:C}\n" +
                                            $"📊 Rentabilidade: {position.ProfitLossPercentage:F2}%\n" +
                                            $"🎯 Peso no Portfolio: {position.WeightInPortfolio:F1}%\n" +
                                            $"💸 Dividendos Recebidos: {position.DividendsReceived:C}",

                Trade trade => $"💹 Operação - {trade.Asset?.Symbol}\n\n" +
                              $"📊 Tipo: {(trade.Type == TradeType.Buy ? "Compra" : "Venda")}\n" +
                              $"📦 Quantidade: {trade.Quantity} unidades\n" +
                              $"💰 Preço: {trade.Price:C}\n" +
                              $"📅 Data: {trade.Date:dd/MM/yyyy}\n" +
                              $"💵 Valor Bruto: {trade.GrossValue:C}\n" +
                              $"🏦 Taxas: {trade.TotalCosts:C}\n" +
                              $"💎 Valor Líquido: {Math.Abs(trade.NetValue):C}",

                Asset asset => $"📈 {asset.Symbol} - {asset.Name}\n\n" +
                              $"🏷️ Tipo: {asset.Type}\n" +
                              $"💰 Preço Atual: {asset.CurrentPrice:C}\n" +
                              $"📊 Variação Diária: {asset.DailyChangePercentage:F2}%\n" +
                              $"🏛️ Bolsa: {asset.Exchange}\n" +
                              $"📅 Última Atualização: {asset.LastPriceUpdate:dd/MM/yyyy HH:mm}\n" +
                              $"🔄 Status: {asset.Status}",

                InvestmentGoal goal => $"🎯 {goal.Name}\n\n" +
                                     $"💰 Valor Alvo: {goal.TargetAmount:C}\n" +
                                     $"💎 Valor Atual: {goal.CurrentAmount:C}\n" +
                                     $"📊 Progresso: {goal.ProgressPercentage:F1}%\n" +
                                     $"📅 Data Alvo: {goal.TargetDate:dd/MM/yyyy}\n" +
                                     $"💵 Contribuição Mensal: {goal.MonthlyContribution:C}\n" +
                                     $"⏱️ Meses Restantes: {goal.MonthsRemaining}\n" +
                                     $"🎯 Tipo: {goal.Type}",

                Dividend dividend => $"💰 Dividendo - {dividend.Asset?.Symbol}\n\n" +
                                   $"💵 Valor por Ação: {dividend.AmountPerShare:C}\n" +
                                   $"📅 Data de Pagamento: {dividend.PaymentDate:dd/MM/yyyy}\n" +
                                   $"📊 Tipo: {dividend.Type}\n" +
                                   $"📋 Descrição: {dividend.Description}",

                _ => "Detalhes não disponíveis para este item."
            };

            await DisplayAlert("Detalhes", details, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao exibir detalhes: {ex.Message}", "OK");
        }
    }

    private async Task DeleteItem(object item)
    {
        var confirmed = await DisplayAlert("Confirmação", "Deseja realmente excluir este item?", "Sim", "Não");
        if (!confirmed) return;

        try
        {
            switch (item)
            {
                case Trade trade:
                    await _viewModel.DeleteTradeAsync(trade.Id);
                    break;
                case InvestmentGoal goal:
                    await _viewModel.DeleteInvestmentGoalAsync(goal.Id);
                    break;
                case Dividend dividend:
                    // Note: Implementar exclusão de dividendo se necessário
                    await DisplayAlert("Info", "Funcionalidade de exclusão de dividendo em desenvolvimento", "OK");
                    return;
            }

            await DisplayAlert("Sucesso", "Item excluído com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao excluir: {ex.Message}", "OK");
        }
    }

    private async Task EditAssetPrice(Asset? asset)
    {
        if (asset == null) return;

        try
        {
            var newPriceStr = await DisplayPromptAsync(
                "Editar Preço",
                $"Novo preço para {asset.Symbol}:",
                initialValue: asset.CurrentPrice.ToString(),
                keyboard: Keyboard.Numeric);

            if (string.IsNullOrWhiteSpace(newPriceStr)) return;

            if (decimal.TryParse(newPriceStr, out decimal newPrice) && newPrice > 0)
            {
                asset.PreviousPrice = asset.CurrentPrice;
                asset.CurrentPrice = newPrice;
                asset.LastPriceUpdate = DateTime.UtcNow;

                var serviceProvider = Handler?.MauiContext?.Services;
                var investmentService = serviceProvider?.GetService<IInvestmentService>();
                if (investmentService != null)
                {
                    await investmentService.UpdateAssetAsync(asset);
                    await _viewModel.RefreshAllDataAsync();
                }

                await DisplayAlert("Sucesso", "Preço atualizado com sucesso!", "OK");
            }
            else
            {
                await DisplayAlert("Erro", "Preço inválido", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao atualizar preço: {ex.Message}", "OK");
        }
    }

    private async Task AddPriceAlert(Asset? asset)
    {
        if (asset == null) return;

        try
        {
            var priceStr = await DisplayPromptAsync("Alerta de Preço", "Preço alvo:", keyboard: Keyboard.Numeric);
            if (!decimal.TryParse(priceStr, out decimal price) || price <= 0)
            {
                await DisplayAlert("Erro", "Preço inválido", "OK");
                return;
            }

            var condition = await DisplayActionSheet("Condição", "Cancelar", null, "Acima", "Abaixo", "Igual");
            if (condition == "Cancelar") return;

            var alertCondition = condition switch
            {
                "Acima" => AlertCondition.Above,
                "Abaixo" => AlertCondition.Below,
                "Igual" => AlertCondition.Equals,
                _ => AlertCondition.Above
            };

            var alert = new PriceAlert
            {
                AssetId = asset.Id,
                TargetPrice = price,
                Condition = alertCondition,
                IsActive = true,
                IsTriggered = false,
                Notes = $"Alerta para {asset.Symbol} - {condition} {price:C}"
            };

            await _viewModel.AddPriceAlertAsync(alert);
            await DisplayAlert("Sucesso", "Alerta criado com sucesso!", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao criar alerta: {ex.Message}", "OK");
        }
    }

    private async Task ShowAssetHistory(PortfolioPosition? position)
    {
        if (position == null) return;

        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            var investmentService = serviceProvider?.GetService<IInvestmentService>();
            if (investmentService == null) return;

            var asset = await investmentService.GetAssetBySymbolAsync(position.AssetSymbol);
            if (asset == null) return;

            var trades = await investmentService.GetTradesByAssetAsync(asset.Id);

            var history = string.Join("\n", trades.Take(10).Select(t =>
                $"{t.Date:dd/MM} - {(t.Type == TradeType.Buy ? "Compra" : "Venda")} - {t.Quantity}un @ {t.Price:C}"));

            if (string.IsNullOrWhiteSpace(history))
            {
                history = "Nenhuma operação encontrada para este ativo.";
            }

            await DisplayAlert($"Histórico - {position.AssetSymbol}", history, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao buscar histórico: {ex.Message}", "OK");
        }
    }

    private async void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is SearchBar searchBar)
        {
            _viewModel.SearchText = searchBar.Text;
        }
    }

    private async void OnRefreshClicked(object sender, EventArgs e)
    {
        try
        {
            await _viewModel.RefreshAllDataAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Erro", $"Erro ao atualizar: {ex.Message}", "OK");
        }
    }
}

// Converter para determinar se um valor é positivo ou negativo (para cores)
public class ProfitLossToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is decimal decimalValue)
        {
            return decimalValue >= 0;
        }

        if (value is double doubleValue)
        {
            return doubleValue >= 0;
        }

        if (value is float floatValue)
        {
            return floatValue >= 0;
        }

        if (value is int intValue)
        {
            return intValue >= 0;
        }

        return true; // Default para verdadeiro (verde)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
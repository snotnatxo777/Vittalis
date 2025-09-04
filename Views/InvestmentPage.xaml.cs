using Vittalis.ViewModels;
using Vittalis.Services;

namespace Vittalis.Views;

public partial class InvestmentPage : ContentPage
{
    private readonly InvestmentViewModel _viewModel;

    public InvestmentPage(InvestmentViewModel viewModel)
    {
        _viewModel = viewModel;
        BindingContext = viewModel;
        CreateUI();
    }


    private void CreateUI()
    {
        Title = "Investimentos";

        // Labels com binding
        var totalValueLabel = new Label
        {
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Blue
        };
        totalValueLabel.SetBinding(Label.TextProperty, new Binding("TotalPortfolioValue", stringFormat: "{0:C}"));

        var totalInvestedLabel = new Label
        {
            FontSize = 16,
            HorizontalOptions = LayoutOptions.Center,
            TextColor = Colors.Gray
        };
        totalInvestedLabel.SetBinding(Label.TextProperty, new Binding("TotalInvested", stringFormat: "Investido: {0:C}"));

        var profitLossLabel = new Label
        {
            FontSize = 18,
            FontAttributes = FontAttributes.Bold,
            HorizontalOptions = LayoutOptions.Center
        };
        profitLossLabel.SetBinding(Label.TextProperty, new Binding("TotalProfitLoss", stringFormat: "{0:C}"));
        profitLossLabel.SetBinding(Label.TextColorProperty, new Binding("TotalProfitLoss", converter: new ProfitLossColorConverter()));

        var profitLossPercentageLabel = new Label
        {
            FontSize = 14,
            HorizontalOptions = LayoutOptions.Center
        };
        profitLossPercentageLabel.SetBinding(Label.TextProperty, new Binding("TotalProfitLossPercentage", stringFormat: "({0:F2}%)"));
        profitLossPercentageLabel.SetBinding(Label.TextColorProperty, new Binding("TotalProfitLoss", converter: new ProfitLossColorConverter()));

        // Botões
        var addTradeButton = new Button
        {
            Text = "Nova Operação",
            BackgroundColor = Colors.Green,
            TextColor = Colors.White,
            Margin = 10
        };
        addTradeButton.Clicked += OnAddTradeClicked;

        var refreshButton = new Button
        {
            Text = "Atualizar",
            BackgroundColor = Colors.Blue,
            TextColor = Colors.White,
            Margin = 10
        };
        refreshButton.Clicked += async (s, e) => await _viewModel.RefreshAsync();

        Content = new ScrollView
        {
            Content = new StackLayout
            {
                Spacing = 15,
                Children =
                {
                    new StackLayout
                    {
                        Orientation = StackOrientation.Horizontal,
                        HorizontalOptions = LayoutOptions.Center,
                        Children = { addTradeButton, refreshButton }
                    },

                    // Resumo do portfólio
                    new Frame
                    {
                        BackgroundColor = Colors.LightBlue,
                        CornerRadius = 10,
                        HasShadow = true,
                        Margin = 15,
                        Content = new StackLayout
                        {
                            Spacing = 10,
                            Children =
                            {
                                new Label { Text = "Valor Total do Portfólio", FontSize = 16, HorizontalOptions = LayoutOptions.Center, TextColor = Colors.DimGray },
                                totalValueLabel,
                                totalInvestedLabel,
                                new StackLayout
                                {
                                    Orientation = StackOrientation.Horizontal,
                                    HorizontalOptions = LayoutOptions.Center,
                                    Children = { profitLossLabel, profitLossPercentageLabel }
                                }
                            }
                        }
                    },

                    // Lista do portfólio
                    new Label { Text = "Meu Portfólio", FontSize = 18, FontAttributes = FontAttributes.Bold, Margin = new Thickness(15, 0), TextColor = Colors.DimGray },
                    CreatePortfolioList(),

                    // Operações recentes
                    new Label { Text = "Operações Recentes", FontSize = 18, FontAttributes = FontAttributes.Bold, Margin = new Thickness(15, 0), TextColor = Colors.DimGray },
                    CreateTradesList()
                }
            }
        };
    }

    private CollectionView CreatePortfolioList()
    {
        var collectionView = new CollectionView
        {
            HeightRequest = 300,
            Margin = 15
        };

        collectionView.SetBinding(ItemsView.ItemsSourceProperty, "Portfolio");

        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                CornerRadius = 8,
                HasShadow = true,
                Margin = new Thickness(0, 5),
                Padding = 15
            };

            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                }
            };

            var leftStack = new StackLayout();

            var symbolLabel = new Label { FontSize = 16, FontAttributes = FontAttributes.Bold, TextColor = Colors.DimGray };
            symbolLabel.SetBinding(Label.TextProperty, "AssetSymbol");

            var nameLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
            nameLabel.SetBinding(Label.TextProperty, "AssetName");

            var quantityLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
            quantityLabel.SetBinding(Label.TextProperty, new Binding("TotalQuantity", stringFormat: "Quantidade: {0}"));

            leftStack.Children.Add(symbolLabel);
            leftStack.Children.Add(nameLabel);
            leftStack.Children.Add(quantityLabel);

            var valueLabel = new Label
            {
                FontSize = 16,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                TextColor = Colors.DimGray
            };
            valueLabel.SetBinding(Label.TextProperty, new Binding("CurrentValue", stringFormat: "{0:C}"));

            var profitLabel = new Label
            {
                FontSize = 12,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End
            };
            profitLabel.SetBinding(Label.TextProperty, new Binding("ProfitLoss", stringFormat: "{0:C}"));
            profitLabel.SetBinding(Label.TextColorProperty, new Binding("ProfitLoss", converter: new ProfitLossColorConverter()));

            Grid.SetColumn(leftStack, 0);
            Grid.SetColumn(valueLabel, 1);
            Grid.SetColumn(profitLabel, 2);

            grid.Children.Add(leftStack);
            grid.Children.Add(valueLabel);
            grid.Children.Add(profitLabel);

            frame.Content = grid;
            return frame;
        });

        return collectionView;
    }

    private CollectionView CreateTradesList()
    {
        var collectionView = new CollectionView
        {
            HeightRequest = 200,
            Margin = 15
        };

        collectionView.SetBinding(ItemsView.ItemsSourceProperty, "RecentTrades");

        collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Star },
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Auto }
                },
                Margin = new Thickness(15, 5),
                Padding = new Thickness(0, 10)
            };

            var leftStack = new StackLayout();

            var assetLabel = new Label { FontSize = 14, FontAttributes = FontAttributes.Bold, TextColor = Colors.DimGray };
            assetLabel.SetBinding(Label.TextProperty, "Asset.Symbol");

            var typeLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
            typeLabel.SetBinding(Label.TextProperty, "Type");

            var dateLabel = new Label { FontSize = 12, TextColor = Colors.Gray };
            dateLabel.SetBinding(Label.TextProperty, new Binding("Date", stringFormat: "{0:dd/MM/yyyy}"));

            leftStack.Children.Add(assetLabel);
            leftStack.Children.Add(typeLabel);
            leftStack.Children.Add(dateLabel);

            var quantityLabel = new Label
            {
                FontSize = 14,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                TextColor = Colors.DimGray
            };
            quantityLabel.SetBinding(Label.TextProperty, new Binding("Quantity", stringFormat: "{0} un"));

            var valueLabel = new Label
            {
                FontSize = 14,
                FontAttributes = FontAttributes.Bold,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.End,
                TextColor = Colors.DimGray
            };
            valueLabel.SetBinding(Label.TextProperty, new Binding("TotalCost", stringFormat: "{0:C}"));

            Grid.SetColumn(leftStack, 0);
            Grid.SetColumn(quantityLabel, 1);
            Grid.SetColumn(valueLabel, 2);

            grid.Children.Add(leftStack);
            grid.Children.Add(quantityLabel);
            grid.Children.Add(valueLabel);

            return grid;
        });

        return collectionView;
    }

    private async void OnAddTradeClicked(object sender, EventArgs e)
    {
        var serviceProvider = Handler?.MauiContext?.Services;
        if (serviceProvider != null)
        {
            var investmentService = serviceProvider.GetService<IInvestmentService>();
            if (investmentService != null)
            {
                var addTradePage = new AddTradePage(investmentService);
                await Navigation.PushAsync(addTradePage);
            }
        }
    }
}

// Converter para cores de lucro/prejuízo
public class ProfitLossColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        if (value is decimal amount)
        {
            return amount >= 0 ? Colors.Green : Colors.Red;
        }
        return Colors.Gray;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
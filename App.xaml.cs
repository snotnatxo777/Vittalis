using Vittalis.Data;

namespace Vittalis;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        MainPage = new NavigationPage(new MainPage());
    }

    protected override async void OnStart()
    {
        base.OnStart();

        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            if (serviceProvider != null)
            {
                var dbContext = serviceProvider.GetService<VittalisDbContext>();
                if (dbContext != null)
                {
                    await DatabaseInitializer.InitializeAsync(dbContext);
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erro ao inicializar banco: {ex.Message}");
        }
    }
}
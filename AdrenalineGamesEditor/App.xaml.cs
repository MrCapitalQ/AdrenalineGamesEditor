using Microsoft.UI.Xaml;

namespace MrCapitalQ.AdrenalineGamesEditor;

public partial class App : Application
{
    public App() => InitializeComponent();

    public static new App Current => (App)Application.Current;
    public Window? Window { get; protected set; }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        Window.Activate();
    }
}

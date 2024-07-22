using Microsoft.UI.Xaml;
using MrCapitalQ.AdrenalineGamesEditor.Games;

namespace MrCapitalQ.AdrenalineGamesEditor;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        RootFrame.Navigate(typeof(GamesListPage));
    }
}

using Microsoft.UI.Xaml;

namespace MrCapitalQ.AdrenalineGamesEditor;

public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();

    private void myButton_Click(object sender, RoutedEventArgs e)
    {
        myButton.Content = "Clicked";
    }
}

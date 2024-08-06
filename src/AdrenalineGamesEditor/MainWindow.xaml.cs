using CommunityToolkit.Mvvm.Messaging;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Games;
using MrCapitalQ.AdrenalineGamesEditor.Shared;
using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel;
using WinUIEx;

namespace MrCapitalQ.AdrenalineGamesEditor;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed partial class MainWindow : WindowEx
{
    public MainWindow(IMessenger messenger)
    {
        InitializeComponent();

        Title = Package.Current.DisplayName;
        ExtendsContentIntoTitleBar = true;
        PersistenceId = nameof(MainWindow);

        RootFrame.Navigated += RootFrame_Navigated;
        RootFrame.Navigate(typeof(GamesListPage));

        messenger.Register<MainWindow, NavigateMessage>(this, (r, m) => r.RootFrame.Navigate(m.SourcePageType, m.Parameter));
    }

    private void GoBack()
    {
        if (RootFrame.CanGoBack)
            RootFrame.GoBack();
    }

    private void GoForward()
    {
        if (RootFrame.CanGoForward)
            RootFrame.GoForward();
    }

    private void TitleBar_BackRequested(object sender, EventArgs e) => GoBack();

    private void Grid_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        var pointerProperties = e.GetCurrentPoint(sender as UIElement).Properties;
        if (pointerProperties.IsXButton1Pressed)
        {
            GoBack();
            e.Handled = true;
        }
        else if (pointerProperties.IsXButton2Pressed)
        {
            GoForward();
            e.Handled = true;
        }
    }

    private void BackKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        GoBack();
        args.Handled = true;
    }

    private void ForwardKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
    {
        GoForward();
        args.Handled = true;
    }

    private void RootFrame_Navigated(object sender, NavigationEventArgs e)
        => TitleBar.IsBackButtonVisible = RootFrame.CanGoBack;
}

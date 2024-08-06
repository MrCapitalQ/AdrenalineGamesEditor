using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using MrCapitalQ.AdrenalineGamesEditor.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed partial class GamesListPage : Page
{
    private readonly GamesListViewModel _viewModel;
    private readonly IMessenger _messenger;

    public GamesListPage()
    {
        InitializeComponent();

        _viewModel = App.Current.Services.GetRequiredService<GamesListViewModel>();
        _messenger = App.Current.Services.GetRequiredService<IMessenger>();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        _messenger.Register<GamesListPage, PickPackagedAppRequestMessage>(this, (r, m) =>
        {
            if (r.Content.XamlRoot is null)
                return;

            var dialog = new PackagedAppPicker() { XamlRoot = r.Content.XamlRoot };

            var tcs = new TaskCompletionSource<PackagedAppListing?>();
            DispatcherQueue.TryEnqueue(async () =>
            {
                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary && dialog.SelectedApp is not null)
                    tcs.SetResult(dialog.SelectedApp);
                else
                    tcs.SetResult(null);
            });

            m.Reply(tcs.Task);
        });
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e) => _messenger.UnregisterAll(this);
}

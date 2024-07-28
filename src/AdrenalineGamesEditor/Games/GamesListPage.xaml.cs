using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

public sealed partial class GamesListPage : Page
{
    private readonly GamesListViewModel _viewModel;

    public GamesListPage()
    {
        InitializeComponent();
        _viewModel = App.Current.Services.GetRequiredService<GamesListViewModel>();
    }
}

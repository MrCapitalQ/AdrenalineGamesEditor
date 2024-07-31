using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml.Controls;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
public sealed partial class GamesListPage : Page
{
    private readonly GamesListViewModel _viewModel;

    public GamesListPage()
    {
        InitializeComponent();
        _viewModel = App.Current.Services.GetRequiredService<GamesListViewModel>();
    }
}

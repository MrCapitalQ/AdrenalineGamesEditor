using CommunityToolkit.WinUI.Collections;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Games;

namespace MrCapitalQ.AdrenalineGamesEditor.Tests.Games;

public class GamesListViewModelTests
{
    private readonly (Guid, string) _gameTestData1 = (Guid.NewGuid(), "Test Game 1");
    private readonly (Guid, string) _gameTestData2 = (Guid.NewGuid(), "Test Game 2");
    private readonly (Guid, string) _gameTestData3 = (Guid.NewGuid(), "Test Game 3");
    private readonly IAdrenalineGamesDataService _adrenalineGamesDataService = Substitute.For<IAdrenalineGamesDataService>();
    private readonly IDispatcherQueue _dispatcherQueue = Substitute.For<IDispatcherQueue>();

    public GamesListViewModelTests()
    {
        _dispatcherQueue.TryEnqueue(Arg.Any<Action>()).Returns(x =>
        {
            x.Arg<Action>().Invoke();
            return true;
        });
    }

    [Fact]
    public void Ctor_InitializesGamesCollectionView()
    {
        var games = new List<AdrenalineGameInfo> { CreateGameInfo(_gameTestData2), CreateGameInfo(_gameTestData1) };
        var expectedGameVms = games.Select(GameListItemViewModel.CreateFromInfo).OrderBy(x => x.DisplayName);
        _adrenalineGamesDataService.GamesData.Returns(games);
        var expectedSort = new SortDescription(nameof(GameListItemViewModel.DisplayName), SortDirection.Ascending);

        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue);

        var actualSort = Assert.Single(viewModel.GamesCollectionView.SortDescriptions);
        Assert.Equivalent(expectedSort, actualSort);
        Assert.Equal(expectedGameVms, viewModel.GamesCollectionView.Cast<GameListItemViewModel>(), AreEqual);
    }

    [Fact]
    public void DataService_GamesDataChanged_UpdatesGamesCollectionView()
    {
        var initialGames = new List<AdrenalineGameInfo> { CreateGameInfo(_gameTestData2), CreateGameInfo(_gameTestData1) };
        var updatedGames = new List<AdrenalineGameInfo> { CreateGameInfo(_gameTestData3), CreateGameInfo(_gameTestData1) };
        var expectedGameVms = updatedGames.Select(GameListItemViewModel.CreateFromInfo).OrderBy(x => x.DisplayName);
        _adrenalineGamesDataService.GamesData.Returns(initialGames, updatedGames);
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue);

        _adrenalineGamesDataService.GamesDataChanged += Raise.Event();

        Assert.Equal(expectedGameVms, viewModel.GamesCollectionView.Cast<GameListItemViewModel>(), AreEqual);
    }

    private static AdrenalineGameInfo CreateGameInfo((Guid Id, string DisplayName) data) => new(data.Id,
        data.DisplayName,
        "Path_To_Image.png",
        "Path_To_CommandLine",
        "Path_To_Exe.exe",
        true);

    private static bool AreEqual(GameListItemViewModel expected, GameListItemViewModel actual) => expected.Id == actual.Id
        && expected.DisplayName == actual.DisplayName
        && expected.ImagePath == actual.ImagePath
        && expected.ExePath == actual.ExePath
        && expected.IsManual == actual.IsManual;
}
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Collections;
using MrCapitalQ.AdrenalineGamesEditor.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using MrCapitalQ.AdrenalineGamesEditor.Games;
using MrCapitalQ.AdrenalineGamesEditor.Shared;

namespace MrCapitalQ.AdrenalineGamesEditor.Tests.Games;

public class GamesListViewModelTests
{
    private readonly (Guid, string) _gameTestData1 = (Guid.NewGuid(), "Test Game 1");
    private readonly (Guid, string) _gameTestData2 = (Guid.NewGuid(), "Test Game 2");
    private readonly (Guid, string) _gameTestData3 = (Guid.NewGuid(), "Test Game 3");
    private readonly IAdrenalineGamesDataService _adrenalineGamesDataService = Substitute.For<IAdrenalineGamesDataService>();
    private readonly IDispatcherQueue _dispatcherQueue = Substitute.For<IDispatcherQueue>();
    private readonly IMessenger _messenger = Substitute.For<IMessenger>();
    private readonly IPath _path = Substitute.For<IPath>();

    public GamesListViewModelTests()
    {
        _dispatcherQueue.TryEnqueue(Arg.Any<Action>()).Returns(x =>
        {
            x.Arg<Action>().Invoke();
            return true;
        });
        _path.Exists(Arg.Any<string>()).Returns(true);
    }

    [Fact]
    public void Ctor_InitializesGamesCollectionView()
    {
        var games = new List<AdrenalineGameInfo> { CreateGameInfo(_gameTestData2), CreateGameInfo(_gameTestData1) };
        var expectedGameVms = games.Select(GamesListItemViewModel.CreateFromInfo).OrderBy(x => x.DisplayName);
        _adrenalineGamesDataService.GamesData.Returns(games);
        var expectedSort = new SortDescription(nameof(GamesListItemViewModel.DisplayName), SortDirection.Ascending);

        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);

        var actualSort = Assert.Single(viewModel.GamesCollectionView.SortDescriptions);
        Assert.Equivalent(expectedSort, actualSort);
        Assert.Equal(expectedGameVms, viewModel.GamesCollectionView.Cast<GamesListItemViewModel>(), AreEqual);
    }

    [Fact]
    public void SetShowAutomaticallyAddedGames_False_FiltersOutAutomaticallyAddedgames()
    {
        var matchingGameInfo = CreateGameInfo(_gameTestData1, true, false);
        var games = new List<AdrenalineGameInfo>
        {
            matchingGameInfo,
            CreateGameInfo(_gameTestData2, false, false)
        };
        var expected = GamesListItemViewModel.CreateFromInfo(matchingGameInfo);
        _adrenalineGamesDataService.GamesData.Returns(games);

        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path)
        {
            ShowAutomaticallyDetectedGames = false
        };

        var actual = Assert.Single(viewModel.GamesCollectionView.Cast<GamesListItemViewModel>());
        Assert.Equal(expected, actual, AreEqual);
    }

    [Fact]
    public void SetShowManuallyAddedGames_False_FiltersOutManuallyAddedgames()
    {
        var matchingGameInfo = CreateGameInfo(_gameTestData1, false, false);
        var games = new List<AdrenalineGameInfo>
        {
            matchingGameInfo,
            CreateGameInfo(_gameTestData2, true, false)
        };
        var expected = GamesListItemViewModel.CreateFromInfo(matchingGameInfo);
        _adrenalineGamesDataService.GamesData.Returns(games);

        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path)
        {
            ShowManuallyAddedGames = false
        };

        var actual = Assert.Single(viewModel.GamesCollectionView.Cast<GamesListItemViewModel>());
        Assert.Equal(expected, actual, AreEqual);
    }

    [Fact]
    public void SetShowHiddenGames_True_IncludesHiddenGames()
    {
        var games = new List<AdrenalineGameInfo>
        {
            CreateGameInfo(_gameTestData1, true, true),
            CreateGameInfo(_gameTestData2, true, false)
        };
        var expectedGameVms = games.Select(GamesListItemViewModel.CreateFromInfo).OrderBy(x => x.DisplayName);
        _adrenalineGamesDataService.GamesData.Returns(games);

        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path)
        {
            ShowHiddenGames = true
        };

        Assert.Equal(expectedGameVms, viewModel.GamesCollectionView.Cast<GamesListItemViewModel>(), AreEqual);
    }

    [Fact]
    public void DataService_GamesDataChanged_UpdatesGamesCollectionView()
    {
        var initialGames = new List<AdrenalineGameInfo> { CreateGameInfo(_gameTestData2), CreateGameInfo(_gameTestData1) };
        var updatedGames = new List<AdrenalineGameInfo> { CreateGameInfo(_gameTestData3), CreateGameInfo(_gameTestData1) };
        var expectedGameVms = updatedGames.Select(GamesListItemViewModel.CreateFromInfo).OrderBy(x => x.DisplayName);
        _adrenalineGamesDataService.GamesData.Returns(initialGames, updatedGames);
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);

        _adrenalineGamesDataService.GamesDataChanged += Raise.Event();

        Assert.Equal(expectedGameVms, viewModel.GamesCollectionView.Cast<GamesListItemViewModel>(), AreEqual);
    }

    [Fact]
    public void DataService_GamesDataChanged_GameImagePathDoesNotExists_SetsRequiresAttentionToTrueForGame()
    {
        var game = CreateGameInfo(_gameTestData1);
        _adrenalineGamesDataService.GamesData.Returns([game]);
        _path.Exists(game.ImagePath).Returns(false);
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);

        _adrenalineGamesDataService.GamesDataChanged += Raise.Event();

        var actual = Assert.Single(viewModel.GamesCollectionView.OfType<GamesListItemViewModel>());
        Assert.True(actual.RequiresAttention);
    }

    [Fact]
    public void DataService_GamesDataChanged_GameExePathDoesNotExists_SetsRequiresAttentionToTrueForGame()
    {
        var game = CreateGameInfo(_gameTestData1);
        _adrenalineGamesDataService.GamesData.Returns([game]);
        _path.Exists(game.ExePath).Returns(false);
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);

        _adrenalineGamesDataService.GamesDataChanged += Raise.Event();

        var actual = Assert.Single(viewModel.GamesCollectionView.OfType<GamesListItemViewModel>());
        Assert.True(actual.RequiresAttention);
    }

    [Fact]
    public void AddGameCommand_NoGameSelected_DoesNothing()
    {
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);
        _messenger.Send(Arg.Any<PickPackagedAppRequestMessage>(), Arg.Any<TestMessengerToken>()).Returns(x =>
        {
            var request = x.Arg<PickPackagedAppRequestMessage>();
            request.Reply(Task.FromResult<PackagedAppListing?>(null));
            return request;
        });

        viewModel.AddGameCommand.Execute(null);

        _messenger.Received(1).Send(Arg.Any<PickPackagedAppRequestMessage>(), Arg.Any<TestMessengerToken>());
        _messenger.Received(0).Send(Arg.Any<NavigateMessage>(), Arg.Any<TestMessengerToken>());
    }

    [Fact]
    public void AddGameCommand_GameSelected_SendsNavigateMessageWithSelectedGameId()
    {
        var selectedGame = new PackagedAppListing("Test Game", "Package!App", @"C:\InstalledLocation", DateTimeOffset.MinValue, null, null, true);
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);
        _messenger.Send(Arg.Any<PickPackagedAppRequestMessage>(), Arg.Any<TestMessengerToken>()).Returns(x =>
        {
            var request = x.Arg<PickPackagedAppRequestMessage>();
            request.Reply(Task.FromResult<PackagedAppListing?>(selectedGame));
            return request;
        });

        viewModel.AddGameCommand.Execute(null);

        _messenger.Received(1).Send(Arg.Any<PickPackagedAppRequestMessage>(), Arg.Any<TestMessengerToken>());
        _messenger.Received(1).Send(Arg.Is<NavigateMessage>(x => x.SourcePageType == typeof(GameEditPage) && (x.Parameter as string) == selectedGame.AppUserModelId), Arg.Any<TestMessengerToken>());
    }

    [Fact]
    public async Task RestartAdrenalineAsync_UpdatesIsAdrenalineRestarting()
    {
        // Arrange
        var tcs = new TaskCompletionSource<bool>();
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);
        _adrenalineGamesDataService.RestartAdrenalineAsync().Returns(tcs.Task);

        // Act
        _ = viewModel.RestartAdrenalineCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.IsAdrenalineRestarting);

        // Arrange
        tcs.SetResult(true);

        // Assert
        Assert.False(viewModel.IsAdrenalineRestarting);
        await _adrenalineGamesDataService.Received(1).RestartAdrenalineAsync();
    }

    [Fact]
    public async Task RestartAdrenalineAsync_RestartFails_SetsDidAdrenalineRestartFailToTrue()
    {
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);
        _adrenalineGamesDataService.RestartAdrenalineAsync().Returns(false);

        await viewModel.RestartAdrenalineCommand.ExecuteAsync(null);

        Assert.True(viewModel.DidAdrenalineRestartFail);
    }

    [Fact]
    public void EditGameCommand()
    {
        var clickedItem = new GamesListItemViewModel() { Id = Guid.NewGuid() };
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);

        viewModel.EditGameCommand.Execute(clickedItem);

        _messenger.Received(1).Send(Arg.Is<NavigateMessage>(x => x.SourcePageType == typeof(GameEditPage) && (x.Parameter as Guid?) == clickedItem.Id), Arg.Any<TestMessengerToken>());
    }

    [Fact]
    public void DataService_IsRestartRequiredChanged_UpdatesIsAdrenalineRestarting()
    {
        var viewModel = new GamesListViewModel(_adrenalineGamesDataService, _dispatcherQueue, _messenger, _path);
        _adrenalineGamesDataService.IsRestartRequired.Returns(true);

        _adrenalineGamesDataService.IsRestartRequiredChanged += Raise.Event();

        Assert.True(viewModel.IsAdrenalineRestartRequired);
    }

    private static AdrenalineGameInfo CreateGameInfo((Guid Id, string DisplayName) data,
        bool isManual = true,
        bool isHidden = false) => new(data.Id,
            data.DisplayName,
            @"C:\Path\Image.png",
            "Test-Command",
            @"C:\Path\Executable.exe",
            isManual,
            isHidden);

    private static bool AreEqual(GamesListItemViewModel expected, GamesListItemViewModel actual) => expected.Id == actual.Id
        && expected.DisplayName == actual.DisplayName
        && expected.EffectiveImagePath == actual.EffectiveImagePath
        && expected.IsManual == actual.IsManual
        && expected.IsHidden == actual.IsHidden;
}
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using System.Collections.ObjectModel;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GamesListViewModel : ObservableObject
{
    private readonly IAdrenalineGamesDataService _dataService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly Dictionary<Guid, GameListItemViewModel> _gamesDictionary = [];
    private readonly ObservableCollection<GameListItemViewModel> _games = [];

    public AdvancedCollectionView GamesCollectionView { get; }

    public GamesListViewModel(IAdrenalineGamesDataService dataService, IDispatcherQueue dispatcherQueue)
    {
        _dataService = dataService;
        _dataService.GamesDataChanged += DataService_GamesDataChanged;

        _dispatcherQueue = dispatcherQueue;

        GamesCollectionView = new(_games, true);
        GamesCollectionView.SortDescriptions.Add(new(nameof(GameListItemViewModel.DisplayName), SortDirection.Ascending));

        UpdateGamesList();
    }

    private void UpdateGamesList()
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var removedId = _gamesDictionary.Keys.ToHashSet();

            foreach (var gameInfo in _dataService.GamesData)
            {
                removedId.Remove(gameInfo.Id);

                if (_gamesDictionary.TryGetValue(gameInfo.Id, out var gameListItemViewModel))
                {
                    gameListItemViewModel.UpdateFromInfo(gameInfo);
                }
                else
                {
                    var gamesListItemViewModel = GameListItemViewModel.CreateFromInfo(gameInfo);
                    _games.Add(gamesListItemViewModel);
                    _gamesDictionary[gameInfo.Id] = gamesListItemViewModel;
                }
            }

            foreach (var id in removedId)
            {
                _games.Remove(_gamesDictionary[id]);
                _gamesDictionary.Remove(id);
            }
        });
    }

    private void DataService_GamesDataChanged(object? sender, EventArgs e) => UpdateGamesList();
}

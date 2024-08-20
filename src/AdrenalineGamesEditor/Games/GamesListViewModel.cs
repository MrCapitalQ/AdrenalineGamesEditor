using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using CommunityToolkit.WinUI.Collections;
using MrCapitalQ.AdrenalineGamesEditor.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using MrCapitalQ.AdrenalineGamesEditor.Shared;
using System.Collections.ObjectModel;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GamesListViewModel : ObservableObject
{
    private readonly IAdrenalineGamesDataService _dataService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly IMessenger _messenger;
    private readonly IPath _path;
    private readonly Dictionary<Guid, GamesListItemViewModel> _gamesDictionary = [];
    private readonly ObservableCollection<GamesListItemViewModel> _games = [];

    [ObservableProperty]
    private bool _isAdrenalineRestartRequired;

    [ObservableProperty]
    private bool _isAdrenalineRestarting;

    [ObservableProperty]
    private bool _didAdrenalineRestartFail;

    [ObservableProperty]
    private bool _showAutomaticallyDetectedGames = true;

    [ObservableProperty]
    private bool _showManuallyAddedGames = true;

    [ObservableProperty]
    private bool _showHiddenGames;

    public GamesListViewModel(IAdrenalineGamesDataService dataService,
        IDispatcherQueue dispatcherQueue,
        IMessenger messenger,
        IPath path)
    {
        _dataService = dataService;
        _dataService.GamesDataChanged += DataService_GamesDataChanged;
        _dataService.IsRestartRequiredChanged += DataService_IsRestartRequiredChanged;

        _dispatcherQueue = dispatcherQueue;
        _messenger = messenger;
        _path = path;

        GamesCollectionView = new(_games, true)
        {
            Filter = x =>
            {
                var game = (GamesListItemViewModel)x;

                var isVisibleForAddMethod = (ShowAutomaticallyDetectedGames && !game.IsManual)
                    || (ShowManuallyAddedGames && game.IsManual);
                var isVisibleForHiddenStatus = ShowHiddenGames || !game.IsHidden;

                return isVisibleForAddMethod && isVisibleForHiddenStatus;
            }
        };
        GamesCollectionView.SortDescriptions.Add(new(nameof(GamesListItemViewModel.DisplayName), SortDirection.Ascending));

        UpdateGamesList();
        IsAdrenalineRestartRequired = _dataService.IsRestartRequired;
    }

    public AdvancedCollectionView GamesCollectionView { get; }

    private void UpdateGamesList()
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            var removedId = _gamesDictionary.Keys.ToHashSet();

            foreach (var gameInfo in _dataService.GamesData)
            {
                removedId.Remove(gameInfo.Id);

                if (_gamesDictionary.TryGetValue(gameInfo.Id, out var itemViewModel))
                {
                    itemViewModel.UpdateFromInfo(gameInfo);
                }
                else
                {
                    itemViewModel = GamesListItemViewModel.CreateFromInfo(gameInfo);
                    _games.Add(itemViewModel);
                    _gamesDictionary[gameInfo.Id] = itemViewModel;
                }

                itemViewModel.RequiresAttention = (!string.IsNullOrEmpty(gameInfo.ImagePath) && !_path.Exists(gameInfo.ImagePath))
                    || (!string.IsNullOrEmpty(gameInfo.ExePath) && !_path.Exists(gameInfo.ExePath));
            }

            foreach (var id in removedId)
            {
                _games.Remove(_gamesDictionary[id]);
                _gamesDictionary.Remove(id);
            }

            GamesCollectionView.RefreshFilter();
        });
    }

    [RelayCommand]
    private async Task AddGameAsync()
    {
        if ((await _messenger.Send<PickPackagedAppRequestMessage>()) is { } selectedApp)
            _messenger.Send(new NavigateMessage(typeof(GameEditPage), selectedApp.AppUserModelId));
    }

    [RelayCommand]
    private async Task RestartAdrenalineAsync()
    {
        IsAdrenalineRestarting = true;

        try
        {
            DidAdrenalineRestartFail = !await _dataService.RestartAdrenalineAsync();
        }
        finally
        {
            IsAdrenalineRestarting = false;
        }
    }

    [RelayCommand]
    private void EditGame(GamesListItemViewModel item)
        => _messenger.Send(new NavigateMessage(typeof(GameEditPage), item.Id));

    private void DataService_GamesDataChanged(object? sender, EventArgs e) => UpdateGamesList();

    private void DataService_IsRestartRequiredChanged(object? sender, EventArgs e)
        => IsAdrenalineRestartRequired = _dataService.IsRestartRequired;

    partial void OnShowAutomaticallyDetectedGamesChanged(bool value) => GamesCollectionView.RefreshFilter();

    partial void OnShowManuallyAddedGamesChanged(bool value) => GamesCollectionView.RefreshFilter();

    partial void OnShowHiddenGamesChanged(bool value) => GamesCollectionView.RefreshFilter();
}

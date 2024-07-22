using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.WinUI.Collections;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Drawing.Imaging;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GamesListViewModel : ObservableObject
{
    private readonly AdrenalineGamesDataService _dataService;

    private readonly Dictionary<Guid, GameListItemViewModel> _gamesDictionary = [];
    private ObservableCollection<GameListItemViewModel> _games = [];

    public AdvancedCollectionView GamesCollectionView { get; }

    public GamesListViewModel(AdrenalineGamesDataService dataService)
    {
        _dataService = dataService;
        _dataService.DataChanged += DataService_DataChanged;

        GamesCollectionView = new(_games, true);
        GamesCollectionView.SortDescriptions.Add(new(nameof(GameListItemViewModel.DisplayName), SortDirection.Ascending));

        UpdateGamesList();
    }

    private void UpdateGamesList()
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
                var gamesListItemViewModel = GameListItemViewModel.FromInfo(gameInfo);
                _games.Add(gamesListItemViewModel);
                _gamesDictionary[gameInfo.Id] = gamesListItemViewModel;
            }
        }

        foreach (var id in removedId)
        {
            _games.Remove(_gamesDictionary[id]);
            _gamesDictionary.Remove(id);
        }
    }

    private void DataService_DataChanged(object? sender, EventArgs e)
        => App.Current.Window?.DispatcherQueue.TryEnqueue(UpdateGamesList);
}

internal partial class GameListItemViewModel : ObservableObject
{

    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    private string _exePath = string.Empty;

    [ObservableProperty]
    private bool _isManual;

    public Guid Id { get; init; }

    public static GameListItemViewModel FromInfo(AdrenalineGameInfo adrenalineGameInfo) => new()
    {
        Id = adrenalineGameInfo.Id,
        DisplayName = adrenalineGameInfo.DisplayName,
        ImagePath = adrenalineGameInfo.ImagePath,
        ExePath = adrenalineGameInfo.ExePath,
        IsManual = adrenalineGameInfo.IsManual
    };

    public void UpdateFromInfo(AdrenalineGameInfo adrenalineGameInfo)
    {
        DisplayName = adrenalineGameInfo.DisplayName;
        ImagePath = adrenalineGameInfo.ImagePath;
        ExePath = adrenalineGameInfo.ExePath;
        IsManual = adrenalineGameInfo.IsManual;
    }
}

internal class GameImageConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, string language)
    {
        if (value is not GameListItemViewModel game)
            return null;

        if (!string.IsNullOrEmpty(game.ImagePath))
            return new BitmapImage(new Uri(game.ImagePath));

        using var icon = GetExeIcon(game.ExePath);
        return CreateFromIcon(icon);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, string language)
        => throw new NotImplementedException();

    private static Icon? GetExeIcon(string path)
    {
        Icon? icon = null;

        try
        {
            icon = Icon.ExtractIcon(path, 0, 96);
        }
        catch (IOException)
        { }

        if (icon is not null)
            return icon;

        try
        {
            icon = Icon.ExtractAssociatedIcon(path);
        }
        catch (IOException)
        { }

        return icon;
    }
    private static BitmapImage? CreateFromIcon(Icon? icon)
    {
        if (icon is null)
            return null;

        using var iconBitmap = icon.ToBitmap();
        using var memoryStream = new MemoryStream();

        iconBitmap.Save(memoryStream, ImageFormat.Jpeg);
        memoryStream.Position = 0;

        var image = new BitmapImage();
        image.SetSource(memoryStream.AsRandomAccessStream());

        return image;
    }
}

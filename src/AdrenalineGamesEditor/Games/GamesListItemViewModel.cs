using CommunityToolkit.Mvvm.ComponentModel;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GameListItemViewModel : ObservableObject, IAdrenalineGameImageInfo
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GameImage))]
    private string _imagePath = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(GameImage))]
    private string _exePath = string.Empty;

    [ObservableProperty]
    private bool _isManual;

    public GameListItemViewModel() => GameImage = new(this);

    public Guid Id { get; init; }

    public GameImageAdapter GameImage { get; }

    public static GameListItemViewModel CreateFromInfo(AdrenalineGameInfo adrenalineGameInfo) => new()
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

    internal class GameImageAdapter(GameListItemViewModel viewModel) : IAdrenalineGameImageInfo
    {
        private readonly GameListItemViewModel _viewModel = viewModel;

        public string? ImagePath => _viewModel.ImagePath;
        public string? ExePath => _viewModel.ExePath;
    }
}

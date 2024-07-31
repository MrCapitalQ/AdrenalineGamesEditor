using CommunityToolkit.Mvvm.ComponentModel;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

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
}

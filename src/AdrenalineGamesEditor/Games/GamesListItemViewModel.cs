using CommunityToolkit.Mvvm.ComponentModel;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GamesListItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _displayName = string.Empty;

    [ObservableProperty]
    private string _effectiveImagePath = string.Empty;

    [ObservableProperty]
    private bool _isManual;

    [ObservableProperty]
    private bool _isHidden;

    [ObservableProperty]
    private bool _requiresAttention;

    public Guid Id { get; init; }

    public static GamesListItemViewModel CreateFromInfo(AdrenalineGameInfo adrenalineGameInfo) => new()
    {
        Id = adrenalineGameInfo.Id,
        DisplayName = adrenalineGameInfo.DisplayName,
        EffectiveImagePath = GetEffectiveImagePath(adrenalineGameInfo),
        IsManual = adrenalineGameInfo.IsManual,
        IsHidden = adrenalineGameInfo.IsHidden
    };

    public void UpdateFromInfo(AdrenalineGameInfo adrenalineGameInfo)
    {
        DisplayName = adrenalineGameInfo.DisplayName;
        EffectiveImagePath = !string.IsNullOrEmpty(adrenalineGameInfo.ImagePath)
            ? adrenalineGameInfo.ImagePath
            : adrenalineGameInfo.ExePath;
        IsManual = adrenalineGameInfo.IsManual;
        IsHidden = adrenalineGameInfo.IsHidden;
    }

    private static string GetEffectiveImagePath(AdrenalineGameInfo adrenalineGameInfo)
        => !string.IsNullOrEmpty(adrenalineGameInfo.ImagePath)
            ? adrenalineGameInfo.ImagePath
            : adrenalineGameInfo.ExePath;
}

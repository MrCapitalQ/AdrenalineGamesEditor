namespace MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

public record AdrenalineGameInfo(Guid Id,
    string DisplayName,
    string ImagePath,
    string CommandLine,
    string ExePath,
    bool IsManual,
    bool IsHidden);

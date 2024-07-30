namespace MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

public interface IAdrenalineGamesDataService
{
    event EventHandler? GamesDataChanged;

    IReadOnlyCollection<AdrenalineGameInfo> GamesData { get; }
}
﻿namespace MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;

public interface IAdrenalineGamesDataService
{
    event EventHandler? GamesDataChanged;
    event EventHandler? IsRestartRequiredChanged;

    IReadOnlyCollection<AdrenalineGameInfo> GamesData { get; }
    bool IsRestartRequired { get; }

    Task SaveAsync(AdrenalineGameInfo gameInfo);
    Task<bool> RestartAdrenalineAsync();
}
﻿namespace MrCapitalQ.AdrenalineGamesEditor.Core.Apps;

public record PackagedAppListing(string DisplayName,
    string AppUserModelId,
    string InstalledPath,
    DateTimeOffset InstalledDate,
    string? Square44x44Logo,
    string? Square150x150Logo,
    bool IsGame)
    : IPackagedAppIconInfo;

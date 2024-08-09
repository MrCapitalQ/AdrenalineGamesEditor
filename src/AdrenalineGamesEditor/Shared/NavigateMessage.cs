namespace MrCapitalQ.AdrenalineGamesEditor.Shared;

internal record NavigateMessage(Type SourcePageType, object? Parameter = null);

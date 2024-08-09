namespace MrCapitalQ.AdrenalineGamesEditor.Shared;

internal record NavigateMessage(Type SourcePageType, object? Parameter = null);

internal class NavigateBackMessage
{
    private NavigateBackMessage() { }

    public static NavigateBackMessage Instance { get; } = new();
}

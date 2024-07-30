namespace MrCapitalQ.AdrenalineGamesEditor.Core.Apps;

public interface IPackagedAppIconInfo
{
    string InstalledPath { get; init; }
    string? Square150x150Logo { get; init; }
    string? Square44x44Logo { get; init; }
}
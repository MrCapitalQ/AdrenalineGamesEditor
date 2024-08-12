namespace MrCapitalQ.AdrenalineGamesEditor.Core.Apps;

public interface IQualifiedFileResolver
{
    string GetPath(string packageInstalledPath, string relativeFilePath, IDictionary<string, string>? qualifiers = null);
}

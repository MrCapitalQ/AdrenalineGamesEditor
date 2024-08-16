using Microsoft.Windows.ApplicationModel.Resources;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Apps;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
internal class QualifiedFileResolver : IQualifiedFileResolver
{
    private static readonly Dictionary<string, ResourceManager> s_resourceManagers = [];
    private static readonly Dictionary<string, ResourceContext> s_resourceContexts = [];
    private static readonly Dictionary<string, ResourceMap?> s_resourceMaps = [];

    public string GetPath(string packageInstalledPath, string relativeFilePath, IDictionary<string, string>? qualifiers = null)
    {
        var nonQualifiedPath = Path.Combine(packageInstalledPath, relativeFilePath);
        var priPath = Path.Combine(packageInstalledPath, "resources.pri");
        if (!Path.Exists(priPath))
            return nonQualifiedPath;

        if (!s_resourceManagers.ContainsKey(priPath))
            s_resourceManagers[priPath] = new ResourceManager(priPath);

        var resourceManager = s_resourceManagers[priPath];

        if (!s_resourceContexts.ContainsKey(priPath))
            s_resourceContexts[priPath] = resourceManager.CreateResourceContext();

        var resourceContext = s_resourceContexts[priPath];
        foreach (var kvp in qualifiers ?? Enumerable.Empty<KeyValuePair<string, string>>())
        {
            resourceContext.QualifierValues[kvp.Key] = kvp.Value;
        }

        if (!s_resourceMaps.ContainsKey(priPath))
            s_resourceMaps[priPath] = resourceManager.MainResourceMap.TryGetSubtree("Files");

        var resourceMap = s_resourceMaps[priPath];
        if (resourceMap is null)
            return nonQualifiedPath;

        var path = resourceMap.GetValue(relativeFilePath, resourceContext).ValueAsString;

        return !string.IsNullOrEmpty(path) ? path : nonQualifiedPath;
    }
}

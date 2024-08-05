using Microsoft.Windows.ApplicationModel.Resources;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;

[ExcludeFromCodeCoverage(Justification = ExcludeFromCoverageJustifications.RequiresUIThread)]
internal class QualifiedFileResolver : IQualifiedFileResolver
{
    public string GetPath(string packageInstalledPath, string relativeFilePath)
    {
        var nonQualifiedPath = Path.Combine(packageInstalledPath, relativeFilePath);
        var priPath = Path.Combine(packageInstalledPath, "resources.pri");
        if (!Path.Exists(priPath))
            return nonQualifiedPath;

        var resourceManager = new ResourceManager(priPath);
        var resourceContext = resourceManager.CreateResourceContext();

        var resourceMap = resourceManager.MainResourceMap.TryGetSubtree("Files");
        if (resourceMap is null)
            return nonQualifiedPath;

        var path = resourceMap.GetValue(relativeFilePath, resourceContext).ValueAsString;

        return !string.IsNullOrEmpty(path) ? path : nonQualifiedPath;
    }
}

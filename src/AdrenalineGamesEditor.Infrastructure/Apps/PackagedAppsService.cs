using Microsoft.Extensions.Logging;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps.Models;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;

internal class PackagedAppsService(ILogger<PackagedAppsService> logger) : IPackagedAppsService
{
    private const string MicrosoftGameConfigFileName = "MicrosoftGame.config";

    private readonly PackageManager _packageManager = new();
    private readonly XmlSerializer _appxManifestSerializer = new(typeof(AppxManifestPackage));
    private readonly ILogger<PackagedAppsService> _logger = logger;

    public async Task<IEnumerable<PackagedAppListing>> GetAllAsync()
    {
        var result = await Task.WhenAll(_packageManager.FindPackagesForUser(string.Empty).Select(GetForPackage));

        return result.SelectMany(x => x);
    }

    private async Task<IEnumerable<PackagedAppListing>> GetForPackage(Package package)
    {
        List<PackagedAppListing> result = [];

        var path = Path.Combine(package.InstalledPath, "appxmanifest.xml");
        if (!File.Exists(path))
        {
            _logger.LogWarning("An appxmanifest could not be found for package {PackageName}.", package.Id.FullName);
            return [];
        }

        AppxManifestPackage? manifest = null;
        try
        {
            using var reader = new IgnoreNamespaceXmlTextReader(path);
            manifest = _appxManifestSerializer.Deserialize(reader) as AppxManifestPackage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deserialize of appxmanifest for package {PackageName} failed.", package.Id.FullName);
        }

        var isGame = File.Exists(Path.Combine(package.InstalledPath, MicrosoftGameConfigFileName)) || File.Exists(Path.Combine(package.InstalledPath, "xboxservices.config"));

        foreach (var appListEntry in await package.GetAppListEntriesAsync())
        {
            var applicationId = appListEntry.GetApplicationId();
            var application = manifest?.Applications.FirstOrDefault(x => string.Equals(x.Id, applicationId));
            if (application is null)
            {
                _logger.LogWarning("Appxmanifest application with ID '{AppId}' could not be found for package {PackageName}.",
                    applicationId,
                    package.Id.FullName);
            }

            result.Add(new(appListEntry.DisplayInfo.DisplayName,
                appListEntry.AppUserModelId,
                package.InstalledPath,
                package.InstalledDate,
                application?.VisualElements?.Square44x44Logo,
                application?.VisualElements?.Square150x150Logo,
                isGame));
        }

        return result;
    }
}

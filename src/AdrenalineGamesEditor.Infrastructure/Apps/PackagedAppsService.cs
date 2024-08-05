using Microsoft.Extensions.Logging;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps.Models;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;

internal class PackagedAppsService(ILogger<PackagedAppsService> logger) : IPackagedAppsService
{
    private const string MicrosoftGameConfigFileName = "MicrosoftGame.config";
    private const string XboxServicesConfigFileName = "xboxservices.config";

    private readonly PackageManager _packageManager = new();
    private readonly XmlSerializer _appxManifestSerializer = new(typeof(AppxManifestPackage));
    private readonly ILogger<PackagedAppsService> _logger = logger;

    public async Task<IEnumerable<PackagedAppListing>> GetAllAsync()
    {
        var result = await Task.WhenAll(_packageManager.FindPackagesForUser(string.Empty).Select(GetForPackage));

        return result.SelectMany(x => x);
    }

    public async Task<PackagedAppInfo?> GetInfoAsync(string appUserModelId)
    {
        var aumid = AppUserModelId.Parse(appUserModelId);
        var packages = _packageManager.FindPackagesForUser(string.Empty, aumid.PackageFamilyName);
        if (!(packages.OrderByDescending(x => GetVersion(x.Id.Version)).FirstOrDefault() is { } package))
            return null;

        var appListEntry = (await package.GetAppListEntriesAsync()).FirstOrDefault(x => x.AppUserModelId == appUserModelId);
        if (appListEntry is null)
        {
            _logger.LogWarning("App list entry with ID '{AppUserModelId}' could not be found for package {PackageName}.",
                appUserModelId,
                package.Id.FullName);
            return null;
        }

        var application = GetAppxManifest(package)?.Applications.FirstOrDefault(x => x.Id == aumid.PackageAppId);
        if (application is null)
        {
            _logger.LogWarning("Appxmanifest application with ID '{AppUserModelId}' could not be found for package {PackageName}.",
                appUserModelId,
                package.Id.FullName);
        }

        return new(appListEntry.DisplayInfo.DisplayName,
            appListEntry.AppUserModelId,
            package.InstalledPath,
            package.InstalledDate,
            application?.VisualElements?.Square44x44Logo,
            application?.VisualElements?.Square150x150Logo,
            IsGame(package),
            GetExecutablePath(package.InstalledPath, application),
            Directory.GetFiles(package.InstalledPath, "*.exe", SearchOption.AllDirectories)
                .Select(x => Path.GetRelativePath(package.InstalledPath, x))
                .ToList());
    }

    private async Task<IEnumerable<PackagedAppListing>> GetForPackage(Package package)
    {
        List<PackagedAppListing> result = [];

        var manifest = GetAppxManifest(package);
        if (manifest is null)
            return result;

        foreach (var appListEntry in await package.GetAppListEntriesAsync())
        {
            var application = manifest.Applications.FirstOrDefault(x => x.Id == appListEntry.GetAppUserModelId().PackageAppId);
            if (application is null)
            {
                _logger.LogWarning("Appxmanifest application with ID '{AppUserModelId}' could not be found for package {PackageName}.",
                    appListEntry.AppUserModelId,
                    package.Id.FullName);
            }

            result.Add(new(appListEntry.DisplayInfo.DisplayName,
                appListEntry.AppUserModelId,
                package.InstalledPath,
                package.InstalledDate,
                application?.VisualElements?.Square44x44Logo,
                application?.VisualElements?.Square150x150Logo,
                IsGame(package)));
        }

        return result;
    }

    private AppxManifestPackage? GetAppxManifest(Package package)
    {
        var path = Path.Combine(package.InstalledPath, "appxmanifest.xml");
        if (!File.Exists(path))
        {
            _logger.LogWarning("An appxmanifest could not be found for package {PackageName}.", package.Id.FullName);
            return null;
        }

        try
        {
            using var reader = new IgnoreNamespaceXmlTextReader(path);
            return _appxManifestSerializer.Deserialize(reader) as AppxManifestPackage;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Deserialize of appxmanifest for package {PackageName} failed.", package.Id.FullName);
            return null;
        }
    }

    private static bool IsGame(Package package)
        => File.Exists(Path.Combine(package.InstalledPath, MicrosoftGameConfigFileName))
        || File.Exists(Path.Combine(package.InstalledPath, XboxServicesConfigFileName));

    private static ulong GetVersion(PackageVersion version) => ((ulong)version.Major << 48)
        | ((ulong)version.Minor << 32)
        | ((ulong)version.Build << 16)
        | version.Revision;

    private static string? GetExecutablePath(string packageInstalledPath, AppxManifestApplication? application)
    {
        if (application is null)
            return null;

        var microsoftGameConfigPath = Path.Combine(packageInstalledPath, MicrosoftGameConfigFileName);
        if (File.Exists(microsoftGameConfigPath))
        {
            var xml = XDocument.Load(microsoftGameConfigPath);
            var executableElement = xml.Descendants("Executable").FirstOrDefault(x => x.Attribute("Id")?.Value == application.Id);
            var exePath = executableElement?.Attribute("Name")?.Value;
            if (exePath is not null)
                return Path.GetRelativePath(".", exePath);
        }

        return application.Executable;
    }
}

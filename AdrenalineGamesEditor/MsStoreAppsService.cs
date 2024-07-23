using Microsoft.Extensions.Logging;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace MrCapitalQ.AdrenalineGamesEditor;

public class MsStoreAppsService(ILogger<MsStoreAppsService> logger)
{
    private const string MicrosoftGameConfigFileName = "MicrosoftGame.config";

    private readonly PackageManager _packageManager = new();
    private readonly XmlSerializer _appxManifestApplicationSerializer = new(typeof(AppxManifestApplication));
    private readonly XmlSerializer _appxManifestSerializer = new(typeof(AppxManifestPackage));
    private readonly ILogger<MsStoreAppsService> _logger = logger;

    public async Task<IEnumerable<MsStoreAppInfo>> GetInstalledAppsAsync()
    {
        var result = await Task.WhenAll(_packageManager.FindPackagesForUser(string.Empty).Select(GetAppInfoAsync));

        return result.SelectMany(x => x);
    }

    // TODO: Fix minimum Windows version.
    // TODO: Clean and optimize.
#pragma warning disable CA1416 // Validate platform compatibility
    private async Task<IEnumerable<MsStoreAppInfo>> GetAppInfoAsync(Package package)
    {
        List<MsStoreAppInfo> result = [];
        try
        {
            var path = Path.Combine(package.InstalledPath, "appxmanifest.xml");
            if (!File.Exists(path))
            {
                _logger.LogWarning("An appxmanifest could not be found for package {PackageName}.", package.Id.FullName);
                return [];
            }

            using var reader = new XmlTextReader(path)
            {
                Namespaces = false
            };
            if (_appxManifestSerializer.Deserialize(reader) is not AppxManifestPackage manifest)
            {
                _logger.LogWarning("Appxmanifest could not be deserialized for package {PackageName}.", package.Id.FullName);
                return [];
            }

            var isGame = File.Exists(Path.Combine(package.InstalledPath, MicrosoftGameConfigFileName)) || File.Exists(Path.Combine(package.InstalledPath, "xboxservices.config"));

            foreach (var appListEntry in await package.GetAppListEntriesAsync())
            {
                var appIdStartIndex = appListEntry.AppUserModelId.LastIndexOf('!') + 1;
                var applicationId = appListEntry.AppUserModelId.AsSpan()[appIdStartIndex..].ToString();
                var application = manifest.Applications.FirstOrDefault(x => string.Equals(x.Id, applicationId));
                if (application is null)
                {
                    _logger.LogWarning("Appxmanifest application with ID '{AppId}' could not be found for package {PackageName}.",
                        applicationId,
                        package.Id.FullName);
                    continue;
                }

                var exePath = application.Executable is not null ? Path.GetFullPath(Path.Combine(package.InstalledPath, application.Executable)) : null;

                // If MicrosoftGame.config exists, it's for sure a game and we can get the real game exe from it.
                if (isGame)
                    exePath = GetGameExecutable(package.InstalledPath, applicationId) ?? exePath;

                result.Add(new(appListEntry.DisplayInfo.DisplayName,
                    appListEntry.AppUserModelId,
                    GetFullLogoPath(package.InstalledPath, application?.VisualElements?.Square44x44Logo),
                    GetFullLogoPath(package.InstalledPath, application?.VisualElements?.Square150x150Logo),
                    exePath,
                    isGame));
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deserialize appxmanifest for package {PackageName}.", package.Id.FullName);
            return [];
        }
    }
#pragma warning restore CA1416 // Validate platform compatibility

    private static string? GetGameExecutable(string installPath, string applicationId)
    {
        var microsoftGameConfigPath = Path.Combine(installPath, MicrosoftGameConfigFileName);
        if (!File.Exists(microsoftGameConfigPath))
            return null;

        var xml = XDocument.Load(microsoftGameConfigPath);
        var executableElement = xml.Descendants("Executable").FirstOrDefault(x => x.Attribute("Id")?.Value == applicationId);
        var exePath = executableElement?.Attribute("Name")?.Value;
        if (exePath is null)
            return null;

        return Path.GetFullPath(Path.Combine(installPath, exePath));
    }

    private static string? GetFullLogoPath(string installPath, string? logoPath)
    {
        if (logoPath is null)
            return null;

        // TODO: Do proper resolution of scaled resources and null checking.
        var logoFilePaths = Directory.GetFiles(Path.GetDirectoryName(Path.Combine(installPath, logoPath))!, Path.GetFileNameWithoutExtension(logoPath) + ".scale-*" + Path.GetExtension(logoPath));
        return logoFilePaths.FirstOrDefault() ?? Path.Combine(installPath, logoPath);
    }

}

public record MsStoreAppInfo(string DisplayName, string ApplicationUserModelId, string? SmallLogoPath, string? LargeLogoPath, string? ExecutablePath, bool IsGame);

[XmlRoot("Package")]
public class AppxManifestPackage
{
    [XmlArray("Applications")]
    [XmlArrayItem("Application")]
    public List<AppxManifestApplication> Applications { get; set; } = [];
}

public record AppxManifestApplication
{
    [XmlAttribute]
    public string? Id { get; init; }

    [XmlAttribute]
    public string? Executable { get; init; }

    [XmlElement("VisualElements")]
    public AppxManifestApplicationVisualElements? VisualElements { get; init; }

}

public record AppxManifestApplicationVisualElements
{
    [XmlAttribute]
    public string? DisplayName { get; init; }

    [XmlAttribute]
    public string? Square150x150Logo { get; init; }

    [XmlAttribute]
    public string? Square44x44Logo { get; init; }

    [XmlAttribute]
    public string? BackgroundColor { get; init; }
}

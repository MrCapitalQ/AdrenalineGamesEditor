using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.FileSystem;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileSystem(this IServiceCollection services)
    {
        services.TryAddTransient<IReadFileStreamCreator, ReadFileStreamCreator>();
        services.TryAddTransient<IFileWriter, FileWriter>();
        services.TryAddTransient<IFileSystemWatcher, FileSystemWatcherAdapter>();
        services.TryAddTransient<IPath, PathAdapter>();
        return services;
    }

    public static IServiceCollection AddPackagedApps(this IServiceCollection services)
    {
        services.TryAddTransient<IPackagedAppsService, PackagedAppsService>();
        return services;
    }

    public static IServiceCollection AddAdrenalineGamesData(this IServiceCollection services)
    {
        services.AddAdrenalineGamesDataCore();
        services.TryAddSingleton<IAdrenalineProcessManager, AdrenalineProcessManager>();
        return services;
    }
}

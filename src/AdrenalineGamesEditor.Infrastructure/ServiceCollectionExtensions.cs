using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.FileSystem;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFileSystem(this IServiceCollection services)
    {
        services.TryAddTransient<IReadFileStreamCreator, ReadFileStreamCreator>();
        services.TryAddTransient<IFileSystemWatcher, FileSystemWatcherAdapter>();
        return services;
    }

    public static IServiceCollection AddPackagedApps(this IServiceCollection services)
    {
        services.TryAddTransient<IPackagedAppsService, PackagedAppsService>();
        return services;
    }
}

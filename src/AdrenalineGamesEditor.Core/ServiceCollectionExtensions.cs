using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Core;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdrenalineGamesDataCore(this IServiceCollection services)
    {
        services.TryAddSingleton<GuidGenerator>();
        services.TryAddSingleton<IAdrenalineGamesDataService, AdrenalineGamesDataService>();
        return services;
    }
}

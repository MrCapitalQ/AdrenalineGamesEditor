using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Adrenaline;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdrenalineGamesDataService(this IServiceCollection services)
    {
        services.TryAddSingleton<IAdrenalineGamesDataService, AdrenalineGamesDataService>();
        return services;
    }
}

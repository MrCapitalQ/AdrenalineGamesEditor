using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Core;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAdrenalineGamesData(this IServiceCollection services)
    {
        services.TryAddSingleton<IAdrenalineGamesDataService, AdrenalineGamesDataService>();
        return services;
    }
}

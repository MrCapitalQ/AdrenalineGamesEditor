using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor;

[ExcludeFromCodeCoverage]
internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddHostedService<WindowsAppHostedService<App>>();

        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<MainWindow>();

        builder.Services.AddAdrenalineGamesDataService();

        var host = builder.Build();

        host.Run();
    }
}

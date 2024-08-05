using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Games;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure;
using MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;
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

        builder.Services.AddSingleton<GamesListViewModel>();
        builder.Services.AddSingleton<GameEditViewModel>();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IDispatcherQueue, DispatcherQueueAdapter>();
        builder.Services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);
        builder.Services.AddTransient<IQualifiedFileResolver, QualifiedFileResolver>();
        builder.Services.AddFileSystem();
        builder.Services.AddAdrenalineGamesData();
        builder.Services.AddPackagedApps();

        var host = builder.Build();

        host.Run();
    }
}

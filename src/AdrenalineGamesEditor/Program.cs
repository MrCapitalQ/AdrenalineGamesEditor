﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MrCapitalQ.AdrenalineGamesEditor.Core;
using MrCapitalQ.AdrenalineGamesEditor.Games;
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

        builder.Services.AddSingleton<GamesListViewModel>();

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<IDispatcherQueue, DispatcherQueueAdapter>();
        builder.Services.AddFileSystem();
        builder.Services.AddAdrenalineGamesData();
        builder.Services.AddPackagedApps();

        var host = builder.Build();

        host.Run();
    }
}

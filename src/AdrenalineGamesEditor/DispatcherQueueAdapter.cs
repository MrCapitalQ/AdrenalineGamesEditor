using MrCapitalQ.AdrenalineGamesEditor.Core;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure;

[ExcludeFromCodeCoverage]
internal class DispatcherQueueAdapter() : IDispatcherQueue
{
    public bool TryEnqueue(Action action) => App.Current.Window?.DispatcherQueue.TryEnqueue(() => action()) ?? false;
}

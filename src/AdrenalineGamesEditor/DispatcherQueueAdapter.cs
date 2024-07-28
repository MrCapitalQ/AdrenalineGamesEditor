using MrCapitalQ.AdrenalineGamesEditor.Core;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure;

internal class DispatcherQueueAdapter() : IDispatcherQueue
{
    public bool TryEnqueue(Action action) => App.Current.Window?.DispatcherQueue.TryEnqueue(() => action()) ?? false;
}

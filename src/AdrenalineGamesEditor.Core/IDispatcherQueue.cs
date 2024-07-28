namespace MrCapitalQ.AdrenalineGamesEditor.Core;

public interface IDispatcherQueue
{
    bool TryEnqueue(Action action);
}

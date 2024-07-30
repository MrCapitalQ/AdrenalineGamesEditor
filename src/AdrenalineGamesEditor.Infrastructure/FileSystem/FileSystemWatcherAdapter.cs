using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.FileSystem;

internal class FileSystemWatcherAdapter : IFileSystemWatcher
{
    public event FileSystemEventHandler? Changed;

    private readonly FileSystemWatcher _watcher = new();

    public FileSystemWatcherAdapter()
    {
        _watcher.Changed += Watcher_Changed;
    }

    private void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        var raiseEvent = Changed;
        raiseEvent?.Invoke(this, e);
    }

    public string Path { get => _watcher.Path; set => _watcher.Path = value; }
    public string Filter { get => _watcher.Filter; set => _watcher.Filter = value; }
    public bool EnableRaisingEvents { get => _watcher.EnableRaisingEvents; set => _watcher.EnableRaisingEvents = value; }

}

namespace MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;

public interface IFileSystemWatcher
{
    event FileSystemEventHandler? Changed;

    string Path { get; set; }
    string Filter { get; set; }
    bool EnableRaisingEvents { get; set; }
}

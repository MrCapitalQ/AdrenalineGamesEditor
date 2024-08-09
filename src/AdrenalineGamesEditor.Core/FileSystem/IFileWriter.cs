namespace MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;

public interface IFileWriter
{
    Task WriteContentAsync(string path, string content);
}

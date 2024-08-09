using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.FileSystem;

internal class FileWriter : IFileWriter
{
    public Task WriteContentAsync(string path, string content) => File.WriteAllTextAsync(path, content);
}

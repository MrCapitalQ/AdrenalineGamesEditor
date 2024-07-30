using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.FileSystem;

internal class ReadFileStreamCreator : IReadFileStreamCreator
{
    public Stream Open(string path) => new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
}

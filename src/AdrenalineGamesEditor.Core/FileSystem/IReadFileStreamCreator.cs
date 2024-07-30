namespace MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;

public interface IReadFileStreamCreator
{
    Stream Open(string path);
}

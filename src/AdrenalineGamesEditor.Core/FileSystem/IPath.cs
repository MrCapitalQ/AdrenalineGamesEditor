using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;

public interface IPath
{
    bool Exists([NotNullWhen(true)] string? path);
}

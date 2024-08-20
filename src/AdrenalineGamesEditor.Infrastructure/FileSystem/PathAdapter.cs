using MrCapitalQ.AdrenalineGamesEditor.Core.FileSystem;
using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.FileSystem;

public class PathAdapter : IPath
{
    public bool Exists([NotNullWhen(true)] string? path) => Path.Exists(path);
}


using System.Diagnostics.CodeAnalysis;

namespace MrCapitalQ.AdrenalineGamesEditor.Core;

[ExcludeFromCodeCoverage]
public class GuidGenerator
{
    public virtual Guid NewGuid() => Guid.NewGuid();
}

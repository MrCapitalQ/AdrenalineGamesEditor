using System.Xml;
using System.Xml.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps.Models;

public record AppxManifestApplicationVisualElements
{
    [XmlAttribute]
    public string? DisplayName { get; init; }

    [XmlAttribute]
    public string? Square150x150Logo { get; init; }

    [XmlAttribute]
    public string? Square44x44Logo { get; init; }

    [XmlAttribute]
    public string? BackgroundColor { get; init; }
}

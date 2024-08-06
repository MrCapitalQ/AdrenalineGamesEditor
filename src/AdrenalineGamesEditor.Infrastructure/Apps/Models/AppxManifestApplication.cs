using System.Xml;
using System.Xml.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps.Models;

public record AppxManifestApplication
{
    [XmlAttribute]
    public string? Id { get; init; }

    [XmlAttribute]
    public string? Executable { get; init; }

    [XmlElement("VisualElements")]
    public AppxManifestApplicationVisualElements? VisualElements { get; init; }

}

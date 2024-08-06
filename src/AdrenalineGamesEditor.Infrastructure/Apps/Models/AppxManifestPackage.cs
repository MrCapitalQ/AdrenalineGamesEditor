using System.Xml.Serialization;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps.Models;

[XmlRoot("Package", IsNullable = false)]
public class AppxManifestPackage
{
    [XmlArray("Applications")]
    [XmlArrayItem("Application")]
    public List<AppxManifestApplication> Applications { get; set; } = [];
}

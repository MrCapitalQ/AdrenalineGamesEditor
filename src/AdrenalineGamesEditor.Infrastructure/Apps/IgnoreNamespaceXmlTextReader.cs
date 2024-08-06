using System.Xml;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;

internal class IgnoreNamespaceXmlTextReader(string path) : XmlTextReader(path)
{
    public override string NamespaceURI { get => string.Empty; }
}


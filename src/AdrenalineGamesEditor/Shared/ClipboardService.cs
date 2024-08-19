using MrCapitalQ.AdrenalineGamesEditor.Core;
using System.Diagnostics.CodeAnalysis;
using Windows.ApplicationModel.DataTransfer;

namespace MrCapitalQ.AdrenalineGamesEditor.Shared;

[ExcludeFromCodeCoverage]
internal class ClipboardService : IClipboardService
{
    public virtual void SetText(string? text)
    {
        if (string.IsNullOrEmpty(text))
            return;

        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
    }
}

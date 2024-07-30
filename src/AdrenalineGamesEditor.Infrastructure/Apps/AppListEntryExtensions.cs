using Windows.ApplicationModel.Core;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;

internal static class AppListEntryExtensions
{
    public static string GetApplicationId(this AppListEntry appListEntry)
    {
        var appIdStartIndex = appListEntry.AppUserModelId.LastIndexOf('!') + 1;
        return appListEntry.AppUserModelId.AsSpan()[appIdStartIndex..].ToString();
    }
}

using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using Windows.ApplicationModel.Core;

namespace MrCapitalQ.AdrenalineGamesEditor.Infrastructure.Apps;

internal static class AppListEntryExtensions
{
    public static AppUserModelId GetAppUserModelId(this AppListEntry appListEntry)
        => AppUserModelId.Parse(appListEntry.AppUserModelId);
}

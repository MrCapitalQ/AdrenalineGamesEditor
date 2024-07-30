namespace MrCapitalQ.AdrenalineGamesEditor.Core.Apps;

public interface IPackagedAppsService
{
    Task<IEnumerable<PackagedAppListing>> GetAllAsync();
}

namespace MrCapitalQ.AdrenalineGamesEditor.Core.Apps;

public record AppUserModelId
{
    public AppUserModelId(string packageFamilyName, string packageAppId)
    {
        PackageFamilyName = packageFamilyName;
        PackageAppId = packageAppId;
        ValueAsString = $"{packageFamilyName}!{packageAppId}";
    }

    public string PackageFamilyName { get; }
    public string PackageAppId { get; }
    public string ValueAsString { get; }

    public static AppUserModelId Parse(string appUserModelId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(appUserModelId);

        var idSpan = appUserModelId.AsSpan();
        var separatorIndex = idSpan.LastIndexOf('!');
        if (separatorIndex <= 0 || (separatorIndex + 1) >= appUserModelId.Length)
            throw new FormatException($"The input string '{appUserModelId}' was not in a correct format.");

        return new(idSpan[..separatorIndex].ToString(), idSpan[(separatorIndex + 1)..].ToString());
    }
}

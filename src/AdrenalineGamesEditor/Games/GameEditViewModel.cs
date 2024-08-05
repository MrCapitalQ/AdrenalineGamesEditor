using CommunityToolkit.Mvvm.ComponentModel;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Shared;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GameEditViewModel : ObservableObject
{
    private readonly IPackagedAppsService _packagedAppsService;
    private readonly IQualifiedFileResolver _qualifiedFileResolver;

    [ObservableProperty]
    private string? _displayName;

    [ObservableProperty]
    private string? _command;

    [ObservableProperty]
    private Uri? _imagePath;

    [ObservableProperty]
    private string? _exePath;

    [ObservableProperty]
    private IEnumerable<ComboBoxOption<string>> _exePathOptions = [];

    [ObservableProperty]
    private ComboBoxOption<string>? _selectedExePathOption;

    public GameEditViewModel(IPackagedAppsService packagedAppsService,
        IQualifiedFileResolver qualifiedFileResolver,
        string appUserModelId)
    {
        _packagedAppsService = packagedAppsService;
        _qualifiedFileResolver = qualifiedFileResolver;

        _ = InitForAppUserModelId(appUserModelId);
    }


    private async Task InitForAppUserModelId(string appUserModelId)
    {
        var appInfo = await _packagedAppsService.GetInfoAsync(appUserModelId);
        if (appInfo is null)
            return;

        DisplayName = appInfo.DisplayName;
        Command = appUserModelId;

        var exePath = appInfo.ExecutablePath is not null
            ? Path.Combine(appInfo.InstalledPath, appInfo.ExecutablePath)
            : null;
        ExePathOptions = appInfo.ExecutablePaths
            .Select(x => new ComboBoxOption<string>(Path.Combine(appInfo.InstalledPath, x), $@"\{x}"))
            .ToList();

        SelectedExePathOption = ExePathOptions.FirstOrDefault(x => x.Value == exePath);

        if (appInfo.Square150x150Logo is not null)
            ImagePath = new Uri(_qualifiedFileResolver.GetPath(appInfo.InstalledPath, appInfo.Square150x150Logo));
    }

    partial void OnSelectedExePathOptionChanged(ComboBoxOption<string>? value)
    {
        if (value is not null)
            ExePath = value.Value;
        else
            ExePath = null;
    }
}

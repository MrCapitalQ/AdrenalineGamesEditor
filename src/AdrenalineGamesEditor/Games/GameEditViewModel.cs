using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.Logging;
using MrCapitalQ.AdrenalineGamesEditor.Core.Adrenaline;
using MrCapitalQ.AdrenalineGamesEditor.Core.Apps;
using MrCapitalQ.AdrenalineGamesEditor.Shared;

namespace MrCapitalQ.AdrenalineGamesEditor.Games;

internal partial class GameEditViewModel : ObservableObject
{
    private readonly IPackagedAppsService _packagedAppsService;
    private readonly IQualifiedFileResolver _qualifiedFileResolver;
    private readonly IAdrenalineGamesDataService _adrenalineGamesDataService;
    private readonly IMessenger _messenger;
    private readonly ILogger<GameEditViewModel> _logger;

    [ObservableProperty]
    private Guid _id;

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
        IAdrenalineGamesDataService adrenalineGamesDataService,
        IMessenger messenger,
        ILogger<GameEditViewModel> logger)
    {
        _packagedAppsService = packagedAppsService;
        _qualifiedFileResolver = qualifiedFileResolver;
        _adrenalineGamesDataService = adrenalineGamesDataService;
        _messenger = messenger;
        _logger = logger;
    }

    public GameEditViewModel(IPackagedAppsService packagedAppsService,
        IQualifiedFileResolver qualifiedFileResolver,
        IAdrenalineGamesDataService adrenalineGamesDataService,
        IMessenger messenger,
        ILogger<GameEditViewModel> logger,
        string appUserModelId)
    {
        _packagedAppsService = packagedAppsService;
        _qualifiedFileResolver = qualifiedFileResolver;
        _adrenalineGamesDataService = adrenalineGamesDataService;
        _messenger = messenger;
        _logger = logger;
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

    [RelayCommand]
    private async Task SaveAsync()
    {
        var gameInfo = new AdrenalineGameInfo(Id,
            DisplayName ?? string.Empty,
            ImagePath?.LocalPath ?? string.Empty,
            Command ?? string.Empty,
            ExePath ?? string.Empty,
            true);

        try
        {
            await _adrenalineGamesDataService.AddAsync(gameInfo);

            _messenger.Send(NavigateBackMessage.Instance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Something went wrong while saving a game entry. {GameInfo}", gameInfo);
        }
    }

    partial void OnSelectedExePathOptionChanged(ComboBoxOption<string>? value)
    {
        if (value is not null)
            ExePath = value.Value;
        else
            ExePath = null;
    }
}
